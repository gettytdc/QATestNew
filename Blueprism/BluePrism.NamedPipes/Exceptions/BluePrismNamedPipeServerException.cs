using System;

namespace BluePrism.NamedPipes.Exceptions
{
    public class BluePrismNamedPipeServerException :Exception
    {
        public BluePrismNamedPipeServerException(string message)
            : base(message) { }
    }
}
