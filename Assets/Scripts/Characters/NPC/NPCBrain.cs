using UnityEngine;

public class NPCBrain : MonoBehaviour
{
    private static readonly string[] DefaultQuestRequestKeywords =
    {
        "퀘스트",
        "의뢰",
        "임무",
        "부탁",
        "할 일",
        "quest"
    };

    public NPCProfile profile;
    public NPCRelationship relationship = new NPCRelationship();
    public NPCMemory memory = new NPCMemory();
    public GameLanguage currentLanguage = GameLanguage.Korean;

    [Header("Quest Dialogue")]
    [SerializeField] private string noAvailableQuestMessage = "지금은 줄 수 있는 퀘스트가 없어.";
    [SerializeField] private string[] questRequestKeywords = DefaultQuestRequestKeywords;

    [HideInInspector] public NPCInteractionTracker interactionTracker;
    [HideInInspector] public NPCQuestHandler questHandler;

    private void Start()
    {
        if (RelationshipSaveManager.Instance.TryLoad(profile.npcId, out int saved))
            relationship.affinity = saved;

        memory.summarizedMemory = RelationshipSaveManager.Instance.LoadMemory(profile.npcId);

        interactionTracker = new NPCInteractionTracker(profile.npcId);
        questHandler = GetComponent<NPCQuestHandler>();
    }

    public void ModifyAffinity(int amount)
    {
        int before = relationship.affinity;
        relationship.ModifyAffinity(amount);
        RelationshipSaveManager.Instance.Save(profile.npcId, relationship.affinity);
        Debug.Log($"[NPC:{profile.npcName}] 친밀도 {before} → {relationship.affinity} ({(amount >= 0 ? "+" : "")}{amount})");
    }

    /// <summary>
    /// 대화 세션 종료 시 호출. 친밀도 소량 증가 + 대화 기록 갱신 + 메모리 요약 저장.
    /// </summary>
    public void OnConversationEnd()
    {
        int remaining = interactionTracker.GetRemainingDailyAffinityGain();
        int gain = Mathf.Min(Random.Range(1, 3), remaining);

        if (gain > 0)
        {
            Debug.Log($"[NPC:{profile.npcName}] ── 대화 종료 ── 친밀도 +{gain} 적용 (일일 잔여: {remaining - gain})");
            ModifyAffinity(gain);
            interactionTracker.RecordAffinityGain(gain);
        }
        else
        {
            Debug.Log($"[NPC:{profile.npcName}] ── 대화 종료 ── 일일 대화 친밀도 상한 도달");
        }

        interactionTracker.RecordConversation();

        memory.SummarizeCurrentSession();
        RelationshipSaveManager.Instance.SaveMemory(profile.npcId, memory.summarizedMemory);
        memory.ClearRecentDialogues();
    }

    public void HandlePlayerInteraction(string playerInput)
    {
        memory.AddDialogue("Player: " + playerInput);

        if (TryHandleQuestRequest(playerInput))
            return;

        if (ShouldUseLocalResponse())
        {
            string local = relationship.GetLocalResponse();
            Debug.Log($"[NPC:{profile.npcName}] 로컬 응답 사용 (예산/쿨다운)");
            ProcessResponse(local);
            return;
        }

        string questContext = questHandler != null ? questHandler.GetPromptContext() : "";

        AIManager.Instance.RequestResponse(
            profile,
            relationship,
            memory,
            playerInput,
            currentLanguage,
            ProcessResponse,
            questContext,
            interactionTracker.TotalConversations,
            interactionTracker.GetDaysSinceLastConversation(),
            interactionTracker.ConsecutiveDays
        );
    }

    private bool TryHandleQuestRequest(string playerInput)
    {
        if (questHandler == null) return false;
        if (!IsQuestRequest(playerInput)) return false;

        if (questHandler.HasPendingProposal())
        {
            string proposal = questHandler.GetProposalMessage();
            memory.AddDialogue(profile.npcName + ": " + proposal);
            ChatUI.Instance?.OpenQuestProposal(profile.npcName, proposal);
            return true;
        }

        ProcessResponse(noAvailableQuestMessage);
        return true;
    }

    private bool IsQuestRequest(string playerInput)
    {
        if (string.IsNullOrWhiteSpace(playerInput))
            return false;

        string normalizedInput = playerInput.ToLowerInvariant();
        if (questRequestKeywords == null || questRequestKeywords.Length == 0)
            return false;

        foreach (string keyword in questRequestKeywords)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                continue;

            if (normalizedInput.Contains(keyword.ToLowerInvariant()))
                return true;
        }

        return false;
    }

    /// <summary>
    /// 일일 토큰 예산 초과 시 로컬 응답으로 대체합니다.
    /// </summary>
    private bool ShouldUseLocalResponse()
    {
        return !TokenTracker.Instance.HasBudget();
    }

    private void ProcessResponse(string response)
    {
        memory.AddDialogue(profile.npcName + ": " + response);
        ChatUI.Instance?.AddNPCResponse(profile.npcName, response);
    }
}
