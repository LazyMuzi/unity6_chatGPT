using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 세션 단위 토큰 사용량을 추적합니다.
/// NPC별 누적 토큰과 일일 예산 관리를 제공합니다.
/// </summary>
public class TokenTracker
{
    public static TokenTracker Instance { get; private set; } = new();

    [System.Serializable]
    public struct UsageSnapshot
    {
        public int promptTokens;
        public int completionTokens;
        public int cachedTokens;
        public int TotalTokens => promptTokens + completionTokens;
    }

    private int sessionPromptTokens;
    private int sessionCompletionTokens;
    private int sessionCachedTokens;
    private int sessionRequestCount;

    private readonly Dictionary<string, int> perNpcTokens = new();

    private int dailyBudget = 100_000;
    private int todayTotalTokens;
    private string todayDate = "";

    public int DailyBudget
    {
        get => dailyBudget;
        set => dailyBudget = Mathf.Max(0, value);
    }

    /// <summary>
    /// 일일 예산이 남아있는지 확인합니다.
    /// </summary>
    public bool HasBudget()
    {
        RefreshDailyReset();
        return todayTotalTokens < dailyBudget;
    }

    /// <summary>
    /// API 응답에서 파싱된 사용량을 기록합니다.
    /// </summary>
    public void Record(string npcId, UsageSnapshot usage)
    {
        RefreshDailyReset();

        sessionPromptTokens += usage.promptTokens;
        sessionCompletionTokens += usage.completionTokens;
        sessionCachedTokens += usage.cachedTokens;
        sessionRequestCount++;

        todayTotalTokens += usage.TotalTokens;

        if (!string.IsNullOrEmpty(npcId))
        {
            perNpcTokens.TryGetValue(npcId, out int existing);
            perNpcTokens[npcId] = existing + usage.TotalTokens;
        }

        float costInput = usage.promptTokens * 0.15f / 1_000_000f;
        float costCached = usage.cachedTokens * 0.075f / 1_000_000f;
        float costOutput = usage.completionTokens * 0.60f / 1_000_000f;
        float totalCost = costInput + costCached + costOutput;

        string cachedInfo = usage.cachedTokens > 0
            ? $", cached:{usage.cachedTokens}"
            : "";

        Debug.Log($"[Token] {npcId}: {usage.promptTokens}in{cachedInfo} + {usage.completionTokens}out = ${totalCost:F5} | " +
                  $"session:{sessionPromptTokens + sessionCompletionTokens} ({sessionRequestCount}req) | " +
                  $"daily:{todayTotalTokens}/{dailyBudget}");
    }

    public int GetNpcTokens(string npcId)
    {
        perNpcTokens.TryGetValue(npcId, out int tokens);
        return tokens;
    }

    private void RefreshDailyReset()
    {
        string today = System.DateTime.Now.ToString("yyyy-MM-dd");
        if (todayDate != today)
        {
            todayDate = today;
            todayTotalTokens = 0;
        }
    }
}
