using UnityEngine;

public enum QuestType
{
    Fetch,
    Assist,
    Dialogue,
    Event
}

/// <summary>
/// 퀘스트 템플릿 데이터. 런타임 상태는 ActiveQuest에서 관리합니다.
/// </summary>
[CreateAssetMenu(fileName = "QuestData", menuName = "Game/Quest")]
public class QuestData : ScriptableObject
{
    public string questId;
    public QuestType type;

    [TextArea]
    public string description;

    public int rewardAffinity;

    [Header("Availability")]
    public int minAffinity;
    public int maxAffinity;

    [Tooltip("완료 후 다시 제안되기까지 걸리는 시간(초)")]
    public float cooldown;

    [Header("Fetch")]
    public ItemData requiredItem;
    public int requiredAmount = 1;

    [Header("Dialogue")]
    [TextArea(2, 4)]
    public string proposalMessage;

    [TextArea(2, 4)]
    public string completionMessage;

    [TextArea(2, 4)]
    public string reminderMessage;
}
