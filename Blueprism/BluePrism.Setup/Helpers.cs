using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using BluePrism.Setup.Controls;
using Microsoft.Win32;
using WixSharp;

namespace BluePrism.Setup
{

    public class Helpers
    {
        private const int SW_RESTORE = 0x9;
        
        [DllImport("user32.dll", EntryPoint = "FindWindowW")]
        private static extern IntPtr FindWindowW([MarshalAs(UnmanagedType.LPTStr)] string lpClassName, [MarshalAs(UnmanagedType.LPTStr)] string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "IsWindowVisible")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "ShowWindow")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", EntryPoint = "IsIconic")]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "SetForegroundWindow")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        public Helpers(IManagedUIShell shell)
        {
            Shell = shell;
        }

        /// <summary>
        /// Empty constructor for use without shell. Methods that rely on access to Shell will fail.
        /// </summary>
        public Helpers()
        {
            
        }
        public IManagedUIShell Shell { get; set; }

        public string X64CitrixRegistryNode => @"SOFTWARE\WOW6432Node\Citrix";

        public string X86CitrixRegistryNode => @"SOFTWARE\Citrix\ICA Client";

        public Image GetUacShield(int size)
        {
            var shield = SystemIcons.Shield.ToBitmap();
            shield.MakeTransparent();
            var image = new Bitmap(size, size);

            var g = Graphics.FromImage(image);

            g.CompositingMode = CompositingMode.SourceOver;
            g.DrawImage(shield, new Rectangle(0, 0, size, size));
            return image;
        }
        public static string GetInstalledVersion()
        {
            return GetRegistryKey("Version");
        }

        private static string GetRegistryKey(string name)
        {
            try
            {
                using (var hklm = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64))
                {
                    using (var key = hklm.OpenSubKey(@"SOFTWARE\Blue Prism Limited\Automate"))
                    {
                        if (key != null)
                        {
                            return key.GetValue(name).ToString();
                        }
                    }
                }

                using (var hklm = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry32))
                {
                    using (var key = hklm.OpenSubKey(@"SOFTWARE\Blue Prism Limited\Automate"))
                    {
                        if (key != null)
                        {
                            return key.GetValue(name).ToString();
                        }
                    }
                }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public string GetInstalledDirectory()
        {
            return GetRegistryKey("InstallDir");
        }

        private static ApiVersion _installedApiVersion;

        public static ApiVersion InstalledApiVersion
        {
            get
            {
                if (_installedApiVersion != null) return _installedApiVersion;
                var versionValue = GetRegistryKey("APIVersion");
                if (!string.IsNullOrWhiteSpace(versionValue)) _installedApiVersion = new ApiVersion(versionValue);
                return _installedApiVersion;
            }
        }

        private static ApiVersion _installerApiVersion;

