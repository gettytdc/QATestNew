namespace BluePrism.DocumentProcessing.Integration.UnitTests
{
    using System.IO;
    using System.Net;
    using System.Text;
    using Utilities.Functional;
    using FluentAssertions;
    using Moq;
    using NUnit.Framework;
    using BluePrism.Utilities.Testing;

    [TestFixture]
    public class DocumentTypeApiTests : UnitTestBase<DocumentTypeApi>
    {
        [Test]
        public void SubmitBatchCallsCorrectEndpoint()
        {
            var endpointCalled = false;

            var mockedResponse = "[]";

            var mockResponse = GetMock<IHttpWebResponse>();
            mockResponse
                .SetupGet(m => m.StatusCode)
                .Returns(HttpStatusCode.OK);
            mockResponse
                .Setup(m => m.GetResponseStream())
                .Returns(mockedResponse.Map(Encoding.UTF8.GetBytes).Map(x => new MemoryStream(x)));

            GetMock<IApiCommunicator>()
                .Setup(m => m.SendHttpRequest("DocumentType", "GET", It.IsAny<string>()))
                .Callback(() => endpointCalled = true)
                .Returns(mockResponse.Object);

            ClassUnderTest.GetDocumentTypes(string.Empty);

            endpointCalled.Should().BeTrue();
        }
    }
}