using System.Collections.Generic;
using System.Linq;
using WixSharp;
using WixSharp.CommonTasks;

namespace BluePrism.Setup
{
    class Components
    {
        private Platform _platform;
        public Components(Platform platform)
        {
            _platform = platform;
        }

        /// <summary>
        /// Statics. The components that we can't harvest directly from the projects
        /// </summary>
        public IEnumerable<File> Statics(params Feature[] features)
        {
            yield return new File(@"..\bin\Activator32.exe");
            if (_platform == Platform.x64)
                yield return new File(@"..\bin\Activator64.exe");
            yield return new File(@"..\AForge\AForge.Imaging.dll") { Features = features };
            yield return new File(@"..\bin\Autofac.dll") { Features = features };
            yield return new File(@"..\bin\Autofac.Extras.NLog.dll") { Features = features };
            yield return new File(@"..\bin\System.Threading.Tasks.Extensions.dll") { Features = features };
            yield return new File(@"..\bin\System.Threading.Channels.dll") { Features = features };
            yield return new File(@"..\bin\System.Threading.ThreadPool.dll") { Features = features };
            yield return new File(@"..\bin\System.Xml.ReaderWriter.dll") { Features = features };
            yield return new File(@"..\bin\System.Runtime.CompilerServices.Unsafe.dll") { Features = features };
            yield return new File(@"..\bin\System.Buffers.dll") { Features = features };
            yield return new File(@"..\bin\System.Memory.dll") { Features = features };
            yield return new File(@"..\bin\System.Numerics.Vectors.dll") { Features = features };
            yield return new File(@"..\bin\Microsoft.Bcl.AsyncInterfaces.dll") { Features = features };
            yield return new File(@"..\bin\System.ValueTuple.dll") { Features = features };
            yield return new File(@"..\bin\AutomateS.exe",
                new FileAssociation("bprelease", "application/custom", "open", @"/nosplash /import ""%1") { Advertise = true, Icon = Constants.ReleaseIcon },
                new FileAssociation("bpskill", "application/custom", "open", @"/nosplash /import ""%1") { Advertise = true, Icon = Constants.SkillsIcon },
                new FileAssociation("bpobject", "application/custom", "open", @"/nosplash /import ""%1") { Advertise = true, Icon = Constants.ObjectIcon },
                new FileAssociation("bpprocess", "application/custom", "open", @"/nosplash /import ""%1") { Advertise = true, Icon = Constants.ProcessIcon });
            yield return new File(@"..\bin\Automatonymous.dll") { Features = features };
            yield return new File(@"..\bin\BluePrism.Common.Security.dll") { Features = features };
            yield return new File(@"..\bin\BluePrism.DatabaseInstaller.dll") { Features = features };
            yield return new File(@"..\bin\BluePrism.DigitalWorker.Messages.Commands.dll") { Features = features };
            yield return new File(@"..\bin\BluePrism.DigitalWorker.Messages.Events.dll") { Features = features };
            yield return new File(@"..\bin\BluePrism.DigitalWorker.MessagingClient.dll") { Features = features };
            yield return new File(@"..\bin\BluePrism.Cirrus.Common.MassTransit.dll") { Features = features };
            yield return new File(@"..\bin\Microsoft.EntityFrameworkCore.dll") { Features = features };
            yield return new File(@"..\bin\BluePrism.Cirrus.Sessions.SessionService.Messages.Commands.dll") { Features = features };
            yield return new File(@"..\bin\BluePrism.Cirrus.Sessions.SessionService.MessagingClient.dll") { Features = features };
            yield return new File(@"..\bin\BluePrism.Server.Domain.Models.Standard.dll") { Features = features };
            yield return new File(@"..\bin\Func.dll") { Features = features };
            yield return new File(@"..\bin\BpInjAgent.dll");
            yield return new File(@"..\bin\BpInjAgentD.dll");
            if (_platform == Platform.x64)
            {
                yield return new File(@"..\bin\BpInjAgent64.dll");
                yield return new File(@"..\bin\BpInjAgent64D.dll");
            }
            yield return new File(@"..\bin\FeatureToggle.Core.dll") { Features = features };
            yield return new File(@"..\bin\FeatureToggle.dll") { Features = features };
            yield return new File(@"..\bin\GreenPipes.dll") { Features = features };
            yield return new File(@"..\bin\HtmlAgilityPack.dll") { Features = features };
            yield return new File(@"..\bin\IdentityModel.dll") { Features = features };
            yield return new File(@"..\bin\IdentityModel.OidcClient.dll") { Features = features };
            yield return new File(@"..\bin\Jeffijoe.MessageFormat.dll") { Features = features };
            yield return new File(@"..\bin\LiveCharts.dll") { Features = features };
            yield return new File(@"..\bin\LiveCharts.Wpf.dll") { Features = features };
            yield return new File(@"..\bin\ManagedSpyLib.dll");
            yield return new File(@"..\bin\ManagedSpyLibNet4.dll");
            yield return new File(@"..\bin\MassTransit.dll") { Features = features };
            yield return new File(@"..\bin\MassTransit.RabbitMqTransport.dll") { Features = features };
            yield return new File(@"..\bin\MassTransit.AutofacIntegration.dll") { Features = features };
            yield return new File(@"..\bin\Microsoft.CodeDom.Providers.DotNetCompilerPlatform.dll") { Features = features };
            yield return new File(@"..\bin\Microsoft.Extensions.Configuration.dll") { Features = features };
            yield return new File(@"..\bin\Microsoft.Extensions.Configuration.Abstractions.dll") { Features = features };
            yield return new File(@"..\bin\Microsoft.Extensions.Configuration.Binder.dll") { Features = features };
            yield return new File(@"..\bin\Microsoft.Extensions.DependencyInjection.Abstractions.dll") { Features = features };
            yield return new File(@"..\bin\Microsoft.Extensions.Logging.Abstractions.dll") { Features = features };
            yield return new File(@"..\bin\Microsoft.Extensions.Logging.dll") { Features = features };
            yield return new File(@"..\bin\Microsoft.Extensions.Options.dll") { Features = features };
            yield return new File(@"..\bin\Microsoft.Extensions.Primitives.dll") { Features = features };
            yield return new File(@"..\bin\Microsoft.IdentityModel.JsonWebTokens.dll") { Features = features };
            yield return new File(@"..\bin\Microsoft.IdentityModel.Logging.dll") { Features = features };
            yield return new File(@"..\bin\Microsoft.IdentityModel.Protocols.dll") { Features = features };
            yield return new File(@"..\bin\Microsoft.IdentityModel.Protocols.OpenIdConnect.dll") { Features = features };
            yield return new File(@"..\bin\Microsoft.IdentityModel.Tokens.dll") { Features = features };
            yield return new File(@"..\bin\Mono.Security.dll") { Features = features };
            yield return new File(@"..\bin\netstandard.dll") { Features = features };
            yield return new File(@"..\bin\NewId.dll") { Features = features };
            yield return new File(@"..\bin\Newtonsoft.Json.Bson.dll") { Features = features };
            yield return new File(@"..\bin\Newtonsoft.Json.dll") { Features = features };
            yield return new File(@"..\bin\NLog.dll") { Features = features };
            yield return new File(@"..\bin\NLog.Extensions.Logging.dll") { Features = features };
            yield return new File(@"..\bin\NodaTime.dll") { Features = features };
            yield return new File(@"..\bin\System.Data.SqlClient.dll") { Features = features };
            yield return new File(@"..\bin\System.Diagnostics.Contracts.dll") { Features = features };
            yield return new File(@"..\bin\System.Diagnostics.Debug.dll") { Features = features };
            yield return new File(@"..\bin\System.Diagnostics.DiagnosticSource.dll") { Features = features };
            yield return new File(@"..\bin\System.Diagnostics.FileVersionInfo.dll") { Features = features };
            yield return new File(@"..\bin\System.Diagnostics.Process.dll") { Features = features };
            yield return new File(@"..\bin\System.Diagnostics.StackTrace.dll") { Features = features };
            yield return new File(@"..\bin\System.Diagnostics.TextWriterTraceListener.dll") { Features = features };
            yield return new File(@"..\bin\System.Diagnostics.Tools.dll") { Features = features };
            yield return new File(@"..\bin\System.Diagnostics.TraceSource.dll") { Features = features };
            yield return new File(@"..\bin\System.Diagnostics.Tracing.dll") { Features = features };
            yield return new File(@"..\bin\System.IdentityModel.Tokens.Jwt.dll") { Features = features };
            yield return new File(@"..\bin\System.Reactive.dll") { Features = features };
            yield return new File(@"..\bin\System.Reflection.dll") { Features = features };
            yield return new File(@"..\bin\System.Reflection.Extensions.dll") { Features = features };
            yield return new File(@"..\bin\System.Reflection.Primitives.dll") { Features = features };
            yield return new File(@"..\bin\System.Runtime.dll") { Features = features };
            yield return new File(@"..\bin\System.Runtime.Extensions.dll") { Features = features };
            yield return new File(@"..\bin\System.Runtime.Handles.dll") { Features = features };
            yield return new File(@"..\bin\System.Runtime.InteropServices.dll") { Features = features };
            yield return new File(@"..\bin\System.Runtime.InteropServices.RuntimeInformation.dll") { Features = features };
            yield return new File(@"..\bin\System.Runtime.Numerics.dll") { Features = features };
            yield return new File(@"..\bin\System.Text.Encodings.Web.dll") { Features = features };
            yield return new File(@"..\bin\System.IO.Compression.dll") { Features = features };
            yield return new File(@"..\bin\websocket-sharp.dll") { Features = features };
            yield return new File(@"..\lib\SslCertBinding.Net\SslCertBinding.Net.dll") { Features = features };
            yield return new File(@"..\lib\UIAutomation\Interop.UIAutomationClient.dll") { Features = features };
            yield return new File(@"..\ScintillaNET\SciLexer.dll");
            yield return new File(@"..\ScintillaNET\ScintillaNET.dll") { Features = features };
            yield return new File(@"..\bin\grpc_csharp_ext.x64.dll") { Features = features };
            yield return new File(@"..\bin\grpc_csharp_ext.x86.dll") { Features = features };
            yield return new File(@"..\lib\gRpc\BluePrism.ClientServerResources.Grpc.Proto.dll") { Features = features };
            yield return new File(@"..\bin\Grpc.Core.Api.dll") { Features = features };
            yield return new File(@"..\bin\Grpc.Core.dll") { Features = features };
            yield return new File(@"..\bin\Google.Protobuf.dll") { Features = features };
            yield return new File(@"..\bin\System.Linq.Async.dll") { Features = features };
            yield return new File(@"..\bin\CsvHelper.dll") { Features = features };
            yield return new File(@"..\bin\Microsoft.Bcl.HashCode.dll") { Features = features };
            yield return new File(@"..\bin\BPC.IMS.Messages.Events.dll") { Features = features };
            yield return new File(@"..\bin\BouncyCastle.Crypto.dll") { Features = features };
        }

