namespace BluePrism.ClientServerResources.Core.Interfaces
{
    public interface ITokenRegistration
    {
        string RegisterTokenWithExpiry(int expiryTimeSeconds);
    }
}
