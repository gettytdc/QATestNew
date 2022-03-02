using System.Collections.Generic;
using WixSharp;

namespace BluePrism.Setup
{
    class InstallExecute
    {
        public static IEnumerable<Action> Sequence(Feature chrome, Feature firefox, Feature citrixDriver, Feature bluePrism)
        {
            yield return new ManagedAction(
                CustomActions.FirefoxPlugin_GetFirefoxInstallDirectory,
                Return.check,
                When.Before,
                Step.CostInitialize,
                null);

            yield return new ManagedAction(
                CustomActions.ChromePlugin_GetNextForceInstallRegistryValueName,
                Return.check,
                When.After,
                Step.CostFinalize,
                chrome.ShallInstall());

            yield return new CustomActionRef(
                "WixCloseApplications",
                When.After,
                Step.InstallInitialize);

            yield return new ElevatedManagedAction(
                CustomActions.ChromeEdgeCreateNativeMessagingConfig,
                Return.check,
                When.After,
                Step.InstallExecute,
                bluePrism.ShallInstall()) { UsesProperties = "BP_INSTALL_FOLDER=[INSTALLFOLDER], ChromeBrowserPluginID=[ChromeBrowserPluginID], EdgeBrowserPluginID=[EdgeBrowserPluginID]" };

            yield return new ElevatedManagedAction(
                CustomActions.FirefoxCreateNativeMessagingConfig,
                Return.check,
                When.After,
                Step.InstallExecute,
                bluePrism.ShallInstall()) { UsesProperties = "BP_INSTALL_FOLDER=[INSTALLFOLDER], FirefoxBrowserPluginID=[FirefoxBrowserPluginID]" };

            yield return new ElevatedManagedAction(
                CustomActions.FirefoxPlugin_InstallPreferenceFile,
                Return.check,
                When.After,
                Step.InstallFiles,
                firefox.ShallInstall()) { UsesProperties = "FIREFOX_DIR=[FIREFOX_DIR], BP_INSTALL_FOLDER=[INSTALLFOLDER], FIREFOX_PREF_FILE_NAME=[FIREFOX_PREF_FILE_NAME]" };

            yield return new ElevatedManagedAction(
                CustomActions.FirefoxPlugin_WriteFirefoxPluginRegistrySettings,
                Return.check,
                When.After,
                Step.PreviousAction,
                firefox.ShallInstall()) { UsesProperties = "BP_INSTALL_FOLDER=[INSTALLFOLDER], FIREFOX_PLUGIN_ID=[FirefoxBrowserPluginID]" };

            yield return new ElevatedManagedAction(
                CustomActions.FirefoxPlugin_LoadPlugin,
                Return.check,
                When.After,
                Step.PreviousAction,
                firefox.ShallInstall()) { Impersonate = true, UsesProperties = "BP_INSTALL_FOLDER=[INSTALLFOLDER], FIREFOX_DIR=[FIREFOX_DIR]" };

            yield return new ElevatedManagedAction(
                CustomActions.CitrixAutomationAddVirtualDriverExRegistryKey,
                Return.check,
                When.After,
                Step.WriteRegistryValues,
                citrixDriver.ShallInstall());

            yield return new ElevatedManagedAction(
                CustomActions.CitrixAutomationAddVirtualDriverFileToCitrixInstallDir,
                Return.check,
                When.After,
                Step.InstallFiles,
                citrixDriver.ShallInstall())
            { UsesProperties = "BP_INSTALL_FOLDER=[INSTALLFOLDER]" };

            yield return new ElevatedManagedAction(
                CustomActions.FirefoxPlugin_RemovePreferenceFile,
                Return.check,
                When.After,
                Step.PreviousAction,
                firefox.ShallUninstall()) { UsesProperties = "FIREFOX_DIR=[FIREFOX_DIR], FIREFOX_PREF_FILE_NAME=[FIREFOX_PREF_FILE_NAME]" };

            yield return new ElevatedManagedAction(
                CustomActions.FirefoxPlugin_DeleteFirefoxPluginRegistrySettings,
                Return.check,
                When.After,
                Step.InstallFiles,
                firefox.ShallUninstall()) { UsesProperties = "FIREFOX_PLUGIN_ID=[FirefoxBrowserPluginID]" };

            yield return new ElevatedManagedAction(
                CustomActions.DeleteManufacturerFolder,
                Return.check,
                When.After,
                Step.RemoveFolders, Condition.BeingUninstalled)
            { UsesProperties = "ManufacturerFolder=[INSTALLFOLDER]" };

            yield return new ElevatedManagedAction(
                CustomActions.DeleteCitrixAutomationVirtualDriverExRegistryKey,
                Return.check,
                When.After,
                Step.RemoveFolders,
                citrixDriver.ShallUninstall());

            yield return new ElevatedManagedAction(
                CustomActions.CitrixAutomationDeleteVirtualDriverFileToCitrixInstallDir,
                Return.check,
                When.After,
                Step.RemoveFolders,
                citrixDriver.ShallUninstall());
        }
    }
}
