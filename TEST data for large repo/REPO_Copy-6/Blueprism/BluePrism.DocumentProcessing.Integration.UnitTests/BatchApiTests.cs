namespace BluePrism.DocumentProcessing.Integration.UnitTests
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;
    using Utilities.Functional;
    using FluentAssertions;
    using Moq;
    using NUnit.Framework;
    using BluePrism.Utilities.Testing;

    [TestFixture]
    public class BatchApiTests : UnitTestBase<BatchApi>
    {
        [Test]
        public void CreateBatchReturnsBatchIdOnSuccess()
        {
            var expectedResponse = Guid.NewGuid();

            var mockResponse = GetMock<IHttpWebResponse>();
            mockResponse
                .Setup(m => m.GetResponseStream())
                .Returns(expectedResponse.ToString().Map(Encoding.UTF8.GetBytes).Map(x => new MemoryStream(x)));
            mockResponse
                .SetupGet(m => m.StatusCode)
                .Returns(HttpStatusCode.OK);

            GetMock<IApiCommunicator>()
                .Setup(m => m.SendHttpRequestWithBody(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(mockResponse.Object);

            var result = ClassUnderTest.CreateBatch(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty).Map(Guid.Parse);

            result.Should().Be(expectedResponse);
        }

        [Test]
        public void CreateBatchCallsCorrectEndpoint()
        {
            var endpointCalled = false;

            var mockResponse = GetMock<IHttpWebResponse>();
            mockResponse
                .SetupGet(m => m.StatusCode)
                .Returns(HttpStatusCode.OK);

            GetMock<IApiCommunicator>()
                .Setup(m => m.SendHttpRequestWithBody("Batch", "POST", It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() => endpointCalled = true)
                .Returns(mockResponse.Object);

            ClassUnderTest.CreateBatch(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

            endpointCalled.Should().BeTrue();
        }

        [Test]
        public void CreateBatchThrowsErrorOnInvalidResponse()
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
                ClassUnderTest.CreateBatch(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            errorMessage.Should().Be("Test error message");
        }

        [Test]
        public void AddDocumentToBatchCallsCorrectEndpoint()
        {
            var endpointCalled = false;

            var mockResponse = GetMock<IHttpWebResponse>();
            mockResponse
                .SetupGet(m => m.StatusCode)
                .Returns(HttpStatusCode.OK);

            GetMock<IApiCommunicator>()
                .Setup(m => m.SendHttpRequestWithBody("Batch/Test", "POST", It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() => endpointCalled = true)
                .Returns(mockResponse.Object);

            ClassUnderTest.AddDocumentToBatch(string.Empty, "Test", string.Empty, string.Empty);

            endpointCalled.Should().BeTrue();
        }

        [Test]
        public void SubmitBatchCallsCorrectEndpoint()
        {
            var endpointCalled = false;

            var mockResponse = GetMock<IHttpWebResponse>();
            mockResponse
                .SetupGet(m => m.StatusCode)
                .Returns(HttpStatusCode.OK);

            GetMock<IApiCommunicator>()
                .Setup(m => m.SendHttpRequest("Batch/Test/Submit", "POST", It.IsAny<string>()))
                .Callback(() => endpointCalled = true)
                .Returns(mockResponse.Object);

            ClassUnderTest.SubmitBatch(string.Empty, "Test");

            endpointCalled.Should().BeTrue();
        }
    }
}