using System;
using System.Runtime.Serialization;

namespace BluePrism.Core.Resources
{
    /// <summary>
    /// Gives information about a resource. Can be ORed together.
    /// When adding new attributes ensure that an equivalent entry is added to the
    /// database table BPAResourceAttribute
    /// </summary>
    [Flags]
    [DataContract(Namespace = "bp")]
    public enum ResourceAttribute
    {
        /// <summary>
        /// The default, empty value.
        /// </summary>
        [EnumMember(Value = "N")]
        None = 0,
        /// <summary>
        /// Processes which are retired do not appear in the user interface.
        /// </summary>
        [EnumMember(Value = "R")]
        Retired = 1,
        /// <summary>
        /// Indicates that a resource is being used as a local desktop instance,
        /// which does not communicate with other resources. This is important from
        /// a licensing perspective. Resources without this attribute are assumed to
        /// be 'universal'.
        /// </summary>
        [EnumMember(Value = "L")]
        Local = 2,
        /// <summary>
        /// Indicates that this resource is in use for debugging purposes. This is
        /// important because such resources should not be counted from a licensing
        /// perspective.
        /// </summary>
        [EnumMember(Value = "D")]
        Debug = 4,
        /// <summary>
        /// Indicates that this resource is pool, rather than a physical resource.
        /// </summary>
        [EnumMember(Value = "P")]
        Pool = 8,
        /// <summary>
        /// Indicates that this resource is a Login Agent (i.e. the device is in a
        /// logged off state).
        /// </summary>
        [EnumMember(Value = "A")]
        LoginAgent = 16,
        /// <summary>
        /// Indicates that this resource is Private, started without /public but with
        /// either of the /user or /sso options. Note that resources started with
        /// /public and either of the /user or /sso options are still public.
        /// </summary>
        [EnumMember(Value = "X")]
        Private = 32,
        /// <summary>
        /// Is this resource the one created by Blue Prism Client
        /// </summary>
        [EnumMember(Value = "I")]
        DefaultInstance = 64,

    }
}
