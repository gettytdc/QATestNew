using System.ComponentModel;

namespace BluePrism.BrowserAutomation.WebMessages
{
    public static class BrowserProcessNames
    {
        public const string Chrome = "chrome";
        public const string Edge = "msedge";
        public const string Firefox = "firefox";

        public static string GetBrowserProcessName(BrowserType browserType)
        {
            switch (browserType)
            {
                case BrowserType.Chrome:
                    return Chrome;
                case BrowserType.Firefox:
                    return Firefox;
                case BrowserType.Edge:
                    return Edge;
                default:
                    throw new InvalidEnumArgumentException(nameof(browserType), (int)browserType, typeof(BrowserType));
            }
        }
    }
}
