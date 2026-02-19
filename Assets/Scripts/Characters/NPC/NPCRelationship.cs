using UnityEngine;

[System.Serializable]
public class NPCRelationship
{
    public int affinity = 50;

    public void ModifyAffinity(int amount)
    {
        affinity += amount;
        affinity = Mathf.Clamp(affinity, 0, 100);
    }

    public string GetAffinityDescription()
    {
        if (affinity < 20) return "어색함";
        if (affinity < 50) return "편안함";
        if (affinity < 80) return "반가움";
        return "특별한 존재";
    }

    public string GetGreeting()
    {
        if (affinity < 20) return "...뭐야.";
        if (affinity < 50) return "어, 왔어?";
        if (affinity < 80) return "안녕! 보니까 반갑다!";
        return "왔구나! 보고 싶었어!";
    }

    public string GetAttitudeInstruction()
    {
        if (affinity < 20)
            return "The player is almost a stranger. Respond guardedly with short, blunt answers.";
        if (affinity < 50)
            return "The player is an acquaintance. Respond comfortably but keep some distance.";
        if (affinity < 80)
            return "The player is a good friend. Respond warmly and with genuine friendliness.";
        return "The player is someone very special. Respond with deep affection and care.";
    }

    public string GetFarewell()
    {
        if (affinity < 20) return "...가봐야 해.";
        if (affinity < 50) return "슬슬 가봐야겠다.";
        if (affinity < 80) return "벌써 시간이 이렇게 됐네! 다음에 또 보자!";
        return "벌써 헤어져야 하다니... 다음에 꼭 다시 만나자!";
    }

    public int GetMaxTurns()
    {
        if (affinity < 20) return 3;
        if (affinity < 50) return 5;
        if (affinity < 80) return 7;
        return 10;
    }
}