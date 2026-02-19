using UnityEngine;
using System;

public class AIManager : MonoBehaviour
{
    public static AIManager Instance;

    [SerializeField] private AIService chatHelper;

    private void Awake()
    {
        Instance = this;
    }

    public void RequestResponse(
        NPCProfile profile,
        NPCRelationship relationship,
        NPCMemory memory,
        string userInput,
        GameLanguage language,
        Action<string> callback,
        string questContext = "")
    {
        string systemPrompt =
            PromptBuilder.BuildSystemPrompt(
                profile,
                relationship,
                memory,
                language,
                questContext
            );

        chatHelper.GetChatResponse(systemPrompt, userInput, callback);
    }
}
