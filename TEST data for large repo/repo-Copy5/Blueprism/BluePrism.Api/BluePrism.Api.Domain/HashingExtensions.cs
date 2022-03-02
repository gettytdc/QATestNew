namespace BluePrism.Api.Domain
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using Newtonsoft.Json;

    public static class HashingExtensions
    {
        private const string PagingTokenValidationCode="lARwnogdNDJAItLaV3BQvg";

        internal static string ToHmacSha256(this IProvideHashCodeForPagingTokenValidation @this)
        {
            var stringToHash = JsonConvert.SerializeObject(@this);
            var enc = new UTF8Encoding();
            var codeBytes = enc.GetBytes(PagingTokenValidationCode);
            var messageBytes = enc.GetBytes(stringToHash);

            using (var hmac = new HMACSHA256(codeBytes))
            {
                var hashedBytes = hmac.ComputeHash(messageBytes);
                var hashedString = Convert.ToBase64String(hashedBytes);
                return hashedString; 
            }
        }
    }
}
