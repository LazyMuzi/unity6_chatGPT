public enum GameLanguage
{
    Korean,
    English,
    Japanese,
    ChineseSimplified,
    Spanish
}

public static class LanguageUtility
{
    public static string GetLanguageInstruction(GameLanguage language)
    {
        switch (language)
        {
            case GameLanguage.Korean:
                return "Respond only in Korean. Do not use any other language.";

            case GameLanguage.English:
                return "Respond only in English.";

            case GameLanguage.Japanese:
                return "Respond only in Japanese.";

            case GameLanguage.ChineseSimplified:
                return "Respond only in Simplified Chinese.";

            case GameLanguage.Spanish:
                return "Respond only in Spanish.";

            default:
                return "Respond in English.";
        }
    }
}