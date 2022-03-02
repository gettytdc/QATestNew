using System;

namespace BluePrism.ClientServerResources.Core.Exceptions
{
    [Serializable]
    public class ClientAlreadyExistsException
        : Exception
    {
        public Guid UserId { get; }

        public ClientAlreadyExistsException(Guid g)
            => UserId = g;
    }
}
