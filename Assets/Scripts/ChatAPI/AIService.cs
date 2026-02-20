using UnityEngine;
using System;
using System.Collections.Generic;
using Proyecto26;
using Newtonsoft.Json;

public class AIService : MonoBehaviour
{
    private const string PlaceholderKey = "REPLACE_WITH_REAL_KEY";

    [Header("API Settings")]
    [SerializeField] private string apiUrl = "https://api.openai.com/v1/chat/completions";
    [SerializeField] private string apiKey = PlaceholderKey;

    [SerializeField] private string model = "gpt-4o-mini";
    [SerializeField] private int maxCompletionTokens = 120;
    [SerializeField] private float temperature = 0.7f;

    public bool IsApiKeyValid =>
        !string.IsNullOrWhiteSpace(apiKey) && apiKey != PlaceholderKey;

    /// <param name="npcId">토큰 추적용 NPC 식별자. 빈 문자열이면 추적만 스킵.</param>
    public void GetChatResponse(
        string systemPrompt,
        string userInput,
        Action<string> callback,
        string npcId = "")
    {
        if (!IsApiKeyValid)
        {
            Debug.LogWarning("[AIService] API 키가 설정되지 않았습니다. Inspector에서 AIService의 apiKey를 입력해주세요.");
            callback?.Invoke("......");
            return;
        }

        var requestData = new ChatRequest
        {
            model = model,
            messages = new List<Message>
            {
                new Message { role = "system", content = systemPrompt },
                new Message { role = "user", content = userInput }
            },
            max_completion_tokens = maxCompletionTokens,
            temperature = temperature
        };

        string jsonData = JsonConvert.SerializeObject(requestData);

        RestClient.Request(new RequestHelper
        {
            Uri = apiUrl,
            Method = "POST",
            BodyString = jsonData,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Authorization", $"Bearer {apiKey}" }
            }
        })
        .Then(response =>
        {
            try
            {
                var chatResponse =
                    JsonConvert.DeserializeObject<ChatResponse>(response.Text);

                TrackUsage(npcId, chatResponse);

                if (chatResponse?.choices != null &&
                    chatResponse.choices.Count > 0)
                {
                    callback?.Invoke(
                        chatResponse.choices[0].message.content.Trim()
                    );
                }
                else
                {
                    callback?.Invoke("No valid response.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Parsing Error: " + e.Message);
                callback?.Invoke("Parsing Error");
            }
        })
        .Catch(error =>
        {
            Debug.LogError("API Error: " + error.Message);
            callback?.Invoke("API Error: " + error.Message);
        });
    }

    private void TrackUsage(string npcId, ChatResponse response)
    {
        if (response?.usage == null) return;

        var snapshot = new TokenTracker.UsageSnapshot
        {
            promptTokens = response.usage.prompt_tokens,
            completionTokens = response.usage.completion_tokens,
            cachedTokens = response.usage.prompt_tokens_details?.cached_tokens ?? 0
        };

        TokenTracker.Instance.Record(npcId, snapshot);
    }

    #region API Data Models

    [Serializable]
    public class Message
    {
        public string role;
        public string content;
    }

    [Serializable]
    public class ChatRequest
    {
        public string model;
        public List<Message> messages;
        public int max_completion_tokens;
        public float temperature;
    }

    [Serializable]
    public class ChatResponse
    {
        public List<Choice> choices;
        public Usage usage;
    }

    [Serializable]
    public class Choice
    {
        public Message message;
    }

    [Serializable]
    public class Usage
    {
        public int prompt_tokens;
        public int completion_tokens;
        public int total_tokens;
        public PromptTokensDetails prompt_tokens_details;
    }

    [Serializable]
    public class PromptTokensDetails
    {
        public int cached_tokens;
    }

    #endregion
}