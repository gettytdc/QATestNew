namespace BluePrism.Api
{
    using Microsoft.IdentityModel.Tokens;

    public interface ISigningKeyResolver
    {
        SecurityKey[] GetSigningKeys(string kid);
    }
}
