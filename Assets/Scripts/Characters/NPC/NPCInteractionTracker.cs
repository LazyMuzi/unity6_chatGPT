using System;
using UnityEngine;

/// <summary>
/// NPC별 대화 기록을 추적합니다. 퀘스트 트리거 조건 판단에 사용됩니다.
/// </summary>
[System.Serializable]
public class NPCInteractionTracker
{
    private string npcId;

    public int TotalConversations { get; private set; }
    public string LastConversationDate { get; private set; } = "";
    public int ConsecutiveDays { get; private set; }

    public NPCInteractionTracker(string npcId)
    {
        this.npcId = npcId;
        LoadFromSave();
        Debug.Log($"[Tracker:{npcId}] 로드 완료 | 누적대화: {TotalConversations}, 연속일수: {ConsecutiveDays}, 마지막대화: {LastConversationDate}");
    }

    /// <summary>
    /// 대화 세션 완료 시 호출합니다. 횟수 증가 및 연속 일수를 계산합니다.
    /// </summary>
    public void RecordConversation()
    {
        TotalConversations++;

        string today = DateTime.Now.ToString("yyyy-MM-dd");

        if (LastConversationDate != today)
        {
            if (IsYesterday(LastConversationDate))
                ConsecutiveDays++;
            else
                ConsecutiveDays = 1;

            LastConversationDate = today;
        }

        Debug.Log($"[Tracker:{npcId}] 대화 기록 | 누적대화: {TotalConversations}, 연속일수: {ConsecutiveDays}, 날짜: {LastConversationDate}");
        SaveToStore();
    }

    private bool IsYesterday(string dateStr)
    {
        if (string.IsNullOrEmpty(dateStr)) return false;

        if (DateTime.TryParse(dateStr, out DateTime lastDate))
        {
            return (DateTime.Now.Date - lastDate.Date).Days == 1;
        }

        return false;
    }

    private void LoadFromSave()
    {
        var data = RelationshipSaveManager.Instance.LoadInteraction(npcId);
        if (data == null) return;

        TotalConversations = data.totalConversations;
        LastConversationDate = data.lastConversationDate ?? "";
        ConsecutiveDays = data.consecutiveDays;
    }

    private void SaveToStore()
    {
        RelationshipSaveManager.Instance.SaveInteraction(npcId,
            TotalConversations, LastConversationDate, ConsecutiveDays);
    }
}
