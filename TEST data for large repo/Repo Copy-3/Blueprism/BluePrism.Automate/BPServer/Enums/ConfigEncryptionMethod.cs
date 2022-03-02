using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrism.BPServer.Enums
{
    [Flags]
    public enum ConfigEncryptionMethod
    {
        BuiltIn = 0,
        OwnCertificate = 1
    }
}
