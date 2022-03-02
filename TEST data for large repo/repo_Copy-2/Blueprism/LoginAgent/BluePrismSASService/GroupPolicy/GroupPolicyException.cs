using System;

namespace BluePrism.LoginAgent.Sas.GroupPolicy
{
    public class GroupPolicyException : Exception
    {
        internal GroupPolicyException(string message) : base(message) { }
    }
}
