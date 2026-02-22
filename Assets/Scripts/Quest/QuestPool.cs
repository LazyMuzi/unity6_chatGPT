using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NPC별 퀘스트 후보 목록. NPC는 이 풀을 참조하여 퀘스트를 제안합니다.
/// </summary>
[CreateAssetMenu(fileName = "QuestPool", menuName = "Game/Quest Pool")]
public class QuestPool : ScriptableObject
{
    public string npcId;
    public List<QuestData> quests;
}
