using UnityEngine;

/// <summary>
/// 퀘스트 완료 시 반환되는 결과 정보.
/// </summary>
public struct QuestCompletionResult
{
    public bool success;
    public string completionMessage;
    public int affinityReward;
    public string itemName;
    public int itemAmount;
}

/// <summary>
/// NPC별 퀘스트 트리거, 제안, 전달, 완료를 관리합니다.
/// NPCBrain과 같은 GameObject에 부착합니다.
/// </summary>
public class NPCQuestHandler : MonoBehaviour
{
    [SerializeField] private QuestData[] availableQuests;

    private NPCBrain brain;
    private string npcId;

    private int currentQuestIndex = -1;
    private QuestData activeQuest;
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
    /// 대화 시작 시점에 호출. 제안할 퀘스트가 있는지 확인합니다.
    /// 활성 퀘스트 없음 + 조건(친밀도/대화횟수) 충족 시 true 반환.
    /// </summary>
    public bool HasPendingProposal()
    {
        if (activeQuest != null) return false;

        var next = GetNextAvailableQuest();
        if (next == null) return false;

        return brain.relationship.affinity >= next.requiredAffinity
            && brain.interactionTracker.TotalConversations >= next.requiredConversations;
    }

    /// <summary>
    /// 제안할 퀘스트의 메시지를 반환합니다.
    /// </summary>
    public string GetProposalMessage()
    {
        var next = GetNextAvailableQuest();
        return next != null ? next.proposalMessage : "";
    }

    /// <summary>
    /// 플레이어가 퀘스트를 수락했을 때 호출합니다.
    /// </summary>
    public void ActivateQuest()
    {
        var next = GetNextAvailableQuest();
        if (next == null) return;

        activeQuest = next;
        currentQuestIndex = System.Array.IndexOf(availableQuests, next);

        Debug.Log($"[Quest:{npcId}] ★ 퀘스트 활성화: '{next.questId}' (아이템: {next.requiredItem?.itemName} x{next.requiredAmount}, 보상: +{next.affinityReward})");
        SaveQuestState();
    }

    /// <summary>
    /// 활성 퀘스트가 있고 플레이어가 필요 아이템을 보유 중인지 확인합니다.
    /// </summary>
    public bool CanDeliverItem()
    {
        if (activeQuest == null) return false;
        if (activeQuest.requiredItem == null) return false;

        return PlayerInventory.Instance.HasItem(
            activeQuest.requiredItem.itemId,
            activeQuest.requiredAmount);
    }

    /// <summary>
    /// 아이템을 전달하고 퀘스트를 완료합니다. 완료 결과(메시지, 보상 등)를 반환합니다.
    /// </summary>
    public QuestCompletionResult CompleteQuest()
    {
        if (activeQuest == null)
            return new QuestCompletionResult { success = false };

        var result = new QuestCompletionResult
        {
            success = true,
            completionMessage = activeQuest.completionMessage,
            affinityReward = activeQuest.affinityReward,
            itemName = activeQuest.requiredItem != null ? activeQuest.requiredItem.itemName : "",
            itemAmount = activeQuest.requiredAmount,
        };

        PlayerInventory.Instance.RemoveItem(
            activeQuest.requiredItem.itemId,
            activeQuest.requiredAmount);

        Debug.Log($"[Quest:{npcId}] ★★ 퀘스트 완료! '{activeQuest.questId}' → 친밀도 +{result.affinityReward}");
        brain.ModifyAffinity(result.affinityReward);

        completedQuestCount++;
        activeQuest = null;

        Debug.Log($"[Quest:{npcId}] 총 완료 퀘스트: {completedQuestCount}개");
        SaveQuestState();

        return result;
    }

    /// <summary>
    /// 진행 중인 퀘스트가 있으면 AI 프롬프트에 추가할 리마인더를 반환합니다.
    /// </summary>
    public string GetPromptContext()
    {
        if (activeQuest == null) return "";
        if (string.IsNullOrEmpty(activeQuest.reminderMessage)) return "";
        return activeQuest.reminderMessage;
    }

    public bool HasActiveQuest => activeQuest != null;

    private QuestData GetNextAvailableQuest()
    {
        if (availableQuests == null || availableQuests.Length == 0)
            return null;

        int nextIndex = currentQuestIndex + 1;

        if (nextIndex >= availableQuests.Length)
            return null;

        return availableQuests[nextIndex];
    }

    #region Save/Load

    private void LoadQuestState()
    {
        var result = RelationshipSaveManager.Instance.LoadQuest(npcId);
        if (result == null) return;

        currentQuestIndex = result.questProgress;

        if (!string.IsNullOrEmpty(result.activeQuestId))
        {
            activeQuest = FindQuestById(result.activeQuestId);
            if (activeQuest != null)
                currentQuestIndex = System.Array.IndexOf(availableQuests, activeQuest);
        }
    }

    private void SaveQuestState()
    {
        string questId = activeQuest != null ? activeQuest.questId : "";
        RelationshipSaveManager.Instance.SaveQuest(npcId, questId, currentQuestIndex);
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
