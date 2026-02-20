/// <summary>
/// AI 시스템 프롬프트를 구성합니다.
/// OpenAI 프롬프트 캐싱 최적화를 위해 정적 → 동적 순서로 배치합니다.
/// </summary>
public static class PromptBuilder
{
    public static string BuildSystemPrompt(
        NPCProfile profile,
        NPCRelationship relationship,
        NPCMemory memory,
        GameLanguage language,
        string questContext = "",
        int totalConversations = 0,
        int daysSinceLastConversation = -1,
        int consecutiveDays = 0)
    {
        string languageInstruction =
            LanguageUtility.GetLanguageInstruction(language);

        string questSection = string.IsNullOrEmpty(questContext)
            ? ""
            : $"\n[Quest]\n{questContext}\n";

        string historyContext = BuildHistoryContext(
            totalConversations, daysSinceLastConversation, consecutiveDays);

        string memorySection = string.IsNullOrEmpty(memory.summarizedMemory)
            ? ""
            : $"\n[Memory Summary]\n{memory.summarizedMemory}\n";

        string recentSection = memory.HasRecentDialogue()
            ? $"\n[Recent Conversation]\n{memory.GetRecentConversation()}\n"
            : "";

        // --- 캐싱 최적화: 정적(Rules+Identity) → 준정적(관계) → 동적(메모리/대화) ---
        return $@"[Rules]
- Stay in character at all times
- Keep responses within 3 sentences
- Output dialogue only — no narration, no actions
- Do not mention AI or break the fourth wall
- Do not fabricate memories or claim to remember things not in conversation history
- Naturally reflect the passage of time without stating exact day counts
- {languageInstruction}

[Identity]
You are an NPC in a life simulation game.
Name: {profile.npcName}
Personality: {profile.personality}
Background: {profile.background}
Speech Style: {profile.speechStyle}

[Relationship]
Status: {relationship.GetAffinityDescription()} (Affinity: {relationship.affinity}/100)
Attitude: {relationship.GetAttitudeInstruction()}
{historyContext}
{questSection}{memorySection}{recentSection}";
    }

    private static string BuildHistoryContext(
        int totalConversations, int daysSince, int consecutiveDays)
    {
        if (totalConversations == 0)
            return "History: First meeting. You have never spoken to this player before. Do not act familiar.";

        string history = $"History: {totalConversations} past conversation(s).";

        if (daysSince == 0)
            history += " Met again today.";
        else if (daysSince == 1 && consecutiveDays >= 2)
            history += $" Visiting {consecutiveDays} days in a row.";
        else if (daysSince >= 30)
            history += $" Absent for {daysSince} days — very long. You feel worried or relieved.";
        else if (daysSince >= 7)
            history += $" Absent for {daysSince} days — quite a while.";
        else if (daysSince >= 2)
            history += $" {daysSince} days since last visit.";

        return history;
    }
}
