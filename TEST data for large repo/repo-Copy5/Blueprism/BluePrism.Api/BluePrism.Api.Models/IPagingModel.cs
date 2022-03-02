namespace BluePrism.Api.Models
{
    public interface IPagingModel<T>
    {
        PagingTokenModel<T> PagingToken { get; }
    }
}
