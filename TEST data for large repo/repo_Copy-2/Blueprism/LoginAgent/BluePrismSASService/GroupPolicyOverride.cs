using BluePrism.LoginAgent.Sas.GroupPolicy;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Threading;
using BluePrism.LoginAgent.Utilities;

namespace BluePrism.LoginAgent.Sas
{
    /// <summary>
    /// Class to change group policy to allow SAS to be raised from services and bypassing the 
    /// legal notice screen.
    /// </summary>
    class GroupPolicyOverride
    {
        private readonly EventLogger _eventLogger;
        private readonly bool _overrideSas;
        private readonly bool _overrideLegalMessage;

        public GroupPolicyOverride(bool overrideLegal, bool overrideSas, EventLogger eventLogger)
        {
            _overrideLegalMessage = overrideLegal;
            _overrideSas = overrideSas;
            _eventLogger = eventLogger;
        }

        /// <summary>
        /// Runs the group policy setting in a different thread as it 
        /// has to be an STAThread to work.
        /// </summary>
        public void SetGroupPolicy()
        {
            if (_overrideLegalMessage || _overrideSas)
            {
                var thread = new Thread(SetGroupPolicyWorker);
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();
            }
        }

        /// <summary>
        /// Updates the group policy to allow services to bypass the SAS programatically.
        /// </summary>
        private void SetGroupPolicyWorker()
        {
            try
            {
                _eventLogger.WriteLogEntry(EventLogEntryType.Information, "Altering group policy");

                const string GroupPolicy_KeyPath = "Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System";
                const string GroupPolicy_KeyName_Sas = "SoftwareSASGeneration";
                const string GroupPolicy_KeyName_LegalTitle = "legalnoticecaption";
                const string GroupPolicy_KeyName_LegalMessage = "legalnoticetext";

                var computerGroupPolicy = new ComputerGroupPolicyObject();
                
                using (var machineKey = computerGroupPolicy.GetRootRegistryKey(GroupPolicySection.Machine))
                using (var terminalServicesKey = machineKey.CreateSubKey(GroupPolicy_KeyPath))
                { 
                        if (_overrideSas)
                            terminalServicesKey.SetValue(GroupPolicy_KeyName_Sas, 00000001, RegistryValueKind.DWord);

                        if (_overrideLegalMessage)
                        {
                            terminalServicesKey.SetValue(GroupPolicy_KeyName_LegalTitle, string.Empty, RegistryValueKind.String);
                            terminalServicesKey.SetValue(GroupPolicy_KeyName_LegalMessage, string.Empty, RegistryValueKind.String);
                        }
                }

                computerGroupPolicy.Save();
                                
            }
            catch (Exception ex)
            {
                _eventLogger.WriteLogEntry(EventLogEntryType.Warning, $"Error altering group policy. {ex}");
            }

        }

    }
}


