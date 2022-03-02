namespace BluePrism.Api.Setup
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Xml.Linq;
    using Common;
    using Common.Exceptions;
    using Common.IIS;
    using Dialogs;
    using Helpers;
    using WixSharp.Localization;
    using global::WixSharp;
    using global::WixSharp.CommonTasks;
    using static BluePrism.Api.Setup.CustomActions.CustomActions;
    using static Common.ProjectHelper;

    internal static class Program
    {
        private const string InstallingVersion = "INSTALLING_VERSION";

        [STAThread]
        internal static void Main(string[] args)
        {
            var signingCertificateHash = Environment.GetEnvironmentVariable("SigningCertificateHash");

#if DEBUG
            var buildConfiguration = "Debug";
#else
            var buildConfiguration = "Release";
#endif

            var customActionDllPath =
                $@"..\BluePrism.Api.Setup.CustomActions\bin\{buildConfiguration}\BluePrism.Api.Setup.CustomActions.dll";

            var createHarvester =
                Common.Harvester.GetFactoryMethod(new DirectoryTools());

            var projectVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            var project =
                createHarvester("Blue Prism API")
                .AddFilesFromDirectory(@"..\..\BluePrism.Api\bin\app.publish")
                .AddOuterEntity(ConfigurationHelper.GetProperties())
                .AddOuterEntity(new Property(InstallingVersion, projectVersion.ToString()))
                .AddOuterEntity(new IconFile(@"Resources\icon.ico"))
                .AddOuterEntity(new Property("ARPPRODUCTICON", "IconFile"))
                .AddOuterEntity(new ElevatedManagedAction(ConfigureWebsiteCertificate, customActionDllPath, Return.check, When.After, new Step("ConfigureIIs"), Condition.NOT_Installed)
                {
                    UsesProperties = GetElevatedPropertiesString(
                        MsiProperties.ApiIisSslCertificateId,
                        MsiProperties.ApiIisHostname,
                        MsiProperties.ApiIisPort)
                })
                .AddOuterEntity(new ElevatedManagedAction(UpdateWebsiteAppPool, customActionDllPath, Return.check, When.Before, Step.InstallFinalize, Condition.NOT_BeingRemoved))
                .AddOuterEntity(new ElevatedManagedAction(UpdateWebConfig, customActionDllPath, Return.check, When.After, Step.InstallFiles, Condition.NOT_BeingRemoved)
                {
                    UsesProperties = GetElevatedPropertiesString(
                        MsiProperties.ApiSqlServer,
                        MsiProperties.ApiSqlDatabaseName,
                        MsiProperties.ApiSqlUsername,
                        MsiProperties.ApiSqlPassword,
                        MsiProperties.ApiSqlAuthenticationMode,
                        MsiProperties.ApiBluePrismUsername,
                        MsiProperties.ApiBluePrismPassword)
                })
                .AddInnerEntity(new IISVirtualDir
                {
                    Name = WebAppProperties.WebAppName,
                    WebSite = new WebSite
                    {
                        InstallWebSite = true,
                        Description = WebAppProperties.WebAppName,
                        Addresses = new[]
                        {
                            new WebSite.WebAddress
                            {
                                Attributes = new Dictionary<string, string>
                                {
                                    { "Secure", "yes" },
                                    { "Port", $"[{MsiProperties.ApiIisPort}]"}
                                },
                            },
                        },
                    },
                    WebAppPool = new WebAppPool(WebAppProperties.AppPoolName,  "ManagedPipelineMode=Integrated"),
                })
                .AddOuterEntity(new Binary(new Id("dialog"), @"../BannerLeftInstallationBox.png"))
                .AddOuterEntity(new Binary(new Id("banner"), @"../BannerTopRightAlignedLogo.png"))
                .GetProject();

            project.UseWinFormsLocalization();

#if DEBUG   // Optimize build for debug
            project.Media[0].EmbedCab = true;
            project.Media[0].CompressionLevel = CompressionLevel.none;
#endif

            project.GUID = new Guid("3DFF5D28-494A-4B8A-B8CA-D48938575E4A");
            project.Version = projectVersion;

            project.BannerImage = @"../BannerTopRightAlignedLogo.png";

            project.WixSourceGenerated += WixSourceGeneratedEvent;

            project.MajorUpgrade = new MajorUpgrade
            {
                AllowSameVersionUpgrades = true,
                Schedule = UpgradeSchedule.afterInstallInitialize,
                DowngradeErrorMessage =
                    "A later version of [ProductName] is already installed. Setup will now exit."
            };

            project.ManagedUI = new ManagedUI { Icon = "../Icon32x32BPCloud.ico" };

            project.ManagedUI.InstallDialogs
                .Add<WelcomeDialog>()
                .Add<LicenceDialog>()
                .Add<InstallDirDialog>()
                .Add<IISDialog>()
                .Add<DatabaseDialog>()
                .Add<ProgressDialog>()
                .Add<ExitDialog>();

            project.ManagedUI.ModifyDialogs
                .Add<MaintenanceTypeDialog>()
                .Add<DatabaseDialog>()
                .Add<ProgressDialog>()
                .Add<ExitDialog>();

            ValidateAssemblyCompatibility();

            project.AddRegValue(new RegValue(project.DefaultFeature, RegistryHive.LocalMachine,
                @"Software\Blue Prism Limited\Blue Prism Api", "InstallDir", "[INSTALLDIR]"));

            project.UIInitialized += ProjectOnUiInitialized();

            LoadDefaultRefAssemblies(project, buildConfiguration);

            if (!string.IsNullOrWhiteSpace(signingCertificateHash))
            {
                project.ApplyDigitalSigning(signingCertificateHash,
                    "http://timestamp.digicert.com",
                    "Blue Prism API Installer");
            }

            project.LicenceFile = "../EULA.rtf";

            project.BuildMsi();
        }

        private static void LoadDefaultRefAssemblies(ManagedProject project, string buildConfiguration) =>
            project.DefaultRefAssemblies.Add(
                $@"..\BluePrism.Api.Setup.Common\bin\{buildConfiguration}\BluePrism.Api.Setup.Common.dll");

        private static ManagedProject.SetupEventHandler ProjectOnUiInitialized() =>
            setupEventArgs =>
            {
                CultureInfo culture;
                try
                {
                    culture = new CultureInfo(setupEventArgs.Session[MsiProperties.Locale]);
                }
                catch(CultureNotFoundException)
                {
                    culture = CultureInfo.CurrentUICulture;
                }

                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;

                ConfigurationHelper.SetProperties(setupEventArgs);
            };

        private static string GetElevatedPropertiesString(params string[] properties) =>
            string.Join(",", properties.Select(x => $"{x}=[{x}]"));

        private static void WixSourceGeneratedEvent(XDocument document)
        {
            try
            {
                var webSite = document.FindAll("WebSite")
                                      .First(x => x.HasAttribute("Description", WebAppProperties.WebAppName));
                var webApp = webSite.Parent.FindSingle("WebApplication");
                webApp.MoveTo(webSite);
            }
            catch(InvalidOperationException ex)
            {
                throw new InvalidWixXmlException("Invalid Wix Xml when attempting to assign app pool to website", ex);
            }
        }
    }
}
