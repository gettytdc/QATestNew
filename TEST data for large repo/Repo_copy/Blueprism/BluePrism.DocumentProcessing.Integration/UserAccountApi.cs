namespace BluePrism.DocumentProcessing.Integration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Core.Configuration;
    using Core.Utility;
    using Utilities.Functional;
    using Newtonsoft.Json;

    public class UserAccountApi : IUserAccountApi
    {
        private readonly IApiCommunicator _apiCommunicator;
        private readonly string _apiEndpoint;

        public UserAccountApi(IApiCommunicator apiCommunicator, IAppSettings appSettings)
        {
            _apiCommunicator = apiCommunicator;
            _apiEndpoint = appSettings["DocumentProcessing.ApiEndpoint"];
        }

        public IEnumerable<string> GetUserAccounts(string token) =>
            _apiCommunicator.SendHttpRequest($"{_apiEndpoint}Account", "GET", token)
                .Use(r => r
                    .Tee(_apiCommunicator.ThrowErrorOnInvalidResponse)
                    .GetResponseStream()
                    ?.ReadEntireStream()
                    .Map(JsonConvert.DeserializeObject<(string Name, Guid Id)[]>)
                    .Select(x => x.Name));

        public void CreateUser(string token, string username, string password)
        {
            var body = (username, password).Map<object, string>(JsonConvert.SerializeObject);
            _apiCommunicator.SendHttpRequestWithBody($"{_apiEndpoint}Account", "POST", body, token)
                .Use(_apiCommunicator.ThrowErrorOnInvalidResponse);
        }
    }
}