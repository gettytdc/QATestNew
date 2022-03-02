namespace BluePrism.Api.CommonTestClasses
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using Microsoft.IdentityModel.Tokens;

    public static class AuthHelper
    {
        private const string TestKey = "VhBgtBldXAvLFqIbtdTtSwgNqzjlPnpMyPZRLcHEgSpyQPSCEDKvLhSYxkpRhTQbSrCjjz";

        public static SecurityKey SecurityKey => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestKey));

        public static string GenerateToken(string issuer,
                                           string audience,
                                           DateTime? notBefore = null,
                                           DateTime? expires = null,
                                           IEnumerable<Claim> claims = null)
        {
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims ?? Array.Empty<Claim>()),
                    NotBefore = notBefore ?? DateTime.UtcNow,
                    Expires = expires ?? DateTime.UtcNow.AddHours(1),
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256),
                };
                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
        }
    }
}
