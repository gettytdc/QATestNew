using System;
using System.Runtime.Serialization;

namespace BluePrism.ClientServerResources.Core.Enums
{
    [Flags, DataContract(Namespace ="bp")]
    public enum InstructionalConnectionModes
    {
        [EnumMember(Value = "N")]
        None = 0,
        [EnumMember(Value = "C")]
        Certificate = 1,
        [EnumMember(Value = "W")]
        Windows = 2,
        [EnumMember(Value = "I")]
        Insecure = 3
    }
}
