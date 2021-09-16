namespace BluePrism.DocumentProcessing.Integration.UnitTests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using Utilities.Functional;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using BluePrism.Utilities.Testing;

    [TestFixture]
    public class UserAccountApiTests : UnitTestBase<UserAccountApi>
    {
        [Test]
        public void GetUserAccountReturnsAccountsOnSuccess()
        {
            var serverResult = new[]
            {
                (Name: "Test Name", Id: Guid.NewGuid())
            };
            var serverResultJson = JsonConvert.SerializeObject(serverResult);

            var mockResponse = GetMock<IHttpWebResponse>();
            mockResponse
                .Setup(m => m.GetResponseStream())
                .Returns(serverResultJson.Map(Encoding.UTF8.GetBytes).Map(x => new MemoryStream(x)));
            mockResponse
                .SetupGet(m => m.StatusCode)
                .Returns(HttpStatusCode.OK);

            GetMock<IApiCommunicator>()
                .Setup(m => m.SendHttpRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(mockResponse.Object);

            var result = ClassUnderTest.GetUserAccounts(string.Empty);

            result.Should().BeEquivalentTo(serverResult.Select(x => x.Name));
        }

        [Test]
        public void GetUserAccountHandlesEmptyCollection()
        {
            var serverResult = new string[0];
            var serverResultJson = JsonConvert.SerializeObject(serverResult);

            var mockResponse = GetMock<IHttpWebResponse>();
            mockResponse
                .Setup(m => m.GetResponseStream())
                .Returns(serverResultJson.Map(Encoding.UTF8.GetBytes).Map(x => new MemoryStream(x)));
            mockResponse
                .SetupGet(m => m.StatusCode)
                .Returns(HttpStatusCode.OK);

            GetMock<IApiCommunicator>()
                .Setup(m => m.SendHttpRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(mockResponse.Object);

            var result = ClassUnderTest.GetUserAccounts(string.Empty);

            result.Any().Should().BeFalse();
        }

        [Test]
        public void GetUserAccountThrowsErrorOnInvalidResponse()
        {
            var errorMessage = default(string);

            var mockResponse = GetMock<IHttpWebResponse>();
            mockResponse
                .SetupGet(m => m.StatusCode)
                .Returns(HttpStatusCode.InternalServerError);
            mockResponse
                .SetupGet(m => m.StatusDescription)
                .Returns("Test error message");

            var apiCommunicatorMock = GetMock<IApiCommunicator>();
            apiCommunicatorMock
                .Setup(m => m.SendHttpRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(mockResponse.Object);
            apiCommunicatorMock
                .Setup(m => m.ThrowErrorOnInvalidResponse(It.IsAny<IHttpWebResponse>()))
                .Throws(new Exception("Test error message"));

            try
            {
                ClassUnderTest.GetUserAccounts(string.Empty);
            }
            catch(Exception ex)
            {
                errorMessage = ex.Message;
            }

            errorMessage.Should().Be("Test error message");
        }

        [Test]
        public void CreateUserCallsCorrectEndpoint()
        {
            var endpointCalled = false;

            var mockResponse = GetMock<IHttpWebResponse>();
            mockResponse
                .SetupGet(m => m.StatusCode)
                .Returns(HttpStatusCode.OK);

            GetMock<IApiCommunicator>()
                .Setup(m => m.SendHttpRequestWithBody("Account", "POST", It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() => endpointCalled = true)
                .Returns(mockResponse.Object);

            ClassUnderTest.CreateUser(string.Empty, string.Empty, string.Empty);

            endpointCalled.Should().BeTrue();
        }

        [Test]
        public void CreateUserThrowsErrorOnInvalidResponse()
        {
            var errorMessage = default(string);

            var mockResponse = GetMock<IHttpWebResponse>();
            mockResponse
                .SetupGet(m => m.StatusCode)
                .Returns(HttpStatusCode.InternalServerError);
            mockResponse
                .SetupGet(m => m.StatusDescription)
                .Returns("Test error message");

            var apiCommunicatorMock = GetMock<IApiCommunicator>();
            apiCommunicatorMock
                .Setup(m => m.SendHttpRequestWithBody(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(mockResponse.Object);
            apiCommunicatorMock
                .Setup(m => m.ThrowErrorOnInvalidResponse(It.IsAny<IHttpWebResponse>()))
                .Throws(new Exception("Test error message"));

            try
            {
                ClassUnderTest.CreateUser(string.Empty, string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            errorMessage.Should().Be("Test error message");
        }
    }
}