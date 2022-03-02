namespace BluePrism.DocumentProcessing.Integration.UnitTests
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;
    using Utilities.Functional;
    using Domain;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using BluePrism.Utilities.Testing;

    [TestFixture]
    public class DocumentApiTests : UnitTestBase<DocumentApi>
    {
        [Test]
        public void GetDocumentDataReturnsFormDataOnSuccess()
        {
            var expectedResponse = JsonConvert.SerializeObject(new DocumentFormDocument());

            var mockResponse = GetMock<IHttpWebResponse>();
            mockResponse
                .Setup(m => m.GetResponseStream())
                .Returns(expectedResponse.Map(Encoding.UTF8.GetBytes).Map(x => new MemoryStream(x)));
            mockResponse
                .SetupGet(m => m.StatusCode)
                .Returns(HttpStatusCode.OK);

            GetMock<IApiCommunicator>()
                .Setup(m => m.SendHttpRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(mockResponse.Object);

            var result =
                    ClassUnderTest.GetDocumentData(string.Empty, string.Empty);

            result.Should().Be(expectedResponse);
        }

        [Test]
        public void GetDocumentDataThrowsErrorOnInvalidResponse()
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
                ClassUnderTest.GetDocumentData(string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            errorMessage.Should().Be("Test error message");
        }
    }
}