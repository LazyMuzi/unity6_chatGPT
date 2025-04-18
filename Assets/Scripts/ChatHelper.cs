using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Proyecto26;
using UnityEngine;
using UnityEngine.Networking;
using System;
public class ChatHelper : MonoBehaviour
{
    [SerializeField] private string apiUrl = "https://api.openai.com/v1/chat/completions";
    [SerializeField] private string apiKey = "Replace with an actual API key";

    public void GetChatGPTResponse(string prompt, Action<string> callback)
    {
        var requestData = new ChatRequest
        {
            model = "gpt-3.5-turbo",
            messages = new[]
            {
                new Message { role = "user", content = prompt }
            },
            max_tokens = 20
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
        }).Then(response =>
        {
            var chatResponse = JsonConvert.DeserializeObject<ChatResponse>(response.Text);
            var content = chatResponse.choices[0].message.content.Trim();
            callback?.Invoke(content);
        }).Catch(error =>
        {
            callback?.Invoke("API Error: " + error.Message);
            Debug.LogError("API Error: " + error.Message);
        });
    }

    [System.Serializable]
    public class Message
    {
        public string role;
        public string content;
    }

    [System.Serializable]
    public class ChatRequest
    {
        public string model;
        public Message[] messages;
        public int max_tokens;
    }

    [System.Serializable]
    public class ChatResponse
    {
        public Choice[] choices;
    }

    [System.Serializable]
    public class Choice
    {
        public Message message;
    }
}
