namespace BluePrism.Api.Setup
{
    using System.Collections.Generic;

    public static class LanguageConstants
    {
        public static readonly IReadOnlyCollection<string> SupportedLanguages = new []
        {
            "de-DE",
            "en-US",
            "es-419",
            "fr-FR",
            "ja-JP",
            "zh-Hans",
        };
    }
}
