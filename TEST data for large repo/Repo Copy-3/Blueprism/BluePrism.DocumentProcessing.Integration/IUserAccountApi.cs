namespace BluePrism.DocumentProcessing.Integration
{
    using System.Collections.Generic;

    public interface IUserAccountApi
    {
        IEnumerable<string> GetUserAccounts(string token);
        void CreateUser(string token, string username, string password);
    }
}