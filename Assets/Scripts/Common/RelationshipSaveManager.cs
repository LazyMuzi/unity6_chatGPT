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
        public string npcId;
        public int affinity;
        public int totalConversations;
        public string lastConversationDate = "";
        public int consecutiveDays;
        public string activeQuestId = "";
        public List<QuestTimeEntry> lastQuestTimes = new();
        public string memorySummary = "";
        public int dailyAffinityGained;
        public string dailyAffinityDate = "";
    }

    [System.Serializable]
    private class QuestTimeEntry
    {
        public string questId;
        public float time;
    }

    [System.Serializable]
    private class SaveFile
    {
        public List<NPCSaveData> entries = new();
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

    public void SaveInteraction(string npcId, int totalConversations, string lastDate,
        int consecutiveDays, int dailyAffinityGained = -1, string dailyAffinityDate = null)
    {
        if (string.IsNullOrEmpty(npcId)) return;
        EnsureEntry(npcId);

        var entry = saveData[npcId];
        entry.totalConversations = totalConversations;
        entry.lastConversationDate = lastDate;
        entry.consecutiveDays = consecutiveDays;

        if (dailyAffinityGained >= 0)
            entry.dailyAffinityGained = dailyAffinityGained;
        if (dailyAffinityDate != null)
            entry.dailyAffinityDate = dailyAffinityDate;

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
            consecutiveDays = data.consecutiveDays,
            dailyAffinityGained = data.dailyAffinityGained,
            dailyAffinityDate = data.dailyAffinityDate ?? ""
        };
    }

    public class InteractionLoadResult
    {
        public int totalConversations;
        public string lastConversationDate;
        public int consecutiveDays;
        public int dailyAffinityGained;
        public string dailyAffinityDate;
    }

    #endregion

    #region Quest

    public void SaveQuest(string npcId, string questId, Dictionary<string, float> lastQuestTimes)
    {
        if (string.IsNullOrEmpty(npcId)) return;
        EnsureEntry(npcId);

        var entry = saveData[npcId];
        entry.activeQuestId = questId ?? "";

        entry.lastQuestTimes.Clear();
        if (lastQuestTimes != null)
        {
            foreach (var kvp in lastQuestTimes)
                entry.lastQuestTimes.Add(new QuestTimeEntry { questId = kvp.Key, time = kvp.Value });
        }

        WriteToDisk();
    }

    /// <summary>
    /// 퀘스트 데이터를 반환합니다. 해당 NPC의 저장 데이터가 없으면 null을 반환합니다.
    /// </summary>
    public QuestLoadResult LoadQuest(string npcId)
    {
        if (string.IsNullOrEmpty(npcId) || !saveData.TryGetValue(npcId, out var data))
            return null;

        var times = new Dictionary<string, float>();
        if (data.lastQuestTimes != null)
        {
            foreach (var e in data.lastQuestTimes)
                times[e.questId] = e.time;
        }

        return new QuestLoadResult
        {
            activeQuestId = data.activeQuestId ?? "",
            lastQuestTimes = times
        };
    }

    public class QuestLoadResult
    {
        public string activeQuestId;
        public Dictionary<string, float> lastQuestTimes;
    }

    #endregion

    #region Memory

    public void SaveMemory(string npcId, string memorySummary)
    {
        if (string.IsNullOrEmpty(npcId)) return;
        EnsureEntry(npcId);
        saveData[npcId].memorySummary = memorySummary ?? "";
        WriteToDisk();
    }

    public string LoadMemory(string npcId)
    {
        if (string.IsNullOrEmpty(npcId) || !saveData.TryGetValue(npcId, out var data))
            return "";
        return data.memorySummary ?? "";
    }

    #endregion

    #region Core IO

    private void EnsureEntry(string npcId)
    {
        saveData.TryAdd(npcId, new NPCSaveData());
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
                entry.lastConversationDate ??= "";
                entry.activeQuestId ??= "";
                entry.lastQuestTimes ??= new List<QuestTimeEntry>();
                entry.memorySummary ??= "";
                entry.dailyAffinityDate ??= "";
                saveData[entry.npcId] = entry;
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
            kvp.Value.npcId = kvp.Key;
            file.entries.Add(kvp.Value);
        }

        File.WriteAllText(SavePath, JsonUtility.ToJson(file, true));
    }

    public void DeleteSave()
    {
        saveData.Clear();
        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }

    #endregion
}
