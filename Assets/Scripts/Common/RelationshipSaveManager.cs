using UnityEngine;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// NPC별 친밀도, 대화 기록, 퀘스트 진행 상태를 JSON으로 저장/로드합니다.
/// </summary>
public class RelationshipSaveManager : MonoBehaviour
{
    public static RelationshipSaveManager Instance;

    private const string FileName = "relationship_data.json";
    private const int DefaultAffinity = 50;

    private Dictionary<string, NPCSaveData> saveData = new();
    private string SavePath => Path.Combine(Application.persistentDataPath, FileName);

    [System.Serializable]
    private class NPCSaveData
    {
        public int affinity;
        public int totalConversations;
        public string lastConversationDate = "";
        public int consecutiveDays;
        public string activeQuestId = "";
        public int questProgress;
    }

    [System.Serializable]
    private class SaveFile
    {
        public List<SaveEntry> entries = new();
    }

    [System.Serializable]
    private class SaveEntry
    {
        public string npcId;
        public int affinity;
        public int totalConversations;
        public string lastConversationDate = "";
        public int consecutiveDays;
        public string activeQuestId = "";
        public int questProgress;
    }

    private void Awake()
    {
        Instance = this;
        LoadAll();
    }

    #region Affinity

    public void Save(string npcId, int affinity)
    {
        if (string.IsNullOrEmpty(npcId)) return;
        EnsureEntry(npcId);
        saveData[npcId].affinity = affinity;
        WriteToDisk();
    }

    public int Load(string npcId)
    {
        if (!string.IsNullOrEmpty(npcId) && saveData.TryGetValue(npcId, out var data))
            return data.affinity;

        return DefaultAffinity;
    }

    public bool TryLoad(string npcId, out int affinity)
    {
        affinity = DefaultAffinity;

        if (string.IsNullOrEmpty(npcId))
            return false;

        if (saveData.TryGetValue(npcId, out var data))
        {
            affinity = data.affinity;
            return true;
        }

        return false;
    }

    #endregion

    #region Interaction Tracking

    public void SaveInteraction(string npcId, int totalConversations, string lastDate, int consecutiveDays)
    {
        if (string.IsNullOrEmpty(npcId)) return;
        EnsureEntry(npcId);

        var entry = saveData[npcId];
        entry.totalConversations = totalConversations;
        entry.lastConversationDate = lastDate;
        entry.consecutiveDays = consecutiveDays;
        WriteToDisk();
    }

    /// <summary>
    /// 대화 기록 데이터를 반환합니다. 데이터가 없으면 null을 반환합니다.
    /// </summary>
    public InteractionLoadResult LoadInteraction(string npcId)
    {
        if (string.IsNullOrEmpty(npcId) || !saveData.TryGetValue(npcId, out var data))
            return null;

        return new InteractionLoadResult
        {
            totalConversations = data.totalConversations,
            lastConversationDate = data.lastConversationDate,
            consecutiveDays = data.consecutiveDays
        };
    }

    public class InteractionLoadResult
    {
        public int totalConversations;
        public string lastConversationDate;
        public int consecutiveDays;
    }

    #endregion

    #region Quest

    public void SaveQuest(string npcId, string questId, int progress)
    {
        if (string.IsNullOrEmpty(npcId)) return;
        EnsureEntry(npcId);

        var entry = saveData[npcId];
        entry.activeQuestId = questId ?? "";
        entry.questProgress = progress;
        WriteToDisk();
    }

    /// <summary>
    /// 퀘스트 데이터를 반환합니다. 활성 퀘스트가 없으면 null을 반환합니다.
    /// </summary>
    public QuestLoadResult LoadQuest(string npcId)
    {
        if (string.IsNullOrEmpty(npcId) || !saveData.TryGetValue(npcId, out var data))
            return null;

        if (string.IsNullOrEmpty(data.activeQuestId))
            return null;

        return new QuestLoadResult
        {
            activeQuestId = data.activeQuestId,
            questProgress = data.questProgress
        };
    }

    public class QuestLoadResult
    {
        public string activeQuestId;
        public int questProgress;
    }

    #endregion

    #region Core IO

    private void EnsureEntry(string npcId)
    {
        if (!saveData.ContainsKey(npcId))
            saveData[npcId] = new NPCSaveData();
    }

    public void LoadAll()
    {
        saveData.Clear();

        if (!File.Exists(SavePath)) return;

        try
        {
            string json = File.ReadAllText(SavePath);
            var file = JsonUtility.FromJson<SaveFile>(json);

            if (file?.entries == null) return;

            foreach (var entry in file.entries)
            {
                saveData[entry.npcId] = new NPCSaveData
                {
                    affinity = entry.affinity,
                    totalConversations = entry.totalConversations,
                    lastConversationDate = entry.lastConversationDate ?? "",
                    consecutiveDays = entry.consecutiveDays,
                    activeQuestId = entry.activeQuestId ?? "",
                    questProgress = entry.questProgress
                };
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Failed to load save data: " + e.Message);
        }
    }

    private void WriteToDisk()
    {
        var file = new SaveFile();
        foreach (var kvp in saveData)
        {
            file.entries.Add(new SaveEntry
            {
                npcId = kvp.Key,
                affinity = kvp.Value.affinity,
                totalConversations = kvp.Value.totalConversations,
                lastConversationDate = kvp.Value.lastConversationDate,
                consecutiveDays = kvp.Value.consecutiveDays,
                activeQuestId = kvp.Value.activeQuestId,
                questProgress = kvp.Value.questProgress
            });
        }

        string json = JsonUtility.ToJson(file, true);
        File.WriteAllText(SavePath, json);
    }

    public void DeleteSave()
    {
        saveData.Clear();
        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }

    #endregion
}
