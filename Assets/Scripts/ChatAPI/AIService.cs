using UnityEngine;
using System;
using System.Collections.Generic;
using Proyecto26;
using Newtonsoft.Json;

public class AIService : MonoBehaviour
{
    [Header("API Settings")]
    [SerializeField] private string apiUrl = "https://api.openai.com/v1/chat/completions";
    [SerializeField] private string apiKey = "REPLACE_WITH_REAL_KEY";

    [SerializeField] private string model = "gpt-4o-mini";
    [SerializeField] private int maxCompletionTokens = 120;
    [SerializeField] private float temperature = 0.7f;

    public void GetChatResponse(
        string systemPrompt,
        string userInput,
        Action<string> callback)
    {
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
    }

    [Serializable]
    public class Choice
    {
        public Message message;
    }
}