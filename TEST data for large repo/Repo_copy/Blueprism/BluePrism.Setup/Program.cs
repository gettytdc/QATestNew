using BluePrism.Setup.Dialogs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using System.Xml.Linq;
using WixSharp;
using WixSharp.CommonTasks;
using BluePrism.Core.Utility;

namespace BluePrism.Setup
{
    public enum Edition
    {
        Enterprise,
        Trial
    }

    public class Program
    {
        private static readonly bool BluePrismBuild = bool.Parse(Environment.GetEnvironmentVariable("BluePrismBuild") ?? "false");

        private string _semanticVersion = Environment.GetEnvironmentVariable("SemanticVersion");

        private const int EnterpriseInstallCost = 500 * 1024;//MB -> KB
        private const int TrialInstallCost = 750 * 1024;

        private readonly Platform _platform = ParsePlatform(Environment.GetEnvironmentVariable("Platform"));
        private readonly bool _includeBrowserPlugin = bool.Parse(Environment.GetEnvironmentVariable("IncludePlugin") ?? "false");
        private readonly string _signingCertificateHash = Environment.GetEnvironmentVariable("SigningCertificateHash");

        private static Platform ParsePlatform(string platform) => (platform == "x86") ? Platform.x86 : Platform.x64;

        [STAThread]
        public static void Main()
        {
            if (!BluePrismBuild) return;
            var p = new Program();

            p.Build(Edition.Enterprise);

            // We don't build a "Trial" installer for x86 currently
            if (p._platform == Platform.x86) return;
            p.Build(Edition.Trial);
        }

