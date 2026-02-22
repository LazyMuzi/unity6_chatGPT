using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 친밀도 범위와 쿨다운을 기준으로 QuestPool에서 제안 가능한 퀘스트를 선택합니다.
/// MonoBehaviour가 아닌 순수 로직 클래스입니다.
/// </summary>
public class QuestGenerator
{
    public QuestData GetAvailableQuest(
        QuestPool pool,
        int affinity,
        Dictionary<string, float> lastQuestTimes)
    {
        if (pool == null || pool.quests == null || pool.quests.Count == 0)
            return null;

        float currentTime = Time.time;

        var available = pool.quests.Where(q =>
            q != null &&
            affinity >= q.minAffinity &&
            affinity <= q.maxAffinity &&
            (!lastQuestTimes.ContainsKey(q.questId) ||
             currentTime - lastQuestTimes[q.questId] > q.cooldown)
        ).ToList();

        if (available.Count == 0)
            return null;

        return available[Random.Range(0, available.Count)];
    }
}
