using BluePrism.ClientServerResources.Core.Data;

namespace BluePrism.ClientServerResources.Core.Interfaces
{
    public interface ITokenValidator
    {
        TokenValidationInfo Validate(string token); 
    }
}
