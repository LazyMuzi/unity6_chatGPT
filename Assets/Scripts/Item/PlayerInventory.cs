using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// 플레이어의 아이템 인벤토리를 관리합니다. JSON 파일로 영구 저장됩니다.
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    public event Action<ItemData, int> OnItemAdded;
    public event Action<string, int> OnItemRemoved;

    private const string FileName = "inventory_data.json";
    private Dictionary<string, int> items = new();
    private string SavePath => Path.Combine(Application.persistentDataPath, FileName);

    private void Awake()
    {
        Instance = this;
        LoadFromDisk();
    }

    public void AddItem(ItemData item, int amount = 1)
    {
        if (item == null || amount <= 0) return;

        items.TryGetValue(item.itemId, out int current);
        items[item.itemId] = current + amount;

        Debug.Log($"[Inventory] '{item.itemName}' x{amount} 획득 (보유: {items[item.itemId]})");
        SaveToDisk();
        OnItemAdded?.Invoke(item, amount);
    }

    public bool HasItem(string itemId, int amount = 1)
    {
        return items.TryGetValue(itemId, out int count) && count >= amount;
    }

    public bool RemoveItem(string itemId, int amount = 1)
    {
        if (!HasItem(itemId, amount)) return false;

        items[itemId] -= amount;
        if (items[itemId] <= 0)
            items.Remove(itemId);

        Debug.Log($"[Inventory] '{itemId}' x{amount} 제거");
        SaveToDisk();
        OnItemRemoved?.Invoke(itemId, amount);
        return true;
    }

    public int GetItemCount(string itemId)
    {
        return items.TryGetValue(itemId, out int count) ? count : 0;
    }

    #region Persistence

    [Serializable]
    private class SaveFile
    {
        public List<ItemEntry> entries = new();
    }

    [Serializable]
    private class ItemEntry
    {
        public string itemId;
        public int count;
    }

    private void SaveToDisk()
    {
        var file = new SaveFile();
        foreach (var kvp in items)
            file.entries.Add(new ItemEntry { itemId = kvp.Key, count = kvp.Value });

        File.WriteAllText(SavePath, JsonUtility.ToJson(file, true));
    }

    private void LoadFromDisk()
    {
        items.Clear();
        if (!File.Exists(SavePath)) return;

        try
        {
            var file = JsonUtility.FromJson<SaveFile>(File.ReadAllText(SavePath));
            if (file?.entries == null) return;

            foreach (var entry in file.entries)
                items[entry.itemId] = entry.count;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Inventory] 로드 실패: {e.Message}");
        }
    }

    #endregion
}
