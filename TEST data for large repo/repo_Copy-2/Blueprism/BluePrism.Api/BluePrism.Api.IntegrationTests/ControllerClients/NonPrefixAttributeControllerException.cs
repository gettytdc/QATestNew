namespace BluePrism.Api.IntegrationTests.ControllerClients
{
    using System;

    public class NonPrefixAttributeControllerException : Exception
    {
        public NonPrefixAttributeControllerException(string message) : base(message) {}
    }
}
