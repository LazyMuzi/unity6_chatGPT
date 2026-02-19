using UnityEngine;

/// <summary>
/// NPC별 퀘스트 트리거, 진행, 완료를 관리합니다.
/// NPCBrain과 같은 GameObject에 부착합니다.
/// </summary>
public class NPCQuestHandler : MonoBehaviour
{
    [SerializeField] private QuestData[] availableQuests;

    private NPCBrain brain;
    private string npcId;

    private int currentQuestIndex = -1;
    private QuestData activeQuest;
    private int questProgress;

    private bool shouldProposeQuest;
    private bool shouldCompleteQuest;
    private int completedQuestCount;

    private void Awake()
    {
        brain = GetComponent<NPCBrain>();
    }

    private void Start()
    {
        npcId = brain.profile.npcId;
        LoadQuestState();
    }

    /// <summary>
    /// 대화 시작 시 호출. 퀘스트 상태에 따른 AI 프롬프트 컨텍스트를 반환합니다.
    /// </summary>
    public string GetPromptContext(NPCInteractionTracker tracker)
    {
        if (shouldCompleteQuest && activeQuest != null)
            return activeQuest.completionDialogue;

        if (shouldProposeQuest && activeQuest == null)
        {
            QuestData next = GetNextAvailableQuest(tracker);
            if (next != null)
                return next.questDialogue;
        }

        if (activeQuest != null)
        {
            return BuildActiveQuestReminder();
        }

        return "";
    }

    /// <summary>
    /// 대화 종료 시 NPCBrain.OnConversationEnd()에서 호출됩니다.
    /// </summary>
    public void OnConversationRecorded()
    {
        if (shouldCompleteQuest && activeQuest != null)
        {
            CompleteQuest();
            return;
        }

        if (shouldProposeQuest && activeQuest == null)
        {
            QuestData next = GetNextAvailableQuest(brain.interactionTracker);
            if (next != null)
                ActivateQuest(next);
            return;
        }

        if (activeQuest != null)
            ProgressActiveQuest();

        CheckTriggerConditions(brain.interactionTracker);
    }

    private void CheckTriggerConditions(NPCInteractionTracker tracker)
    {
        if (activeQuest != null) return;

        QuestData next = GetNextAvailableQuest(tracker);
        if (next != null && tracker.TotalConversations >= next.conversationThreshold)
        {
            shouldProposeQuest = true;
            Debug.Log($"[Quest:{npcId}] 퀘스트 트리거! 다음 대화에서 '{next.questId}' 제안 예정 (누적대화: {tracker.TotalConversations} >= 임계값: {next.conversationThreshold})");
        }
    }

    private QuestData GetNextAvailableQuest(NPCInteractionTracker tracker)
    {
        if (availableQuests == null || availableQuests.Length == 0)
            return null;

        int nextIndex = currentQuestIndex + 1;

        if (nextIndex >= availableQuests.Length)
            nextIndex = nextIndex % availableQuests.Length;

        var candidate = availableQuests[nextIndex];
        if (candidate == null) return null;

        int threshold = candidate.conversationThreshold;
        if (completedQuestCount > 0 && nextIndex <= currentQuestIndex)
            threshold = tracker.TotalConversations;

        if (tracker.TotalConversations >= threshold)
            return candidate;

        return null;
    }

    private void ActivateQuest(QuestData quest)
    {
        activeQuest = quest;
        questProgress = 0;

        currentQuestIndex = System.Array.IndexOf(availableQuests, quest);
        shouldProposeQuest = false;

        if (quest.type == QuestType.ConsecutiveVisit)
            questProgress = brain.interactionTracker.ConsecutiveDays >= 1 ? 1 : 0;

        Debug.Log($"[Quest:{npcId}] ★ 퀘스트 활성화: '{quest.questId}' ({quest.type}, 필요: {quest.requiredCount}, 보상: +{quest.affinityReward})");
        SaveQuestState();
    }

    private void ProgressActiveQuest()
    {
        if (activeQuest == null) return;

        switch (activeQuest.type)
        {
            case QuestType.VisitAgain:
                string today = System.DateTime.Now.ToString("yyyy-MM-dd");
                string lastDate = brain.interactionTracker.LastConversationDate;
                if (today != lastDate || questProgress == 0)
                    questProgress++;
                break;

            case QuestType.TalkMultiple:
                questProgress++;
                break;

            case QuestType.ConsecutiveVisit:
                questProgress = brain.interactionTracker.ConsecutiveDays;
                break;
        }

        Debug.Log($"[Quest:{npcId}] 퀘스트 진행: '{activeQuest.questId}' ({questProgress}/{activeQuest.requiredCount})");

        if (questProgress >= activeQuest.requiredCount)
        {
            shouldCompleteQuest = true;
            Debug.Log($"[Quest:{npcId}] ✓ 퀘스트 조건 달성! 다음 대화에서 완료 처리 예정");
        }

        SaveQuestState();
    }

    private void CompleteQuest()
    {
        Debug.Log($"[Quest:{npcId}] ★★ 퀘스트 완료! '{activeQuest.questId}' → 친밀도 +{activeQuest.affinityReward}");

        int reward = activeQuest.affinityReward;
        brain.ModifyAffinity(reward);

        completedQuestCount++;
        activeQuest = null;
        questProgress = 0;
        shouldCompleteQuest = false;
        shouldProposeQuest = false;

        Debug.Log($"[Quest:{npcId}] 총 완료 퀘스트: {completedQuestCount}개");
        SaveQuestState();
    }

    private string BuildActiveQuestReminder()
    {
        if (activeQuest == null) return "";

        int remaining = activeQuest.requiredCount - questProgress;
        return activeQuest.type switch
        {
            QuestType.VisitAgain =>
                $"[Quest in progress: The player promised to visit again. {remaining} more visit(s) needed.]",
            QuestType.TalkMultiple =>
                $"[Quest in progress: The player promised to talk more. {remaining} more conversation(s) needed.]",
            QuestType.ConsecutiveVisit =>
                $"[Quest in progress: The player promised to visit consecutively. {remaining} more consecutive day(s) needed.]",
            _ => ""
        };
    }

    #region Save/Load

    private void LoadQuestState()
    {
        var result = RelationshipSaveManager.Instance.LoadQuest(npcId);
        if (result == null) return;

        questProgress = result.questProgress;

        if (!string.IsNullOrEmpty(result.activeQuestId))
        {
            activeQuest = FindQuestById(result.activeQuestId);
            if (activeQuest != null)
            {
                currentQuestIndex = System.Array.IndexOf(availableQuests, activeQuest);
                ProgressActiveQuest();
            }
        }
    }

    private void SaveQuestState()
    {
        string questId = activeQuest != null ? activeQuest.questId : "";
        RelationshipSaveManager.Instance.SaveQuest(npcId, questId, questProgress);
    }

    private QuestData FindQuestById(string questId)
    {
        if (availableQuests == null) return null;

        foreach (var quest in availableQuests)
        {
            if (quest != null && quest.questId == questId)
                return quest;
        }

        return null;
    }

    #endregion
}
