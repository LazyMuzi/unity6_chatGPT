using UnityEngine;

public enum QuestType
{
    FetchItem
}

/// <summary>
/// NPC가 플레이어에게 제안하는 퀘스트 데이터입니다.
/// </summary>
[CreateAssetMenu(menuName = "Quest/QuestData")]
public class QuestData : ScriptableObject
{
    public string questId;
    public QuestType type;
    public int affinityReward = 10;

    [Header("Trigger Conditions")]
    [Tooltip("퀘스트가 제안되기 위한 최소 친밀도")]
    public int requiredAffinity;

    [Tooltip("퀘스트가 제안되기 위한 최소 누적 대화 횟수")]
    public int requiredConversations;

    [Header("FetchItem")]
    public ItemData requiredItem;
    public int requiredAmount = 1;

    [Header("Dialogue")]
    [TextArea(2, 4)]
    [Tooltip("퀘스트 제안 시 NPC가 하는 말 (인사 대신 표시)")]
    public string proposalMessage;

    [TextArea(2, 4)]
    [Tooltip("아이템 전달 시 NPC 반응")]
    public string completionMessage;

    [TextArea(2, 4)]
    [Tooltip("퀘스트 진행 중 AI 프롬프트에 추가되는 컨텍스트")]
    public string reminderMessage;
}