        public IEnumerable<Dir> SubDirectories()
        {
            yield return new Dir("skills");

            yield return new Dir(new Id("Plugins_Dir"), "Plugins");

            yield return new Dir("VBO", new Files(@"..\VBO\*.xml"));

            yield return new Dir("Tesseract", new Files(@"..\bin\Tesseract\*.*"));

            yield return new Dir("OcrPlus", new Files(@"..\bin\OcrPlus\*.*"));

            yield return new Dir("roslyn", new Files(@"..\bin\roslyn\*.*"));

            yield return new Dir("Help", new Files(@"..\Help\*.*"));

            var cefSharp = new Dir("x86", CefSharpStatics().ToArray());
            cefSharp.AddDir(new Dir("Locales", CefSharpLocaleFiles().ToArray()));

            yield return cefSharp;
        }

        public IEnumerable<File> OutlookInterop(Feature feature)
        {
            yield return new File(feature, @"..\lib\OutlookAutomation\Microsoft.Office.Interop.Outlook.dll");
        }

        public IEnumerable<File> GoogleSheets(Feature feature)
        {
            yield return new File(feature, @"..\lib\GoogleSheets\Google.Apis.dll");
            yield return new File(feature, @"..\lib\GoogleSheets\Google.Apis.Auth.dll");
            yield return new File(feature, @"..\lib\GoogleSheets\Google.Apis.Core.dll");
            yield return new File(feature, @"..\lib\GoogleSheets\Google.Apis.Sheets.v4.dll");
        }

