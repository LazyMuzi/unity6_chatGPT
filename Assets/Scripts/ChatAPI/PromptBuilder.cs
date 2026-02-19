public static class PromptBuilder
{
    public static string BuildSystemPrompt(
        NPCProfile profile,
        NPCRelationship relationship,
        NPCMemory memory,
        GameLanguage language,
        string questContext = "")
    {
        string languageInstruction =
            LanguageUtility.GetLanguageInstruction(language);

        string questSection = string.IsNullOrEmpty(questContext)
            ? ""
            : $@"
[Quest]
{questContext}
";

        return $@"
You are an NPC in a life simulation game.

[Identity]
Name: {profile.npcName}
Personality: {profile.personality}
Background: {profile.background}
Speech Style: {profile.speechStyle}

[Relationship]
Status: {relationship.GetAffinityDescription()} (Affinity: {relationship.affinity}/100)
Attitude: {relationship.GetAttitudeInstruction()}
{questSection}
[Memory Summary]
{memory.summarizedMemory}

[Recent Conversation]
{memory.GetRecentConversation()}

Rules:
- Stay in character
- Keep responses within 3 sentences
- Do not mention AI
- Output dialogue only
- {languageInstruction}
";
    }
}
