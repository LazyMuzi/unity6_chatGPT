using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// NPC별 대화 기록과 요약 메모리를 관리합니다.
/// recentDialogues: 현재 세션의 최근 대화 (최대 5턴)
/// summarizedMemory: 과거 세션 요약 (영구 저장, 최대 300자)
/// </summary>
[System.Serializable]
public class NPCMemory
{
    private const int MaxRecentDialogues = 5;
    private const int MaxSummaryLength = 300;

    public List<string> recentDialogues = new();
    public string summarizedMemory = "";

    public void AddDialogue(string dialogue)
    {
        recentDialogues.Add(dialogue);

        if (recentDialogues.Count > MaxRecentDialogues)
            recentDialogues.RemoveAt(0);
    }

    public bool HasRecentDialogue() => recentDialogues.Count > 0;

    public string GetRecentConversation()
    {
        return string.Join("\n", recentDialogues);
    }

    /// <summary>
    /// 현재 세션의 대화에서 플레이어 발화를 추출하여 요약에 추가합니다.
    /// 대화 세션 종료 시 호출합니다. AI 호출 없이 로컬에서 처리합니다.
    /// </summary>
    public void SummarizeCurrentSession()
    {
        if (recentDialogues.Count == 0) return;

        var playerLines = new List<string>();
        foreach (var line in recentDialogues)
        {
            if (line.StartsWith("Player:"))
                playerLines.Add(line.Substring("Player:".Length).Trim());
        }

        if (playerLines.Count == 0) return;

        string date = DateTime.Now.ToString("M/d");
        string topics = string.Join(", ", playerLines);

        if (topics.Length > 80)
            topics = topics.Substring(0, 77) + "...";

        string entry = $"{date}: {topics}";
        AppendToSummary(entry);
    }

    private void AppendToSummary(string entry)
    {
        if (string.IsNullOrEmpty(summarizedMemory))
        {
            summarizedMemory = entry;
            return;
        }

        string combined = summarizedMemory + "\n" + entry;

        while (combined.Length > MaxSummaryLength)
        {
            int firstNewline = combined.IndexOf('\n');
            if (firstNewline < 0) break;
            combined = combined.Substring(firstNewline + 1);
        }

        summarizedMemory = combined;
    }

    /// <summary>
    /// 세션 시작 시 이전 대화 기록을 초기화합니다.
    /// </summary>
    public void ClearRecentDialogues()
    {
        recentDialogues.Clear();
    }
}