        public IEnumerable<File> DocumentProcessing(params Feature[] features)
        {
            yield return new File(@"..\bin\BluePrism.DocumentProcessing.Api.Models.dll") { Features = features };
            yield return new File(@"..\bin\BluePrism.DocumentProcessing.ServerPlugin.Messages.dll") { Features = features };
            yield return new File(@"..\bin\BluePrism.Utilities.Functional.dll") { Features = features };
            yield return new File(@"..\bin\RabbitMQ.Client.dll") { Features = features };
            yield return new File(@"..\bin\System.Net.Http.dll") { Features = features };
            yield return new File(@"..\bin\System.ComponentModel.Annotations.dll");
        }

        public IEnumerable<File> CefSharpStatics()
        {
            yield return new File(@"..\bin\x86\CefSharp.BrowserSubprocess.Core.dll");
            yield return new File(@"..\bin\x86\CefSharp.BrowserSubprocess.exe");
            yield return new File(@"..\bin\x86\CefSharp.dll");
            yield return new File(@"..\bin\x86\CefSharp.Core.dll");
            yield return new File(@"..\bin\x86\CefSharp.WinForms.dll");
            yield return new File(@"..\bin\x86\chrome_elf.dll");
            yield return new File(@"..\bin\x86\libcef.dll");
            yield return new File(@"..\bin\x86\libEGL.dll");
            yield return new File(@"..\bin\x86\libGLESv2.dll");
            yield return new File(@"..\bin\x86\icudtl.dat");
            yield return new File(@"..\bin\x86\natives_blob.bin");
            yield return new File(@"..\bin\x86\snapshot_blob.bin");
            yield return new File(@"..\bin\x86\v8_context_snapshot.bin");
            yield return new File(@"..\bin\x86\cef.pak");
            yield return new File(@"..\bin\x86\cef_100_percent.pak");
            yield return new File(@"..\bin\x86\cef_200_percent.pak");
            yield return new File(@"..\bin\x86\cef_extensions.pak");
            yield return new File(@"..\bin\x86\devtools_resources.pak");
        }

