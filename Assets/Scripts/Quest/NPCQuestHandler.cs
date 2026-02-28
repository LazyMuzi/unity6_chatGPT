using System.Collections.Generic;
using System;
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
/// NPC별 퀘스트 제안, 활성화, 전달, 완료를 관리합니다.
/// QuestPool을 참조하고 QuestGenerator로 조건 기반 퀘스트를 선택합니다.
/// </summary>
public class NPCQuestHandler : MonoBehaviour
{
    [SerializeField] private QuestPool questPool;

    private NPCBrain brain;
    private string npcId;

    private readonly QuestGenerator generator = new();
    private ActiveQuest activeQuest;
    private Dictionary<string, double> lastQuestTimes = new();

    private QuestData pendingProposal;

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
    /// 활성 퀘스트 없음 + 친밀도 범위 + 쿨다운 충족 시 true 반환.
    /// </summary>
    public bool HasPendingProposal()
    {
        if (activeQuest != null) return false;

        int affinity = brain.relationship.affinity;
        pendingProposal = generator.GetAvailableQuest(questPool, affinity, lastQuestTimes);

        return pendingProposal != null;
    }

    /// <summary>
    /// 제안할 퀘스트의 메시지를 반환합니다.
    /// </summary>
    public string GetProposalMessage()
    {
        return pendingProposal != null ? pendingProposal.proposalMessage : "";
    }

    /// <summary>
    /// 플레이어가 퀘스트를 수락했을 때 호출합니다.
    /// </summary>
    public void ActivateQuest()
    {
        if (pendingProposal == null) return;

        activeQuest = new ActiveQuest(pendingProposal);

        Debug.Log($"[Quest:{npcId}] Quest activated: '{pendingProposal.questId}' " +
                  $"(item: {pendingProposal.requiredItem?.itemName} x{pendingProposal.requiredAmount}, " +
                  $"reward: +{pendingProposal.rewardAffinity})");

        pendingProposal = null;
        SaveQuestState();
    }

    /// <summary>
    /// 활성 퀘스트가 있고 플레이어가 필요 아이템을 보유 중인지 확인합니다.
    /// </summary>
    public bool CanDeliverItem()
    {
        if (activeQuest == null) return false;

        var data = activeQuest.data;
        if (data.requiredItem == null) return false;

        return PlayerInventory.Instance.HasItem(
            data.requiredItem.itemId,
            data.requiredAmount);
    }

    /// <summary>
    /// 아이템을 전달하고 퀘스트를 완료합니다. 완료 결과를 반환합니다.
    /// </summary>
    public QuestCompletionResult CompleteQuest()
    {
        if (activeQuest == null)
            return new QuestCompletionResult { success = false };

        var data = activeQuest.data;

        var result = new QuestCompletionResult
        {
            success = true,
            completionMessage = data.completionMessage,
            affinityReward = data.rewardAffinity,
            itemName = data.requiredItem != null ? data.requiredItem.itemName : "",
            itemAmount = data.requiredAmount,
        };

        PlayerInventory.Instance.RemoveItem(
            data.requiredItem.itemId,
            data.requiredAmount);

        Debug.Log($"[Quest:{npcId}] Quest completed: '{data.questId}' -> affinity +{result.affinityReward}");
        brain.ModifyAffinity(result.affinityReward);

        lastQuestTimes[data.questId] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        activeQuest = null;

        SaveQuestState();

        return result;
    }

    /// <summary>
    /// 진행 중인 퀘스트가 있으면 AI 프롬프트에 추가할 리마인더를 반환합니다.
    /// </summary>
    public string GetPromptContext()
    {
        if (activeQuest == null || string.IsNullOrEmpty(activeQuest.data.reminderMessage))
            return "";
        return activeQuest.data.reminderMessage;
    }

    /// <summary>
    /// 활성 퀘스트의 리마인더 메시지를 UI 표시용으로 반환합니다.
    /// </summary>
    public bool TryGetActiveQuestReminder(out string reminderMessage)
    {
        reminderMessage = "";
        if (activeQuest == null || string.IsNullOrEmpty(activeQuest.data.reminderMessage))
            return false;

        reminderMessage = activeQuest.data.reminderMessage;
        return true;
    }

    public bool HasActiveQuest => activeQuest != null;

    #region Save/Load

    private void LoadQuestState()
    {
        var result = RelationshipSaveManager.Instance.LoadQuest(npcId);
        if (result == null) return;

        lastQuestTimes = result.lastQuestTimes ?? new Dictionary<string, double>();

        if (!string.IsNullOrEmpty(result.activeQuestId))
        {
            var questData = FindQuestById(result.activeQuestId);
            if (questData != null)
                activeQuest = new ActiveQuest(questData);
        }
    }

    private void SaveQuestState()
    {
        string questId = activeQuest != null ? activeQuest.data.questId : "";
        RelationshipSaveManager.Instance.SaveQuest(npcId, questId, lastQuestTimes);
    }

    private QuestData FindQuestById(string questId)
    {
        if (questPool == null || questPool.quests == null) return null;

        foreach (var quest in questPool.quests)
        {
            if (quest != null && quest.questId == questId)
                return quest;
        }

        return null;
    }

    #endregion
}
