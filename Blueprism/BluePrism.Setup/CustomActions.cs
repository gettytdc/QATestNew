using Microsoft.Deployment.WindowsInstaller;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace BluePrism.Setup
{

    public class CustomActions
    {
        private const string FirefoxRegistryKeyPath = @"Software\Mozilla\Firefox\Extensions";
        private static readonly string CitrixVirtualDriverExStringName = "VirtualDriverEx";
        private static readonly Helpers Helper = new Helpers();
        private const string CitrixVirtualDriverExBluePrismEntryValue = "Blue Prism";

        /// <summary>
        /// values under the HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Google\Chrome\ExtensionInstallForcelist key are named incrementally - 1, 2, 3 etc.
        /// This action checks the values and returns the next available value name we can use for our plugin. If our plugin already has an entry under this key,
        /// it returns the name of that so it can be re-used.
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        [CustomAction]
        public static ActionResult ChromePlugin_GetNextForceInstallRegistryValueName(Session session)
        {

            try
            {
                session.Log("Begin ChromePlugin_GetNextForceInstallRegistryValueName custom action");
                var pluginId = session["ChromeBrowserPluginID"];

                var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Google\Chrome\ExtensionInstallForcelist");

                if (key == null)
                {
                    session["CHROME_PLUGIN_REG_VALUE"] = "1";
                    return ActionResult.Success;
                }

                var existingEntry = key.GetValueNames().FirstOrDefault(x => key.GetValue(x).ToString().Contains(pluginId));

                if (existingEntry != null)
                {
                    session.Log("ChromePlugin_GetNextForceInstallRegistryValueName custom action: Found existing registry entry for our chrome plugin. Re-using existing value. Name: " + existingEntry);
                    session["CHROME_PLUGIN_REG_VALUE"] = existingEntry;
                    return ActionResult.Success;
                }

                var valueNames = key.GetValueNames().Where(x => int.TryParse(x, out int n)).ToList();
                if (!valueNames.Any())
                {
                    session.Log(@"ChromePlugin_GetNextForceInstallRegistryValueName custom action: No existing values found at key SOFTWARE\Policies\Google\Chrome\ExtensionInstallForcelist. Returning '1'");
                    session["CHROME_PLUGIN_REG_VALUE"] = "1";
                    return ActionResult.Success;
                }

                var nextEntry = valueNames.Select(int.Parse).Max() + 1;
                session.Log(@"ChromePlugin_GetNextForceInstallRegistryValueName custom action: Existing registry entries found at key SOFTWARE\Policies\Google\Chrome\ExtensionInstallForcelist. Appending new value with name: " + nextEntry);
                session["CHROME_PLUGIN_REG_VALUE"] = nextEntry.ToString();
                return ActionResult.Success;

            }
            catch (Exception e)
            {
                session.Log("Error in ChromePlugin_GetNextForceInstallRegistryValueName custom action: " + e.Message);
                return ActionResult.Failure;
            }

        }

        [CustomAction]
        public static ActionResult FirefoxCreateNativeMessagingConfig(Session session)
        {
            try
            {
                var config = File.ReadAllText($@"{Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}\Blue Prism Limited\Blue Prism\com.blueprism.messaging-firefox-manifest.json");
                config = config.Replace("[InstallFolder]", session.CustomActionData["BP_INSTALL_FOLDER"].Replace(@"\", "/"));
                config = config.Replace("[FirefoxExtensionId]", session.CustomActionData["FirefoxBrowserPluginID"]);
                File.WriteAllText($@"{Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}\Blue Prism Limited\Blue Prism\com.blueprism.messaging-firefox-manifest.json", config);
                return ActionResult.Success;
            }
            catch (Exception e)
            {
                session.Log("Error in FirefoxCreateNativeMessagingConfig custom action: " + e.Message);
                return ActionResult.Failure;
            }
        }

        [CustomAction]
        public static ActionResult ChromeEdgeCreateNativeMessagingConfig(Session session)
        {
            try
            {
                var config = File.ReadAllText($@"{Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}\Blue Prism Limited\Blue Prism\com.blueprism.messaging-manifest.json");
                config = config.Replace("[ChromeExtensionId]", session.CustomActionData["ChromeBrowserPluginID"]);
                config = config.Replace("[EdgeExtensionId]", session.CustomActionData["EdgeBrowserPluginID"]);
                config = config.Replace("[InstallFolder]", session.CustomActionData["BP_INSTALL_FOLDER"].Replace(@"\", "/"));
                File.WriteAllText($@"{Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}\Blue Prism Limited\Blue Prism\com.blueprism.messaging-manifest.json", config);
                return ActionResult.Success;
            }
            catch (Exception e)
            {
                session.Log("Error in ChromeCreateNativeMessagingConfig custom action: " + e.Message);
                return ActionResult.Failure;
            }
        }


        /// <summary>
        /// Installs the preference file required for our extension to work into the firefox install directory.
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        [CustomAction]
        public static ActionResult FirefoxPlugin_InstallPreferenceFile(Session session)
        {

            try
            {

                var installDir = session.CustomActionData["FIREFOX_DIR"];

                if (string.IsNullOrEmpty(installDir))
                {
                    // if firefox is not installed take no action.
                    return ActionResult.Success;
                }

                var prefDir = installDir + @"browser\defaults\preferences\";
                var prefSourceDir = session.CustomActionData["BP_INSTALL_FOLDER"];

                var prefFile = session.CustomActionData["FIREFOX_PREF_FILE_NAME"];

                var prefFilePath = prefSourceDir + prefFile;

                // if the preference file to copy doesn't exist for some reason, 
                // just return.
                if (!File.Exists(prefFilePath))
                {
                    return ActionResult.Success;
                }

                // The preference directory in the firefox folder may not exist. Create it if this is the case.
                if (!Directory.Exists(prefDir))
                {
                    Directory.CreateDirectory(prefDir);
                }

                // Copy the preference file.
                File.Copy(prefFilePath, prefDir + prefFile, true);

                return ActionResult.Success;

            }
            catch (Exception e)
            {
                session.Log("Error in FirefoxPlugin_InstallPreferenceFile custom action: " + e.Message);
                return ActionResult.Failure;
            }

        }

        [CustomAction]
        public static ActionResult FirefoxPlugin_LoadPlugin(Session session)
        {
            try
            {

                var firefoxDir = session.CustomActionData["FIREFOX_DIR"];
                var bpInstallDir = Path.Combine(session.CustomActionData["BP_INSTALL_FOLDER"], "FirefoxPlugin.xpi");
                var firefoxPath = Path.Combine(firefoxDir, "firefox.exe");

                if (!string.IsNullOrEmpty(firefoxPath) && File.Exists(firefoxPath))
                {
                    var processName = Process.Start(firefoxPath, $"\"{bpInstallDir}\"")?.ProcessName;
                    Thread.Sleep(12000);

                    if (!string.IsNullOrEmpty(processName))
                    {
                        var firefoxProcesses = Process.GetProcessesByName(processName);

                        foreach (var process in firefoxProcesses)
                        {
                            process.CloseMainWindow();
                            process.Kill();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                session.Log("Error in FirefoxPlugin_LoadPlugin custom action: " + e.Message);
            }

            return ActionResult.Success;
        }


        /// <summary>
        /// Removes the preference file required for our extension to work from the firefox install directory.
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        [CustomAction]
        public static ActionResult FirefoxPlugin_RemovePreferenceFile(Session session)
        {

            try
            {

                var installDir = session.CustomActionData["FIREFOX_DIR"];
                var prefDir = installDir + @"browser\defaults\preferences\";

                var prefFile = session.CustomActionData["FIREFOX_PREF_FILE_NAME"];
                var filePath = Path.Combine(prefDir, prefFile);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception e)
            {
                session.Log("Error in FirefoxPlugin_InstallPreferenceFile custom action: " + e.Message);
            }

            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult FirefoxPlugin_GetFirefoxInstallDirectory(Session session)
        {
            try
            {

                session.Log("Begin FirefoxPlugin_GetFirefoxInstallDirectory custom action");
                var x64Path = $@"{Environment.ExpandEnvironmentVariables("%ProgramW6432%") }\Mozilla Firefox\";
                var x86Path = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)}\Mozilla Firefox\";

                if (File.Exists(x86Path + "firefox.exe"))
                {
                    session["FIREFOX_DIR"] = x86Path;
                }
                else if (File.Exists(x64Path + "firefox.exe"))
                {
                    session["FIREFOX_DIR"] = x64Path;
                }

                return ActionResult.Success;

            }
            catch (Exception e)
            {
                session.Log("Error in FirefoxPlugin_GetFirefoxInstallDirectory custom action: " + e.Message);
                return ActionResult.Failure;
            }
        }

        [CustomAction]
        public static ActionResult FirefoxPlugin_WriteFirefoxPluginRegistrySettings(Session session)
        {
            try
            {
                WriteKeyToRegistry(RegistryView.Registry32, session);
                WriteKeyToRegistry(RegistryView.Registry64, session);

                return ActionResult.Success;
            }
            catch (Exception e)
            {
                session.Log("ERROR: Unable to execute FirefoxPlugin_WriteFirefoxPluginRegistrySettings custom action: {0}", e.Message);
                return ActionResult.Failure;
            }
        }

        private static void WriteKeyToRegistry(RegistryView view, Session session)
        {
            var key = session.CustomActionData["FIREFOX_PLUGIN_ID"];
            var installationDirectory = session.CustomActionData["BP_INSTALL_FOLDER"];
            var value = Path.Combine(installationDirectory, "FirefoxPlugin.xpi");

            using (var registryKeyBase = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
            using (var registryKey = registryKeyBase.CreateSubKey(FirefoxRegistryKeyPath, writable: true))
            {
                registryKey?.SetValue(key, value);
                session.Log($"INFO: Successfully written registry key `{key} - {value}` to `{FirefoxRegistryKeyPath}`");
            }
        }

        [CustomAction]
        public static ActionResult FirefoxPlugin_DeleteFirefoxPluginRegistrySettings(Session session)
        {
            try
            {
                DeleteRegistryKey(RegistryView.Registry32, session);
                DeleteRegistryKey(RegistryView.Registry64, session);

                return ActionResult.Success;
            }
            catch (Exception e)
            {
                session.Log("ERROR: Unable to execute FirefoxPlugin_DeleteFirefoxPluginRegistrySettings custom action: {0}", e.Message);
                return ActionResult.Failure;
            }
        }

        private static void DeleteRegistryKey(RegistryView view, Session session)
        {
            var key = session.CustomActionData["FIREFOX_PLUGIN_ID"];

            using (var registryKeyBase = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
            using (var registryKey = registryKeyBase.OpenSubKey(FirefoxRegistryKeyPath, writable: true))
            {
                try
                {
                    registryKey?.DeleteValue(key, throwOnMissingValue: true);
                    session.Log($"INFO: Successfully deleted registry key `{key}` from `{FirefoxRegistryKeyPath}`");
                }
                catch (ArgumentException e)
                {
                    session.Log($"INFO: Unable to find registry key `{key}` at {FirefoxRegistryKeyPath} to delete: {e.Message}");
                }
            }
        }

        [CustomAction]
        public static ActionResult DeleteManufacturerFolder(Session session)
        {
            try
            {
                //Get the parent of the installDirectory, if it is Blue Prism Limited and empty, delete it
                var installDir = session.CustomActionData["ManufacturerFolder"];
                var parentDirectory = Directory.GetParent(installDir).Parent;

                if (parentDirectory?.Name == "Blue Prism Limited" && parentDirectory.Exists)
                {
                    parentDirectory.Delete();
                }
            }
            catch (Exception e)
            {
                //Log the exception but don't rethrow
                session.Log($"Failed DeleteManufacturerFolder Action. Exception Details. {Environment.NewLine}{e.Message}");
            }
            return ActionResult.Success;
        }


        [CustomAction]
        public static ActionResult CitrixAutomationAddVirtualDriverExRegistryKey(Session session)
        {
            try
            {
                session.Log("Begin CitrixAutomationAddVirtualDriverExRegistryKey custom action");

                var key = Registry.LocalMachine.OpenSubKey(string.Concat(Helper.X64CitrixRegistryNode, @"\ICA Client\Engine\Configuration\Advanced\Modules\ICA 3.0"), true) ?? Registry.LocalMachine.OpenSubKey(string.Concat(Helper.X86CitrixRegistryNode, @"\ICA Client\Engine\Configuration\Advanced\Modules\ICA 3.0"), true);

                if (key != null)
                {
                    var virtualDriverEx = key.GetValue(CitrixVirtualDriverExStringName)?.ToString();
                    session.Log($"In CitrixAutomationAddVirtualDriverExRegistryKey custom action |{virtualDriverEx}|");
                    if (!string.IsNullOrWhiteSpace(virtualDriverEx))
                    {
                        var valueList = virtualDriverEx.Split(',').Select(x => x.Trim()).ToList();
                        if (!valueList.Contains(CitrixVirtualDriverExBluePrismEntryValue))
                        {
                            valueList.Add(CitrixVirtualDriverExBluePrismEntryValue);
                            key.SetValue(CitrixVirtualDriverExStringName, string.Join(",", valueList));
                        }
                    }
                    else
                    {
                        key.SetValue(CitrixVirtualDriverExStringName, CitrixVirtualDriverExBluePrismEntryValue);
                    }

                    var moduleKey = Registry.LocalMachine.OpenSubKey(string.Concat(Helper.X64CitrixRegistryNode, @"\ICA Client\Engine\Configuration\Advanced\Modules"), true) ?? Registry.LocalMachine.OpenSubKey(string.Concat(Helper.X86CitrixRegistryNode, @"\ICA Client\Engine\Configuration\Advanced\Modules"), true);

                    var bluePrismSubKey = moduleKey.CreateSubKey("Blue Prism");
                    bluePrismSubKey.SetValue("DriverName", "AppMan32.Citrix.dll");
                    bluePrismSubKey.SetValue("DriverNameWin16", "AppMan32.Citrix.dll");
                    bluePrismSubKey.SetValue("DriverNameWin32", "AppMan32.Citrix.dll");
                }
                else
                {
                    session.Log("Warning in CitrixAutomationAddVirtualDriverExRegistryKey custom action: ICA Registry node not found");
                }

                return ActionResult.Success;
            }
            catch (Exception e)
            {
                session.Log($"Error in  CitrixAutomationAddVirtualDriverExRegistryKey custom action: {e.Message}");
                return ActionResult.Failure;
            }

        }
        [CustomAction]
        public static ActionResult DeleteCitrixAutomationVirtualDriverExRegistryKey(Session session)
        {
            try
            {
                session.Log("Begin CitrixAutomationAddRegistryKeys custom action");

                var key = Registry.LocalMachine.OpenSubKey(string.Concat(Helper.X64CitrixRegistryNode, @"\ICA Client\Engine\Configuration\Advanced\Modules\ICA 3.0"), true) ?? Registry.LocalMachine.OpenSubKey(string.Concat(Helper.X86CitrixRegistryNode, @"\ICA Client\Engine\Configuration\Advanced\Modules\ICA 3.0"), true);

                if (key != null)
                {
                    var virtualDriverEx = key.GetValue(CitrixVirtualDriverExStringName)?.ToString();
                    var newValue = virtualDriverEx == null ?
                        string.Empty :
                        string.Join(",", virtualDriverEx.Split(',').Select(x => x.Trim()).Where(l => string.Compare(l, "blue prism", StringComparison.InvariantCultureIgnoreCase) != 0));

                    key.SetValue(CitrixVirtualDriverExStringName, newValue);
                }
                else
                {
                    session.Log("Warning in DeleteCitrixAutomationAddRegistryKeys custom action: ICA Registry node not found");
                }
                var moduleKey = Registry.LocalMachine.OpenSubKey(string.Concat(Helper.X64CitrixRegistryNode, @"\ICA Client\Engine\Configuration\Advanced\Modules"), true) ?? Registry.LocalMachine.OpenSubKey(string.Concat(Helper.X86CitrixRegistryNode, @"\ICA Client\Engine\Configuration\Advanced\Modules"), true);

                moduleKey.DeleteSubKey("Blue Prism");                
            }
            catch (Exception e)
            {
                session.Log("Error in  DeleteCitrixAutomationAddRegistryKeys custom action: " + e.Message);
            }
            return ActionResult.Success;

        }       

        [CustomAction]
        public static ActionResult CitrixAutomationAddVirtualDriverFileToCitrixInstallDir(Session session)
        {
            try
            {
                var citrixICAInstallFolder = Helper.GetCitrixInstallFolder();
                if (!string.IsNullOrEmpty(citrixICAInstallFolder))
                {
                    session.Log("Begin CitrixAutomationAddVirtualDriverFileToCitrixInstallDir custom action");
                    session.Log($"Begin CitrixAutomationAddVirtualDriverFileToCitrixInstallDir Install Directory for Citrix {citrixICAInstallFolder}");

                    var bpInstallDir = session.CustomActionData["BP_INSTALL_FOLDER"];

                    var sourceFile = Path.Combine(bpInstallDir, "AppMan32.Citrix.dll");
                    var destinationFile = Path.Combine(citrixICAInstallFolder, "AppMan32.Citrix.dll");
                    File.Copy(sourceFile, destinationFile, true);
                }
                return ActionResult.Success;
            }
            catch (Exception e)
            {
                session.Log($"Error in  CitrixAutomationAddVirtualDriverFileToCitrixInstallDir custom action: {e.Message}");
                return ActionResult.Failure;
            }
        }

        [CustomAction]
        public static ActionResult CitrixAutomationDeleteVirtualDriverFileToCitrixInstallDir(Session session)
        {
            try
            {
                session.Log("Begin CitrixAutomationDeleteVirtualDriverFileToCitrixInstallDir custom action");
                var citrixICAInstallFolder = Helper.GetCitrixInstallFolder();

                var citrixDriverFile = Path.Combine(citrixICAInstallFolder, "AppMan32.Citrix.dll" );
                File.Delete(citrixDriverFile);
            }
            catch (Exception e)
            {
                session.Log($"Warning in CitrixAutomationDeleteVirtualDriverFileToCitrixInstallDir custom action: {e.Message}");
                session.Log($"Warning in CitrixAutomationDeleteVirtualDriverFileToCitrixInstallDir custom action: Citrix driver file may not have been deleted.");
            }
            return ActionResult.Success;
        }
    }
}
