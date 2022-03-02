using System;
using System.Text;

namespace BluePrism.LoginAgent.Sas.GroupPolicy
{
    public class ComputerGroupPolicyObject : GroupPolicyObject
    {
        /// <summary>
        /// Opens the default GPO for the local computer 
        /// </summary>
        public ComputerGroupPolicyObject(GroupPolicyObjectSettings options = null)
        {
            options = options ?? new GroupPolicyObjectSettings();
            TryCatch(() => _instance.OpenLocalMachineGPO(options.Flag),
                "Unable to open local machine GPO");
            IsLocal = true;
        }
        /// <summary>
        /// Opens the default GPO for the specified remote computer
        /// </summary>
        /// <param name="computerName">Name of the remote computer in the format "\\ComputerName"</param>
        public ComputerGroupPolicyObject(string computerName, GroupPolicyObjectSettings options = null)
        {
            options = options ?? new GroupPolicyObjectSettings();
            TryCatch(() => _instance.OpenRemoteMachineGPO(computerName, options.Flag),
                "Unable to open GPO on remote machine '{0}'", computerName);
            IsLocal = false;
        }

        /// <summary>
        /// Returns true if the object is on the local machine
        /// </summary>
        public readonly bool IsLocal;
     
   
        protected override uint GetPathTo(uint section, StringBuilder stringBuilder, int MaxLength) =>
            _instance.GetFileSysPath(section, stringBuilder, MaxLength);
       
    }
}
