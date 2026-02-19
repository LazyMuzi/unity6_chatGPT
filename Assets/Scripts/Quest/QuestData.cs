using UnityEngine;

public enum QuestType
{
    VisitAgain,
    TalkMultiple,
    ConsecutiveVisit
}

/// <summary>
/// NPC가 플레이어에게 제안하는 퀘스트 데이터입니다.
/// </summary>
[CreateAssetMenu(menuName = "Quest/QuestData")]
public class QuestData : ScriptableObject
{
    public string questId;
    public QuestType type;

    [Tooltip("완료에 필요한 횟수 (방문 횟수, 대화 횟수, 연속 일수 등)")]
    public int requiredCount = 1;

    [Tooltip("완료 시 친밀도 보상")]
    public int affinityReward = 10;

    [Tooltip("이 퀘스트가 트리거되기 위한 최소 누적 대화 횟수")]
    public int conversationThreshold = 3;

    [TextArea(2, 4)]
    [Tooltip("퀘스트 제안 시 AI 프롬프트에 추가되는 지시문")]
    public string questDialogue;

    [TextArea(2, 4)]
    [Tooltip("퀘스트 완료 시 AI 프롬프트에 추가되는 지시문")]
    public string completionDialogue;
}
