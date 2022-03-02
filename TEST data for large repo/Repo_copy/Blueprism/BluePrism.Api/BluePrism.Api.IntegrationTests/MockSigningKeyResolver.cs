namespace BluePrism.Api.IntegrationTests
{
    using CommonTestClasses;
    using Microsoft.IdentityModel.Tokens;

        public class MockSigningKeyResolver: ISigningKeyResolver
        {
            public SecurityKey[] GetSigningKeys(string kid) => new [] { AuthHelper.SecurityKey };
        }
}
