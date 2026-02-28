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

    /// <summary>
    /// 친밀도 + 시간 경과를 반영한 인사를 반환합니다.
    /// </summary>
    /// <param name="daysSince">마지막 대화 이후 경과 일수. -1이면 첫 만남.</param>
    /// <param name="consecutiveDays">연속 대화 일수.</param>
    /// <param name="talkedToday">오늘 이미 대화한 적 있는지.</param>
    public string GetGreeting(int daysSince = -1, int consecutiveDays = 0, bool talkedToday = false)
    {
        if (daysSince < 0)
            return GetFirstMeetGreeting();

        if (talkedToday)
            return GetSameDayGreeting();

        if (daysSince <= 1 && consecutiveDays >= 2)
            return GetConsecutiveGreeting(consecutiveDays);

        if (daysSince >= 30)
            return GetLongAbsenceGreeting(daysSince);

        if (daysSince >= 7)
            return GetWeekAbsenceGreeting(daysSince);

        if (daysSince >= 2)
            return GetFewDaysGreeting(daysSince);

        return GetDefaultGreeting();
    }

    private string GetFirstMeetGreeting()
    {
        if (affinity < 20) return "...누구야?";
        if (affinity < 50) return "어... 처음 보는 얼굴이네.";
        if (affinity < 80) return "안녕! 처음 보는 얼굴이다?";
        return "안녕! 반가워!";
    }

    private string GetSameDayGreeting()
    {
        if (affinity < 20) return "...또 왔어?";
        if (affinity < 50) return "오늘 또 온 거야?";
        if (affinity < 80) return "어? 오늘 또 보네!";
        return "오늘도 왔구나! 좋다~";
    }

    private string GetConsecutiveGreeting(int days)
    {
        if (affinity < 20) return "...또 왔네.";
        if (affinity < 50) return $"또 보네, {days}일 연속이야?";
        if (affinity < 80) return $"매일 보는 거 아냐? 벌써 {days}일째!";
        return $"와, {days}일 연속이다! 매일 와줘서 고마워!";
    }

    private string GetFewDaysGreeting(int days)
    {
        if (affinity < 20) return $"...{days}일 만이네.";
        if (affinity < 50) return $"어, {days}일 만이네?";
        if (affinity < 80) return $"{days}일 만이다! 잘 지냈어?";
        return $"{days}일이나 안 오더니! 보고 싶었어~";
    }

    private string GetWeekAbsenceGreeting(int days)
    {
        if (affinity < 20) return "...오랜만이네.";
        if (affinity < 50) return $"오랜만이다, {days}일 만이야?";
        if (affinity < 80) return $"야! {days}일 만이잖아! 어디 갔었어?";
        return $"{days}일이나 안 왔잖아! 무슨 일 있었어? 걱정했어...";
    }

    private string GetLongAbsenceGreeting(int days)
    {
        if (affinity < 20) return "...살아있었어?";
        if (affinity < 50) return "완전 오랜만이야... 잊은 줄 알았어.";
        if (affinity < 80) return $"야!! {days}일이야! 어디 간 줄 알았잖아!";
        return $"{days}일이나... 나 진짜 걱정했거든! 다시 와줘서 고마워.";
    }

    private string GetDefaultGreeting()
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

    /// <summary>
    /// 대화 종료 후 NPC 말풍선에 표시할 짧은 이별인사입니다.
    /// </summary>
    public string GetBubbleFarewell()
    {
        if (affinity < 20) return "...";
        if (affinity < 50) return "잘 가~";
        if (affinity < 80) return "또 봐!";
        return "안녕~ 보고 싶을 거야!";
    }

    public int GetMaxTurns()
    {
        if (affinity < 20) return 3;
        if (affinity < 50) return 5;
        if (affinity < 80) return 7;
        return 10;
    }

    #region Local Response Pool

    private static readonly string[] FallbackLow =
    {
        "......", "...뭐.", "...그래.", "...음.", "...별로."
    };

    private static readonly string[] FallbackMid =
    {
        "음... 그렇구나.", "아 그래?", "흠, 알겠어.", "그런 거구나~", "오~ 그랬어?"
    };

    private static readonly string[] FallbackHigh =
    {
        "헤헤, 그렇구나!", "오~ 재밌다!", "그치그치!", "맞아맞아!", "와, 진짜?"
    };

    private static readonly string[] FallbackSpecial =
    {
        "듣고 있어~ 더 얘기해줘!", "너랑 이야기하면 기분 좋아~",
        "응응, 그래서?", "재밌다! 또 얘기해줘~", "좋다~ 이런 이야기!"
    };

    /// <summary>
    /// 토큰 예산 초과/쿨다운 시 사용하는 로컬 응답을 반환합니다.
    /// </summary>
    public string GetLocalResponse()
    {
        string[] pool;

        if (affinity < 20) pool = FallbackLow;
        else if (affinity < 50) pool = FallbackMid;
        else if (affinity < 80) pool = FallbackHigh;
        else pool = FallbackSpecial;

        return pool[UnityEngine.Random.Range(0, pool.Length)];
    }

    #endregion
}