        private void Build(Edition edition)
        {
            var feature = new Feature
            {
                Name = new LocalizedId("BluePrism"),
                IsEnabled = true,
                AllowChange = false,
                Display = FeatureDisplay.expand,
                AttributesDefinition = "SortKey=A"
            };

            var bpServer = new Feature
            {
                Id = new Id("BPServer"),
                IsEnabled = true,
                AllowChange = false,
                Display = FeatureDisplay.hidden
            };

            var outlook = new Feature
            {
                Name = new LocalizedId("OutlookAutomation"),
                IsEnabled = true,
                AllowChange = true,
                Display = FeatureDisplay.expand
            };
            feature.Add(outlook);

            var googleSheets = new Feature
            {
                Id = new Id("GoogleSheets"),
                Name = new LocalizedId("GoogleSheetsAutomation"),
                IsEnabled = true,
                AllowChange = true,
                Display = FeatureDisplay.expand
            };
            feature.Add(googleSheets);

            var chromePlugin = new Feature
            {
                Id = new Id("ChromePlugin"),
                Name = new LocalizedId("ChromeBrowserExtension"),
                IsEnabled = true,
                AllowChange = true,
                Display = FeatureDisplay.expand,
                AttributesDefinition = "SortKey=B"
            };

            var edgePlugin = new Feature
            {
                Id = new Id("EdgePlugin"),
                Name = new LocalizedId("EdgeBrowserExtension"),
                IsEnabled = true,
                AllowChange = true,
                Display = FeatureDisplay.expand,
                AttributesDefinition = "SortKey=D"
            };

            var firefoxPlugin = new Feature
            {
                Id = new Id("FirefoxPlugin"),
                Name = new LocalizedId("FirefoxBrowserExtension"),
                IsEnabled = true,
                AllowChange = true,
                Display = FeatureDisplay.expand,
                AttributesDefinition = "SortKey=C"
            };

            var citrixDriver = new Feature
            {
                Id = new Id("CitrixDriver"),
                Name = new LocalizedId("CitrixDriverTitle"),
                IsEnabled = false,
                AllowChange = true,
                Display = FeatureDisplay.expand,
                AttributesDefinition = "SortKey=E"
            };

            var redist = new Feature
            {
                Id = new Id("VCRedist"),
                Name = new LocalizedId("VisualStudio2017Runtime"),
                Display = FeatureDisplay.hidden
            };

            var components = new Components(_platform);

            var root = new InstallDir(new Id("INSTALLFOLDER"), "Blue Prism Automate",
                new Dir("ja-JP", new File(@"..\bin\ja-JP\BluePrism.DatabaseInstaller.resources.dll"), new File(@"..\bin\ja-JP\BluePrism.Common.Security.resources.dll")),
                new Dir("gsw-LI", new File(@"..\bin\gsw-LI\BluePrism.DatabaseInstaller.resources.dll")),
                new Dir("zh-Hans", new File(@"..\bin\zh-Hans\BluePrism.DatabaseInstaller.resources.dll"), new File(@"..\bin\zh-Hans\BluePrism.Common.Security.resources.dll")),
                new Dir("de-DE", new File(@"..\bin\de-DE\BluePrism.DatabaseInstaller.resources.dll"), new File(@"..\bin\de-DE\BluePrism.Common.Security.resources.dll")),
                new Dir("es-419", new File(@"..\bin\es-419\BluePrism.DatabaseInstaller.resources.dll")),
                new Dir("fr-FR", new File(@"..\bin\fr-FR\BluePrism.DatabaseInstaller.resources.dll"), new File(@"..\bin\fr-FR\BluePrism.Common.Security.resources.dll"))
            );
            root.AddFiles(components.Statics(feature, bpServer));
            root.AddFiles(components.Media());
            root.AddFiles(components.OutlookInterop(outlook));
            root.AddFiles(components.GoogleSheets(googleSheets));
            root.AddFiles(components.DocumentProcessing(feature, bpServer));
            root.AddDirs(components.SubDirectories());

            // Setup Installers dir
            var installers = new Dir("Installers");
            installers.AddFiles(components.LoginAgentInstallers());
            root.AddDir(installers);

            var vcRedist = new Dir
            {
                Id = new Id("mmTarget"),
                Name = "System"
            };
            vcRedist.AddMergeModules(components.MergeModules(redist));
            root.AddDir(vcRedist);

            var win64 = (_platform == Platform.x64);

            // Setup project.
            var project = new ManagedProject(Constants.ProductName,
                new Dir(win64 ? "%ProgramFiles64%" : "%ProgramFiles%",
                    new Dir(new Id("ManufacturerFolder"), Constants.ManufacturerName, root)));

            var programData = new Dir(
                        @"CommonAppDataFolder\Blue Prism Limited\Blue Prism",
                        new File(@"..\BluePrism.Automate\Automate.NLog.config"),
                        new File(@"..\BluePrism.Automate\BPServer\Server.NLog.config"),
                        new File(@"..\BluePrism.MessagingHost\com.blueprism.messaging-manifest.json"),
                        new File(@"..\BluePrism.MessagingHost\com.blueprism.messaging-firefox-manifest.json"));

            project.AddDir(programData);
            project.AddDirs(components.Shortcuts());

            project.DefaultFeature = feature;
            project.RebootSupressing = RebootSupressing.ReallySuppress;

            //omus is the default for reinstall mode according to MSI documentation, wix# sets to amus
            //https://docs.microsoft.com/en-us/windows/win32/msi/reinstallmode
            project.ReinstallMode = "omus";

            if (!string.IsNullOrEmpty(_signingCertificateHash))
            {
                project.DigitalSignature = new DigitalSignature
                {
                    CertificateId = _signingCertificateHash,
                    CertificateStore = StoreType.sha1Hash,
                    HashAlgorithm = HashAlgorithmType.sha256,
                    Description = "Blue Prism Installer",
                    TimeUrl = new Uri("http://timestamp.digicert.com")
                };
            }

            project.BannerImage = "blank.png";
            project.BackgroundImage = "blank.png";
            project.LocalizationFile = "Default.wxl";

            project.OutDir = Constants.PublishFolder;

            var packager = AssemblyPackager.Instance(Constants.PublishFolder);
            packager.Bind(project);

            // Setup project version.
            project.SetVersionFromFile(@"..\bin\Automate.exe");
            if (string.IsNullOrEmpty(_semanticVersion))
                _semanticVersion = project.Version.ToString();

            // Setup project platform.
            project.Platform = _platform;

            // Harvest content from projects.
            var harvester = new Harvester(project, @"..\bin", "INSTALLFOLDER");
            harvester.AddProjects(components.Projects(feature, bpServer));

            project.GUID = Guid.NewGuid();
            project.UpgradeCode = new Guid("33E3A695-0874-43BB-A13A-3BCCC61ED92E");
            project.InstallerVersion = Constants.InstallerVersion;
            project.InstallScope = InstallScope.perMachine;
            project.InstallPrivileges = InstallPrivileges.elevated;
            project.ControlPanelInfo.ProductIcon = Constants.BluePrismIcon;
            project.ControlPanelInfo.Manufacturer = Constants.ManufacturerName;
            project.ControlPanelInfo.HelpLink = Constants.BluePrismWebsite;

            project.DefaultRefAssemblies.Add(@"..\bin\Internationalisation.dll");

            // Set upgrade options.
            project.MajorUpgrade = new MajorUpgrade
            {
                DowngradeErrorMessage = new LocalizedId("NewerVersionInstalled"),
                AllowSameVersionUpgrades = true,
                MigrateFeatures = true
            };

            // Setup Registry keys.
            project.AddRegKeys(components.RegistryKeys(project, feature));

            // Setup Blue Prism Event Sources.
            root.AddEventSources(components.EventSources());

            // Setup Blue Prism Server Service.
            root.AddFiles(components.BluePrismServerService(bpServer));

            // Setup .NET 4.7 requirement.
            // We can check the NETFRAMEWORK45 property which is set to the Release number of .NET 4.5+ that is installed.
            // The minimum release of .NET 4.7 is 460798 which was the Windows 10 Creators Update version
            project.SetNetFxPrerequisite("NETFRAMEWORK45 >= '#460798'", new LocalizedId("ThisProductRequiresDotNet"));

            // Setup properties.
            project.AddProperties(
                new PropertyRef("NETFRAMEWORK40FULLINSTALLROOTDIR"),
                new Property("PSEUDOLOCALIZATION", "false") { Secure = true },
                new Property("FIREFOX_DIR", string.Empty) { Secure = true },
                new Property("FIREFOX_PREF_FILE_NAME", "all-blueprism.js"),
                new Property("CHROME_PLUGIN_REG_VALUE", "0"),
                new Property("FirefoxBrowserPluginID", "{0df47ec0-3a84-4aac-818f-7a826fe800d6}"),
                new Property("ChromeBrowserPluginID", "lbnooplepikajpiphjgfoniaakpclemh"),
                new Property("EdgeBrowserPluginID", "jecmlbbpjadglfjggkpckhheoblfdohf"),
                new Property("LOCALE", string.Empty) { Secure = true },
                new Property("OUTLOOKINSTALLFOLDER",
                    new RegistrySearch(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\OUTLOOK.EXE", "Path", RegistrySearchType.raw))
                );
         
            //Setup close application settings.
            project.Add(
                new CloseApplication("chrome.exe")
                {
                    CloseMessage = true,
                    Description = new LocalizedId("ChromeIsRunning"),
                    ElevatedCloseMessage = false,
                    RebootPrompt = false,
                    PromptToContinue = true,
                    Condition = chromePlugin.ShallInstall()
                },
                new CloseApplication("msedge.exe")
                {
                    CloseMessage = true,
                    Description = new LocalizedId("EdgeIsRunning"),
                    ElevatedCloseMessage = false,
                    RebootPrompt = false,
                    PromptToContinue = true,
                    Condition = edgePlugin.ShallInstall()
                },
                new CloseApplication("firefox.exe")
                {
                    CloseMessage = true,
                    Description = new LocalizedId("FirefoxIsRunning"),
                    ElevatedCloseMessage = false,
                    RebootPrompt = false,
                    PromptToContinue = true,
                    Condition = firefoxPlugin.ShallInstall()
                });

            // Setup chrome regkey.
            var chromeRegValue = new RegValue(
                chromePlugin,
                RegistryHive.LocalMachine,
                @"Software\Policies\Google\Chrome\ExtensionInstallForcelist",
                "[CHROME_PLUGIN_REG_VALUE]",
                "[ChromeBrowserPluginID];https://clients2.google.com/service/update2/crx")
            {
                ForceCreateOnInstall = true,
                ForceDeleteOnUninstall = true
            };

            var chromeNativeMessagingRegValue = new RegValue(
                feature,
                RegistryHive.LocalMachine,
                @"Software\Google\Chrome\NativeMessagingHosts\com.blueprism.messaging",
                string.Empty,
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}\Blue Prism Limited\Blue Prism\com.blueprism.messaging-manifest.json")
            {
                ForceCreateOnInstall = true,
                ForceDeleteOnUninstall = true
            };

            var edgeNativeMessagingRegValue = new RegValue(
                feature,
                RegistryHive.LocalMachine,
                @"Software\Microsoft\Edge\NativeMessagingHosts\com.blueprism.messaging",
                string.Empty,
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}\Blue Prism Limited\Blue Prism\com.blueprism.messaging-manifest.json")
            {
                ForceCreateOnInstall = true,
                ForceDeleteOnUninstall = true
            };

            var edgeRegValue = new RegValue(
                edgePlugin,
                RegistryHive.LocalMachine,
                @"SOFTWARE\Microsoft\Edge\Extensions\[EdgeBrowserPluginID]",
                "update_url",
                "https://edge.microsoft.com/extensionwebstorebase/v1/crx")
            {
                ForceCreateOnInstall = true,
                ForceDeleteOnUninstall = true
            };

            var firefoxNativeMessagingRegValue = new RegValue(
                feature,
                RegistryHive.LocalMachine,
                @"Software\Mozilla\NativeMessagingHosts\com.blueprism.messaging",
                string.Empty,
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}\Blue Prism Limited\Blue Prism\com.blueprism.messaging-firefox-manifest.json")
            {
                ForceCreateOnInstall = true,
                ForceDeleteOnUninstall = true
            };           

            project.AddRegValues(chromeRegValue,
                                 edgeRegValue,
                                 chromeNativeMessagingRegValue,
                                 edgeNativeMessagingRegValue,
                                 firefoxNativeMessagingRegValue);

            // Setup firefox plugin file.
            const string firefoxPluginFile = @"..\BluePrism.BrowserAutomation\bin\Release\Plugin\FirefoxPlugin.xpi";
            if (!_includeBrowserPlugin)
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(firefoxPluginFile));
                System.IO.File.Create(firefoxPluginFile).Close();
            }

            var firefoxFile = new File(
                firefoxPlugin,
                firefoxPluginFile);

            root.AddFile(firefoxFile);

            var citrixDriverFile = new File(
                citrixDriver,
                @"..\AppMan32.Citrix\bin\AppMan32.Citrix.dll");

            root.AddFile(citrixDriverFile);
            
            var comSpecEnvironmentVariable = new EnvironmentVariable("ComSpec", @"[WindowsFolder]system32\cmd.exe")
            {
                Id = "ComSpec",
                Action = EnvVarAction.create,
                Permanent = true,
                System = true
            };

            project.Add(comSpecEnvironmentVariable);

            // Setup custom actions.
            project.AddActions(InstallExecute.Sequence(chromePlugin, firefoxPlugin, citrixDriver, feature));
            project.WixSourceGenerated += FixServiceDeleteActions;
            project.WixSourceGenerated += FixFeatureOrder;
            project.WixSourceGenerated += RemoveManufacturerFolderComponent;
            project.WixSourceGenerated += doc => doc.Root.Select("Product").SetAttributeValue("Id", "*");
            project.WixSourceGenerated += doc =>
                Array.ForEach(doc.Root.FindAll("Component"), n => n.SetAttributeValue("Guid", Guid.NewGuid().ToString()));
            project.WixSourceGenerated += doc => doc.Root.Select("Product").SetAttributeValue("Language", "0");

            // Custom set of standard UI dialogs
            project.ManagedUI = new ManagedUI
            {
                InstallDirId = root.Id,
                Icon = Constants.BluePrismIcon
            };
            // Events for the Project
            project.UIInitialized += Project_UIInitialized;
            project.UILoaded += Project_UILoaded;

            project.ManagedUI.InstallDialogs
                .Add<VersionCheckDialog>()
                .Add<WelcomeDialog>()
                .Add<LicenceDialog>()
                .Add<InstallDirDialog>()
                .Add<FeaturesDialog>()
                .Add<ProgressDialog>()
                .Add<ExitDialog>()
                .Add<ErrorDialog>();

            project.ManagedUI.ModifyDialogs
                .Add<VersionCheckDialog>()
                .Add<MaintenanceTypeDialog>()
                .Add<FeaturesDialog>()
                .Add<ProgressDialog>()
                .Add<ExitDialog>()
                .Add<ErrorDialog>();

            project.LicenceFile = Constants.LicenseFile;

            project.LightOptions += " -sice:ICE80 ";

            // Build the standard "Enterprise" installer
            if (edition == Edition.Enterprise)
            {
                project.AddProperty(new Property("INSTALL_COST", (EnterpriseInstallCost).ToString()));
                project.OutFileName = $"Automate{_semanticVersion}_{project.Platform}";
                project.BuildMsi();
                return;
            }

            project.AddProperty(new Property("INSTALL_COST", (TrialInstallCost).ToString()));

            // Download SqlLocalDB if it doesn't exist.
            string localDBFile = System.IO.Path.Combine(Constants.PublishFolder, "SqlLocalDB.msi");
            if (!System.IO.File.Exists(localDBFile))
                new WebClient().DownloadFile("https://download.microsoft.com/download/E/F/2/EF23C21D-7860-4F05-88CE-39AA114B014B/SqlLocalDB.msi",
                    localDBFile);

            // Add the SqlLocalDB Msi file.
            installers.AddFile(new File(localDBFile));

            // Add the LocalDB reg key.
            project.AddRegValues(
               new RegValue(feature, RegistryHive.LocalMachine, Constants.RegistryRoot, "LocalDB", @"[INSTALLFOLDER]Installers\SqlLocalDB.msi") { Win64 = win64 },
               new RegValue(feature, RegistryHive.CurrentUser, Constants.RegistryRoot, "ConfigLocation", @"\%AppData\%\Blue Prism Limited\Automate V3\") { Win64 = win64 }
            );

            project.OutFileName = $"Automate_{edition.ToString()}{_semanticVersion}_{project.Platform}";

            // Build the "Trial" installer.
            project.BuildMsi();

        }

        private void Project_UIInitialized(SetupEventArgs e)
        {
            var locale = e.Session["LOCALE"];
            if (string.IsNullOrEmpty(locale))
            {
                if (CultureInfo.CurrentUICulture.Parent.Name != "fr" && !CultureHelper.IsLatinAmericanSpanish())
                    return;

                if (CultureHelper.IsLatinAmericanSpanish())
                    locale = CultureHelper.LatinAmericanSpanish;

                if (CultureInfo.CurrentUICulture.Parent.Name == "fr")
                    locale = "fr-FR";
            }

            if (CultureHelper.IsLatinAmericanSpanish(locale))
                locale = CultureHelper.LatinAmericanSpanish;

            if (locale != null && locale.Length >= 2 && locale.Substring(0, 2) == "fr")
            {
                locale = "fr-FR";
            }

            var helpers = new Helpers(null);
            helpers.ChangeLocale(locale);
        }


        private void FixServiceDeleteActions(XDocument document)
        {
            var ns = XNamespace.Get("http://schemas.microsoft.com/wix/2006/wi");
            var installExecuteSequence = document.Descendants(ns + "InstallExecuteSequence").First();
            installExecuteSequence.AddElement("DeleteServices", "", "NOT UPGRADINGPRODUCTCODE");
            installExecuteSequence.AddElement("InstallServices", "", "NOT UPGRADINGPRODUCTCODE");
        }

        private void FixFeatureOrder(XDocument document)
        {
            var order = new SortedDictionary<string, XElement>();

            var ns = XNamespace.Get("http://schemas.microsoft.com/wix/2006/wi");
            var features = document.Descendants(ns + "Feature");
            foreach (var feature in features)
            {
                var sortKey = feature.Attribute("SortKey");
                if (sortKey == null) continue;

                order.Add(sortKey.Value, feature);
                sortKey.Remove();
            }

            var product = document.Descendants(ns + "Product").First();
            foreach (var feature in order.Values)
            {
                feature.Remove();
                product.AddElement(feature);
            }

        }

        private void RemoveManufacturerFolderComponent(XDocument document)
        {
            var ns = XNamespace.Get("http://schemas.microsoft.com/wix/2006/wi");
            var componentsToRemove =
               document.Descendants(ns + "Component").Union(document.Descendants(ns + "ComponentRef"))
               .Where(x => x.Attribute("Id").Value == "ManufacturerFolder").ToList();

            foreach (var component in componentsToRemove)
                component.Remove();
        }

        private void Project_UILoaded(SetupEventArgs e)
        {
            const int width = 800, height = 720;
            var ui = e.ManagedUI;
            var f = (Form)ui;
            f.FormBorderStyle = FormBorderStyle.None;
            var loc = f.Location;
            f.Location = new Point(loc.X - (width - f.Width) / 2, loc.Y - (height - f.Height) / 2);
            ui.SetSize(width, height);

            UACRevealer.Enabled = true;
        }
    }
}