        public static ApiVersion InstallerApiVersion
        {
            get
            {
                if (_installerApiVersion != null) return _installerApiVersion;
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith("APIVersion.txt"));
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                    if (stream != null)
                        using (var reader = new StreamReader(stream))
                        {
                            _installerApiVersion = new ApiVersion(reader.ReadToEnd());
                        }

                return _installerApiVersion;
            }
        }

        public static bool SkipApiWarning
        {
            get
            {
                var installedApi = InstalledApiVersion;
                return installedApi == null || ApiVersion.AreCompatible(installedApi, InstallerApiVersion);
            }
        }

        private static bool _isPatchUpgrade;

        public bool IsPatchUpgrade(string version)
        {
            if (_isPatchUpgrade) return true;

            var installedVersionString = GetInstalledVersion();
            if (string.IsNullOrEmpty(installedVersionString)) return false;

            Version.TryParse(installedVersionString, out var installedVersion);
            Version.TryParse(version, out var thisVersion);

            if (installedVersion == null || thisVersion == null) return false;
            //Patch Upgrade is where <major> and <minor> match but patch is higher 

            _isPatchUpgrade = installedVersion.Major == thisVersion.Major &&
                installedVersion.Minor == thisVersion.Minor &&
                thisVersion.Build > installedVersion.Build;

            return _isPatchUpgrade;
        }
        public void ApplySubtitleAppearance(BluePrismReadOnlyTextBox subtitle)
        {
            subtitle.Font = GetSubTitleFont(subtitle.Font.Name);
            subtitle.AutoSize = false;
            subtitle.Height = 93;
        }
        public void ApplySubtitleFont(BluePrismReadOnlyTextBox subtitle)
        {
            subtitle.Font = GetSubTitleFont(subtitle.Font.Name);
        }

        private static Font _subTitleFont;
        private Font GetSubTitleFont(string fontName)
        {
            if (_subTitleFont != null) return _subTitleFont;
            var subTitles = new List<string>();
            foreach (var t in Shell.Dialogs)
            {
                System.ComponentModel.ComponentResourceManager dialog = new System.ComponentModel.ComponentResourceManager(t);
                var subtitle = dialog.GetString("Subtitle.Text");
                if (!string.IsNullOrEmpty(subtitle))
                    subTitles.Add(subtitle);
            }

            subTitles = subTitles.OrderByDescending(x => x.Length).ToList();
            var longestSubTitle = subTitles.FirstOrDefault();
            float fontSize = 24.2F;
            float result = 0;
            var font = new Font(fontName, fontSize, FontStyle.Regular, GraphicsUnit.Pixel);
            //Maximum width for a subtitle
            var maxWidth = 650;
            font = GetFontForWordAndWidth(longestSubTitle, fontSize, font, maxWidth);
            _subTitleFont = font;
            return font;
        }

        public Font GetFontForWordAndWidth(string word, float fontSize, Font font, int maxWidth)
        {
            float result;
            if (string.IsNullOrEmpty(word)) return font;
            do
            {
                fontSize = fontSize - 0.2F;
                font = new Font("Segoe UI", fontSize, FontStyle.Regular, GraphicsUnit.Pixel);
                result = MeasureString(font, word);
            } while (result > maxWidth);

            return font;
        }

        public int MeasureString(Font font, string text)
        {
            using (var image = new Bitmap(1, 1))
            {
                using (var g = Graphics.FromImage(image))
                {
                    return (int)Math.Ceiling(g.MeasureString(text, font).Width);
                }
            }
        }
        public void ChangeLocale(string newLocaleName)
        {
            if (string.IsNullOrEmpty(newLocaleName))
                throw new ArgumentNullException("The Culture Name cannot be null");

            CultureInfo newLocale;
            CultureInfo newLocaleFormat;
            if (newLocaleName == CurrentLocale.Name)
            {
                newLocale = new CultureInfo(CurrentLocale.Name);
                newLocaleFormat = new CultureInfo(CurrentLocale.Name);
            }
            else
            {
                newLocale = new CultureInfo(newLocaleName);
                newLocaleFormat = new CultureInfo(newLocaleName);
            }
            _currentCulture = newLocale;
            Thread.CurrentThread.CurrentUICulture = newLocale;
            Thread.CurrentThread.CurrentCulture = newLocaleFormat;
            _subTitleFont = null;
        }

        private static CultureInfo _currentCulture;

        public CultureInfo CurrentLocale
        {
            get
            {
                if (_currentCulture == null)
                    _currentCulture = CultureInfo.CurrentUICulture;

                return _currentCulture;
            }
        }

        public void ForceFocusToForm(Object form)
        {
            var hwnd = FindWindowW(null, ((Form)form).Text); // Find the window handle (Works even if the app is hidden and not shown in taskbar)
            if (!IsWindowVisible(hwnd) | IsIconic(hwnd))
                ShowWindow(hwnd, SW_RESTORE);

            // toggle topMost for our Main form, then set it back as it was
            var topMostValue = ((Form)form).TopMost;
            if (((Form)form).TopMost)
                ((Form)form).TopMost = false;
            ((Form)form).TopMost = true;
            ((Form)form).TopMost = topMostValue;

            ((Form)form).ShowInTaskbar = true;
            SetForegroundWindow(hwnd); // Set the window as the foreground window
        }

        public bool ShellLogContainsCancellationMessage => _shellLogCancellationMessages.Any(l => Shell.Log.Contains(l));

        private readonly List<string> _shellLogCancellationMessages = new List<string>
        {
            "User cancelled installation.",
            "Die Installation wurde vom Benutzer abgebrochen.",
            "L’utilisateur a annulé l’installation.",
            "El usuario ha cancelado la instalación.",
            "ユーザーがインストールを取り消しました。",
            "用户取消了安装。",
            "使用者取消安裝。"
        };

        public string GetCitrixInstallFolder()
        {
            var key = Registry.LocalMachine.OpenSubKey(string.Concat(X64CitrixRegistryNode, @"\Install\ICA Client")) ?? Registry.LocalMachine.OpenSubKey(string.Concat(X86CitrixRegistryNode, @"\Install\ICA Client"));
            var citrixICAInstallFolder = key?.GetValue("InstallFolder")?.ToString();
            return citrixICAInstallFolder;
        }
    }
}
