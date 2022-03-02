using System;

namespace BluePrism.ClientServerResources.Core.Data
{
    public class TokenValidationInfo
    {
        public bool Success { get; private set; }
        public string Reason { get; private set; }
        public Guid UserId { get; private set; }
        public string Name { get; private set; }
        private TokenValidationInfo()
        {
        }

        public static TokenValidationInfo SuccessTokenInfo(string name,Guid userId)
        {
            return new TokenValidationInfo()
            {
                Success = true,
                UserId = userId,
                Reason = string.Empty,
                Name = name
            };
        }
        
        public static TokenValidationInfo FailureTokenInfo(string reason)
        {
            return new TokenValidationInfo()
            {
                Reason = reason,
                Success = false
            };
        }
    }
}
