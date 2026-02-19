using UnityEngine;

public class NPCBrain : MonoBehaviour
{
    public NPCProfile profile;
    public NPCRelationship relationship = new NPCRelationship();
    public NPCMemory memory = new NPCMemory();
    public GameLanguage currentLanguage = GameLanguage.Korean;

    [HideInInspector] public NPCInteractionTracker interactionTracker;
    [HideInInspector] public NPCQuestHandler questHandler;

    private void Start()
    {
        if (RelationshipSaveManager.Instance.TryLoad(profile.npcId, out int saved))
            relationship.affinity = saved;

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
    /// 대화 세션 종료 시 호출. 친밀도 소량 증가 + 대화 기록 갱신 + 퀘스트 진행.
    /// </summary>
    public void OnConversationEnd()
    {
        int gain = Random.Range(1, 3);
        Debug.Log($"[NPC:{profile.npcName}] ── 대화 종료 ── 친밀도 +{gain} 적용");
        ModifyAffinity(gain);
        interactionTracker.RecordConversation();

        if (questHandler != null)
            questHandler.OnConversationRecorded();
    }

    /// <summary>
    /// 대화 시작 시 호출. 퀘스트 제안/완료 여부에 따라 추가 프롬프트 컨텍스트를 반환합니다.
    /// </summary>
    public string GetQuestContext()
    {
        if (questHandler == null) return "";
        return questHandler.GetPromptContext(interactionTracker);
    }

    public void HandlePlayerInteraction(string playerInput)
    {
        memory.AddDialogue("Player: " + playerInput);

        string questContext = GetQuestContext();

        AIManager.Instance.RequestResponse(
            profile,
            relationship,
            memory,
            playerInput,
            currentLanguage,
            ProcessResponse,
            questContext
        );
    }

    private void ProcessResponse(string response)
    {
        memory.AddDialogue(profile.npcName + ": " + response);
        ChatUI.Instance?.AddNPCResponse(profile.npcName, response);
    }
}