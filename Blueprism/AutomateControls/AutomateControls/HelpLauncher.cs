using System.Windows.Forms;
using System.Diagnostics;
using System.Globalization;
using System;
using System.Reflection;
using BluePrism.Core.Utility;

namespace AutomateControls
{
    public static class HelpLauncher
    {
        private const string _baseURL = @"https://bpdocs.blueprism.com";

        private static Uri VersionCultureUrl(Control parent) =>
            new Uri($@"{_baseURL}/{GetBPVersion(parent)}/{GetHelpDocumentationCulture().ToLower()}/");

        private static Uri VersionCultureUrl() =>
            new Uri($@"{_baseURL}/{GetBPVersion()}/{GetHelpDocumentationCulture().ToLower()}/");

        public static string GetBPVersion(Control parent)
        {
            var bpVersion = parent.ProductVersion.Split('.');
            return $"bp-{bpVersion[0]}-{bpVersion[1]}";
        }

        public static string GetBPVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

            var bpVersion = fileVersionInfo.ProductVersion.Split('.');
            return $"bp-{bpVersion[0]}-{bpVersion[1]}";
        }

        /// <summary>
        /// Shows the help contents.
        /// </summary>
        /// <param name="parent">The parent control</param>
        public static void ShowContents(Control parent, string offlineHelpBaseUrl = null)
            => LaunchHelpPage(parent, "home.htm", offlineHelpBaseUrl);

        /// <summary>
        /// Shows the help search.
        /// </summary>
        /// <param name="parent">The parent control</param>
        public static void ShowSearch(Control parent, string offlineHelpBaseUrl = null)
            => LaunchHelpPage(parent, "Search.htm", offlineHelpBaseUrl);

        /// <summary>
        /// Shows the help at a particualr topic.
        /// </summary>
        /// <param name="parent">The parent control</param>
        public static void ShowTopic(IHelp parent)
        {
            string file = parent.GetHelpFile();
            if (file != null)
                ShowTopic(parent as Control, file);
        }

        /// <summary>
        /// Shows the help at a particualr topic.
        /// </summary>
        /// <param name="parent">The parent control</param>
        /// <param name="topic">The help topic</param>
        /// <param name="offlineHelpBaseUrl">The base url for hosting offline help files</param>
        public static void ShowTopic(Control parent, string topic, string offlineHelpBaseUrl = null)
        {           
            if (!string.IsNullOrEmpty(topic) && !topic.Contains(".htm"))
                topic += ".htm";

            LaunchHelpPage(parent, topic, offlineHelpBaseUrl);
        }

        public static void ShowAcknowledgements(string topic, string offlineHelpBaseUrl = null)
        {
            LaunchAcknowledgementsPage(topic, offlineHelpBaseUrl);
        }

        /// <summary>
        /// Shows the topics by number help page anchored at the given topic number.
        /// </summary>
        /// <param name="parent">The parent control for the help window.</param>
        /// <param name="TopicNumber">The topic number to display</param>
        public static void ShowTopicNumber(Control parent, int topicNo)
        {
            ShowTopic(parent, "helpTopicsByNumber.htm#Topic" + topicNo);
        }

        public static void LaunchAcknowledgementsPage(string page, string offlineHelpBaseUrl)
        {
            Uri fullUri;

            if (offlineHelpBaseUrl == null)
            {
                fullUri = new Uri($@"{_baseURL}/{page}");
                Process.Start(fullUri.AbsoluteUri);
            }
            else
            {
                if (!offlineHelpBaseUrl.EndsWith("/"))
                    offlineHelpBaseUrl += "/";
                fullUri = new Uri(new Uri(offlineHelpBaseUrl), $@"{page}");
                Process.Start(fullUri.LocalPath);
            }
        }

        private static void LaunchHelpPage(Control parent, string page, string offlineHelpBaseUrl)
        {
            Uri fullUri;

            if (offlineHelpBaseUrl == null)
            {
                fullUri = new Uri(VersionCultureUrl(parent), page);
                Process.Start(fullUri.AbsoluteUri);
            }
            else
            {
                if (!offlineHelpBaseUrl.EndsWith("/"))
                    offlineHelpBaseUrl += "/";
                fullUri = new Uri(new Uri(offlineHelpBaseUrl), $@"{GetHelpDocumentationCulture().ToLower()}/{page}");
                Process.Start(fullUri.LocalPath);
            }
        }
        
        public static string GetHelpUrl(Control parent, string page)
        {
            return new Uri(VersionCultureUrl(parent), page).ToString();
        }

        public static string GetHelpUrl(string page) =>
            new Uri(VersionCultureUrl(), page).ToString();

        /// <summary>
        /// GetHelpDocumentationCulture will need to be updated whenever we add a new lanaguage/localisation, due to hardcoded urls.
        /// The dev wiki on internationalisation should point to the requirements of this function
        /// </summary>
        /// <returns></returns>
        public static string GetHelpDocumentationCulture()
        {
            var parentUICulture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            if (parentUICulture.Equals("fr"))
                return "fr-fr";
            if (parentUICulture.Equals("zh"))
                return "zh-hans";
            if (parentUICulture.Equals("ja"))
                return "ja-jp";
            if (parentUICulture.Equals("de"))
                return "de-de";
            if (CultureHelper.IsLatinAmericanSpanish())
                return CultureHelper.LatinAmericanSpanishHelpCode;

            return "en-us";
        }
    }
}
