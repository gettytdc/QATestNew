namespace BluePrism.Api.Services
{
    public interface ITokenAccessor
    {
        string TokenString { get; }

        void SetToken(string token);
    }
}
