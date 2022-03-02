namespace BluePrism.Api.Domain
{
    using System;

    [Flags]
    public enum ResourceAttribute
    {
        None = 0,
        Retired = 1 << 0,
        Local = 1 << 1,
        LoginAgent = 1 << 4,
        Private = 1 << 5,
        DefaultInstance = 1 << 6,
    }
}
