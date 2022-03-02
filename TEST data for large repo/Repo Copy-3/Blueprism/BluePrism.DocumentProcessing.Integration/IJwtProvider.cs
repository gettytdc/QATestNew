namespace BluePrism.DocumentProcessing.Integration
{
    using System;
    using System.Collections.Generic;

    public interface IJwtProvider
    {
        string GetToken(string subject, IEnumerable<string> roles, string secret, DateTime elapses);
    }
}