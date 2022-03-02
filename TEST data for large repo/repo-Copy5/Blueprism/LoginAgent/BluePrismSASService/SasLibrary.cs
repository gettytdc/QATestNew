using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BluePrism.LoginAgent.Sas
{
    internal static class SasLibrary
    {
        /// <summary>
        /// Send the secure attention sequence (CTRL + ALT + DEL) programatically
        /// </summary>
        [DllImport("sas.dll", SetLastError = true)]
        public static extern void SendSAS(bool AsUser);
    }
}
