using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using BluePrism.LoginAgent.Utilities;

namespace BluePrism.LoginAgent.Sas.GroupPolicy
{
    public abstract class GroupPolicyObject
    {
        /// <summary>
        /// The snap-in that processes .pol files
        /// </summary>
        private static readonly Guid RegistryExtension = new Guid(0x35378EAC, 0x683F, 0x11D2, 0xA8, 0x9A, 0x00, 0xC0, 0x4F, 0xBB, 0xCF, 0xA2);
        private static readonly Guid AssemblyGuid = new Guid(AssemblyInfoHelper.GetAssemblyAttribute<GuidAttribute>().Value);

        private const uint S_OK = 0;
        protected const int MaxLength = 1024;
        protected IGroupPolicyObject _instance = null;        

        internal GroupPolicyObject()
        {
            _instance = GetInstance();
        }

        /// <summary>
        /// Saves the specified registry policy settings to disk and updates the revision number of the GPO.
        /// This saves both machine and user level settings.
        /// </summary>
        public void Save()
        {
            TryCatch(() => _instance.Save(true, true, RegistryExtension, AssemblyGuid),
                "Error saving machine settings");
            TryCatch(() => _instance.Save(false, true, RegistryExtension, AssemblyGuid),
                "Error saving user settings");
        }
      
        
        /// <summary>
        /// Retrieves the root of the registry key for the specified GPO section
        /// </summary>
        public RegistryKey GetRootRegistryKey(GroupPolicySection section)
        {
            _instance.GetRegistryKey((uint)section, out IntPtr key);
            //"Unable to get section '{0}'", Enum.GetName(typeof(GroupPolicySection), section));
            
            var safeHandle = new SafeRegistryHandle(key, true);
            return RegistryKey.FromHandle(safeHandle, RegistryView.Default);
        }
       
        /// <summary>
        /// Gets the string of the file system path from the object to the root of the specified GPO section
        /// </summary>
        protected string GetStringFromGroupPolicyObjectPath(GroupPolicySection section)
        {
            var stringBuilder = new StringBuilder(MaxLength);
            TryCatch(() => GetPathTo((uint)section, stringBuilder, MaxLength), 
                    "Unable to retrieve path to section '{0}'", 
                    Enum.GetName(typeof(GroupPolicySection), section));

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Retrieves the file system path from the object to the root of the specified GPO section
        /// </summary>
        protected abstract uint GetPathTo(uint section, StringBuilder stringBuilder, int MaxLength);

        protected static IGroupPolicyObject GetInstance()
        {
            return WithSingleThreadedApartmentCheck(() =>
            {
                var concrete = new GPClass();
                return (IGroupPolicyObject)concrete;
            });
        }
        protected static T WithSingleThreadedApartmentCheck<T>(Func<T> operation)
        {
            try
            {
                return operation();
            }
            catch (InvalidCastException e) when (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
            {
                throw new RequiresSingleThreadedApartmentException(e);
            }
        }

        protected static void TryCatch(Func<uint> operation, string formatMessage, params object[] arguments)
        {
            uint result = operation();
            if (result != S_OK)
            {
                string message = String.Format(formatMessage, arguments);
                throw new GroupPolicyException(String.Format("{0}. Error code {1} (see WinError.h)", message, result));
            }
        }
        protected string GetStringFromGroupPolicyObjectName(Func<StringBuilder, int, uint> getName, string errorMessage)
        {
            var stringBuilder = new StringBuilder();
            TryCatch(() => getName(stringBuilder, MaxLength), errorMessage);
            return stringBuilder.ToString();
        }
    }
}