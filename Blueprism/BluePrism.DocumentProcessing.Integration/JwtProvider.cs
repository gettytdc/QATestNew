namespace BluePrism.DocumentProcessing.Integration
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Core.Conversion;
    using Utilities.Functional;
    using Newtonsoft.Json;

    public class JwtProvider : IJwtProvider
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1);

        private readonly string _header =
            new Dictionary<string, string> {
                    { "alg", "HS256"},
                    { "typ", "JWT" }
                }
                .Map(JsonConvert.SerializeObject)
                .Map(EncodeString);

        public string GetToken(string subject, IEnumerable<string> roles, string secret, DateTime elapses) =>
            new[]
                {
                    _header,
                    GetBody(subject, roles, elapses)
                }
                .Map(x => string.Join(".", x))
                .Map(x => (Value: x, Hash: x.Map(HashValue(secret.Map(Encoding.UTF8.GetBytes)))))
                .Map(x => $"{x.Value}.{x.Hash}");

        private static string GetBody(string subject, IEnumerable<string> roles, DateTime elapses) =>
            new Dictionary<string, object>
                {
                    {"http://schemas.microsoft.com/ws/2008/06/identity/claims/role", roles},
                    {"sub", subject},
                    {"jti", Guid.NewGuid().ToString()},
                    {
                        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
                        Guid.NewGuid().ToString()
                    },
                    {"exp", (int) (elapses - UnixEpoch).TotalSeconds},
                    {"iss", "http://localhost/"},
                    {"aud", "http://localhost/"}
                }
                .Map(JsonConvert.SerializeObject)
                .Map(EncodeString);

        private static string EncodeString(string value) =>
            value
                .Map(Encoding.UTF8.GetBytes)
                .Map(UrlBase64Convertor.ToBase64String);

        private static Func<byte[], byte[]> HmacSha256(byte[] secret) => data =>
            new System.Security.Cryptography.HMACSHA256(secret).ComputeHash(data);

        private static Func<string, string> HashValue(byte[] secret) => value =>
            value
                .Map(Encoding.UTF8.GetBytes)
                .Map(HmacSha256(secret))
                .Map(UrlBase64Convertor.ToBase64String);
    }
}