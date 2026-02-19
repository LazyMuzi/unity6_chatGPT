using System.Collections.Generic;

[System.Serializable]
public class NPCMemory
{
    public List<string> recentDialogues = new List<string>();
    public string summarizedMemory = "";

    public void AddDialogue(string dialogue)
    {
        recentDialogues.Add(dialogue);

        if (recentDialogues.Count > 5)
            recentDialogues.RemoveAt(0);
    }

    public string GetRecentConversation()
    {
        return string.Join("\n", recentDialogues);
    }
}