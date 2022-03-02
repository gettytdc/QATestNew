namespace BluePrism.Api.Domain.Errors
{
    public class ResourceNotFoundError : NotFoundError
    {
        public ResourceNotFoundError() : base("Could not find a resource with this ID.")
        {
        }
    }
}