        public IEnumerable<File> CefSharpLocaleFiles()
        {
            foreach (string languagePack in 
                     Internationalisation.Locales.CefLanguagePacks)
            {
                yield return new File(@"..\bin\x86\locales\" + languagePack);
            }
        }

        public IEnumerable<File> LoginAgentInstallers()
        {
            if (_platform == Platform.x86)
                yield return new File(@"..\bin\LoginAgent_x86.msi");
            yield return new File(@"..\bin\LoginAgent_x64.msi");
        }

        public IEnumerable<File> Media()
        {
            yield return new File(@"..\BluePrism.Automate\Help\apihtmlcss.xsl");
            yield return new File(Constants.BluePrismIcon);
            yield return new File(Constants.LicenseFile);
        }

        public IEnumerable<Dir> Shortcuts()
        {
            yield return new Dir(@"%Desktop%",
                new ExeFileShortcut(Constants.ProductName, @"[INSTALLFOLDER]AutomateS.exe", "")
                {
                    WorkingDirectory = "[INSTALLFOLDER]",
                    IconFile = Constants.BluePrismIcon
                });

            yield return new Dir(@"%ProgramMenu%",
                new ExeFileShortcut(Constants.ProductName, @"[INSTALLFOLDER]AutomateS.exe", "")
                {
                    WorkingDirectory = "[INSTALLFOLDER]",
                    IconFile = Constants.BluePrismIcon
                });
        }

        public IEnumerable<ProjectReference> Projects(params Feature[] features)
        {
            yield return new ProjectReference(@"..\ApplicationManager\AMI\AMI.vbproj") { Features = features };
            yield return new ProjectReference(@"..\ApplicationManager\ApplicationManagerUtilities\ApplicationManagerUtilities.vbproj") { Features = features };
            yield return new ProjectReference(@"..\ApplicationManager\AppMan\AppMan.csproj");
            yield return new ProjectReference(@"..\ApplicationManager\BluePrism.AppMan\BluePrism.AppMan.csproj");
            yield return new ProjectReference(@"..\ApplicationManager\AppMan32\AppMan32.csproj");
            if (_platform == Platform.x64)
                yield return new ProjectReference(@"..\ApplicationManager\AppMan64\AppMan64.csproj");
            yield return new ProjectReference(@"..\ApplicationManager\BPDiagnostics\BPDiagnostics.vbproj") { Content = false };
            yield return new ProjectReference(@"..\ApplicationManager\Citrix\Citrix.vbproj") { Features = features };
            yield return new ProjectReference(@"..\ApplicationManager\ClientComms\ClientComms.vbproj") { Features = features };
            yield return new ProjectReference(@"..\ApplicationManager\ClientCommsI\ClientCommsI.vbproj") { Features = features };
            yield return new ProjectReference(@"..\ApplicationManager\ContainerApp\ContainerApp.csproj") { Features = features };
            yield return new ProjectReference(@"..\ApplicationManager\DDE\DDE.csproj") { Features = features };
            yield return new ProjectReference(@"..\ApplicationManager\HTML\HTML.vbproj") { Features = features };
            yield return new ProjectReference(@"..\ApplicationManager\JavaAccessBridge\JABWrapper\JABWrapper.vbproj") { Features = features };
            yield return new ProjectReference(@"..\ApplicationManager\Terminal\BPATerminalEmulation.vbproj") { Features = features };
            yield return new ProjectReference(@"..\ApplicationManager\WindowSpy\WindowSpy.vbproj") { Features = features };

            yield return new ProjectReference(@"..\BluePrism.Automate\BluePrism.Automate.vbproj") { Content = false };
            yield return new ProjectReference(@"..\BluePrism.Automate\AutomateAppCore\AutomateAppCore.vbproj") { Features = features };
            yield return new ProjectReference(@"..\BluePrism.Automate\AutomateC\AutomateC.vbproj");
            yield return new ProjectReference(@"..\BluePrism.Automate\AutomateConfig\AutomateConfig.vbproj") { Content = false };
            yield return new ProjectReference(@"..\BluePrism.Automate\BPScheduler\BPScheduler.csproj") { Features = features };
            yield return new ProjectReference(@"..\BluePrism.Automate\BPServer\BPServer.csproj") { Features = features };
            yield return new ProjectReference(@"..\BluePrism.Automate\BPServerService\BPServerService.csproj") { Binaries = false, Features = features }; // Server binaries installed seperately
            yield return new ProjectReference(@"..\AutomateControls\AutomateControls\AutomateControls.csproj") { Features = features };
            yield return new ProjectReference(@"..\BluePrism.AutomateProcessCore\BluePrism.AutomateProcessCore.vbproj") { Features = features };
            yield return new ProjectReference(@"..\AutomateUI\AutomateUI.vbproj") { Features = features };

            yield return new ProjectReference(@"..\BluePrism.ActiveDirectoryUserSearcher\BluePrism.ActiveDirectoryUserSearcher.csproj") { Features = features };
            yield return new ProjectReference(@"..\BluePrism.BrowserAutomation\BluePrism.BrowserAutomation.csproj") { Features = features };
            yield return new ProjectReference(@"..\BluePrism.Caching\BluePrism.Caching.csproj") { Features = features };
            yield return new ProjectReference(@"..\BluePrism.Core\BluePrism.Core.csproj") { Features = features };
            yield return new ProjectReference(@"..\BluePrism.Data\BluePrism.Data.csproj") { Features = features };
            yield return new ProjectReference(@"..\BluePrism.DataPipeline\BluePrism.DataPipeline.csproj") { Features = features };
            yield return new ProjectReference(@"..\BluePrism.Datapipeline.Consumers\BluePrism.DataPipeline.Logstash.csproj") { Features = features };
            yield return new ProjectReference(@"..\BluePrism.DigitalWorker\BluePrism.DigitalWorker.csproj") { Features = features };
            yield return new ProjectReference(@"..\BluePrism.DocumentProcessing.Integration\BluePrism.DocumentProcessing.Integration.csproj") { Features = features };
            yield return new ProjectReference(@"..\BluePrism.ExternalLoginBrowser\BluePrism.ExternalLoginBrowser.csproj") { Features = features };
            yield return new ProjectReference(@"..\BluePrism.Images\BluePrism.Images.csproj") { Content = false, Features = features };
            yield return new ProjectReference(@"..\BluePrism.Skills\BluePrism.Skills.csproj") { Features = features };
            yield return new ProjectReference(@"..\BluePrism.StartUp\BluePrism.StartUp.csproj") { Features = features };
            yield return new ProjectReference(@"..\BluePrism.UIAutomation\BluePrism.UIAutomation.csproj") { Features = features };
            yield return new ProjectReference(@"..\BluePrism.WorkQueueAnalysis\BluePrism.WorkQueueAnalysis.csproj") { Features = features };
            yield return new ProjectReference(@"..\BluePrism.MessagingHost\BluePrism.NativeMessagingHost.csproj") { Features = features };
            yield return new ProjectReference(@"..\BluePrism.NamedPipes\BluePrism.NamedPipes.csproj") { Features = features };
            yield return new ProjectReference(@"..\BluePrism.NativeMessaging\BluePrism.NativeMessaging.csproj") { Features = features };
            yield return new ProjectReference(@"..\BluePrism.BrowserAutomation.WebMessages\BluePrism.BrowserAutomation.WebMessages.csproj") { Features = features };

            yield return new ProjectReference(@"..\BluePrism.AuthenticationServerSynchronization\BluePrism.AuthenticationServerSynchronization.csproj") { Features = features };
            
            yield return new ProjectReference(@"..\BPCoreLib\BPCoreLib.vbproj") { Features = features };
            yield return new ProjectReference(@"..\CharMatching\CharMatching.csproj") { Features = features };
            yield return new ProjectReference(@"..\Internationalisation\Internationalisation.csproj") { Features = features };
            yield return new ProjectReference(@"..\LocaleTools\LocaleTools.csproj") { Features = features };
            yield return new ProjectReference(@"..\LogPlugins\LogPlugins.csproj") { SourceDir = @"..\bin\plugins", TargetDir="Plugins_Dir" };
            yield return new ProjectReference(@"..\MonthCalendarControl\MonthCalendarControl.csproj") { Features = features };
            
            yield return new ProjectReference(@"..\BluePrism.Server.Core\BluePrism.Server.Core.vbproj") { Features = features };
            yield return new ProjectReference(@"..\BluePrism.Server.Domain.Models\BluePrism.Server.Domain.Models.vbproj") { Features = features };

            yield return new ProjectReference(@"..\BluePrism.ClientServerResources.Core\BluePrism.ClientServerResources.Core.csproj") { Features = features };
            yield return new ProjectReference(@"..\BluePrism.ClientServerResources.Grpc\BluePrism.ClientServerResources.Grpc.csproj") { Features = features };
            yield return new ProjectReference(@"..\BluePrism.ClientServerResources.Wcf\BluePrism.ClientServerResources.Wcf.csproj") { Features = features };

            yield return new ProjectReference(@"..\AppMan.Service\AppMan.Service.csproj") { Features = features };
        }

        public IEnumerable<Merge> MergeModules(Feature feature)
        {
            yield return new Merge(feature, @"..\redist\Microsoft_VC141_CRT_x86.msm") { FileCompression = true, Id = new Id("VCRedistx86") };
            yield return new Merge(feature, @"..\redist\Microsoft_VC141_MFC_x86.msm") { FileCompression = true, Id = new Id("MFCRedistx86") };
            if (_platform == Platform.x64)
            {
                yield return new Merge(feature, @"..\redist\Microsoft_VC141_CRT_x64.msm") { FileCompression = true, Id = new Id("VCRedistx64") };
                yield return new Merge(feature, @"..\redist\Microsoft_VC141_MFC_x64.msm") { FileCompression = true, Id = new Id("MFCRedistx64") };
            }
        }

        public IEnumerable<File> BluePrismServerService(Feature feature)
        {
            yield return new File(feature, @"..\bin\BPServerService.exe")
            {
                ServiceInstaller = new ServiceInstaller
                {
                    Id = "BPServerService",
                    Name = "Blue Prism Server",
                    DisplayName = "Blue Prism Server",
                    Description = "The Blue Prism Server Service",
                    ErrorControl = SvcErrorControl.normal,
                    Start = SvcStartType.demand,
                    Type = SvcType.ownProcess,
                    Account = "[SERVICEACCOUNT]",
                    Password = "[SERVICEPASSWORD]",
                    StopOn = SvcEvent.Uninstall,
                    RemoveOn = SvcEvent.Uninstall,
                    ConfigureServiceTrigger = ConfigureServiceTrigger.None
                }
            };

            yield return new File(feature, @"..\bin\BPServerService.exe.config");
        }

        public IEnumerable<EventSource> EventSources()
        {
            yield return new EventSource
            {
                Name = "Blue Prism General",
                Log = "Blue Prism",
                EventMessageFile = "[NETFRAMEWORK40FULLINSTALLROOTDIR]EventLogMessages.dll"
            };

            yield return new EventSource
            {
                Name = "Blue Prism Resource (Port 8181)",
                Log = "Blue Prism",
                EventMessageFile = "[NETFRAMEWORK40FULLINSTALLROOTDIR]EventLogMessages.dll"
            };

            yield return new EventSource
            {
                Name = "Blue Prism Archiver",
                Log = "Blue Prism",
                EventMessageFile = "[NETFRAMEWORK40FULLINSTALLROOTDIR]EventLogMessages.dll"
            };
        }

        public IEnumerable<RegKey> RegistryKeys(Project project, Feature feature)
        {
            yield return new RegKey(feature, RegistryHive.LocalMachine, Constants.RegistryRoot,
                new RegValue("InstallDir", "[INSTALLFOLDER]"),
                new RegValue("ProductCode", $"{project.GUID}"),
                new RegValue("Version", $"{project.Version}"),
                new RegValue("InstallLocale", "[LOCALE]"),
                new RegValue("APIVersion", Helpers.InstallerApiVersion.ToString())
                )
            { Win64 = (_platform == Platform.x64) };

            yield return new RegKey(feature, RegistryHive.LocalMachine,
                @"SYSTEM\ControlSet001\services\eventlog\Blue Prism",
                new RegValue(null, "File", @"%SystemRoot%\system32\winevt\Logs\BluePrism.evtx"),
                new RegValue(null, "MaxSize", 16780000)
                )
            { Win64 = (_platform == Platform.x64) };
        }
    }
}
