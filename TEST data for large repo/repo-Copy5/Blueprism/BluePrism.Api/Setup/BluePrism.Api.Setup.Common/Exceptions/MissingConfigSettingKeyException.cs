namespace BluePrism.Api.Setup.Common.Exceptions
{
    using System;

    public class MissingConfigSettingKeyException : Exception
    {
        public MissingConfigSettingKeyException(string message) : base(message)
        {
        }
    }
}
