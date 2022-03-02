namespace BluePrism.Api.Domain
{
    public interface IProvideHashCodeForPagingTokenValidation
    {
        string GetHashCodeForValidation();
    }
}
