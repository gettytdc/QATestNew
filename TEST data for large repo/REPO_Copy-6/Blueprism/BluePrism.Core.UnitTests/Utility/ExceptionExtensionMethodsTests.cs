#if UNITTESTS
using NUnit.Framework;
using System;
using BluePrism.Core.Utility;
using System.Net;
using Moq;

namespace BluePrism.Core.UnitTests.Utility
{
    /// <summary>
    /// Contains tests for the <see cref="ExceptionExtensionMethods"/> class
    /// </summary>
    public class ExceptionExtensionMethodsTests
    {
        [Test]
        public void Is401WebException_NotWebException_ReturnFalse()
        {
            var exception = new NotImplementedException();
            Assert.That(exception.Is401WebException, Is.False);
        }

        [Test]
        public void Is401WebException_WebExceptionNullResponse_ReturnFalse()
        {
            var exception = new WebException("some message", new Exception(),
                WebExceptionStatus.UnknownError, null);
            Assert.That(exception.Is401WebException, Is.False);
        }

        [Test]
        public void Is401WebException_WebExceptionNotHttpWebResponse_ReturnFalse()
        {
            var responseMock = new Mock<WebResponse>();
            var exception = new WebException("some message", new Exception(),
                            WebExceptionStatus.UnknownError, responseMock.Object);
            Assert.That(exception.Is401WebException, Is.False);
        }

        [Test]
        public void Is401WebException_WebExceptionResponseWith403StatusCode_ReturnFalse()
        {
            var responseMock = new Mock<HttpWebResponse>();
            responseMock
                .SetupGet(x => x.StatusCode)
                .Returns(HttpStatusCode.Forbidden);

            var exception = new WebException("some message", new Exception(),
                            WebExceptionStatus.UnknownError, responseMock.Object);

            Assert.That(exception.Is401WebException, Is.False);
        }

        [Test]
        public void Is401WebException_WebExceptionResponseWith401StatusCode_ReturnTrue()
        {
            var responseMock = new Mock<HttpWebResponse>();
            responseMock
                .SetupGet(x => x.StatusCode)
                .Returns(HttpStatusCode.Unauthorized);

            var exception = new WebException("some message", new Exception(),
                            WebExceptionStatus.UnknownError, responseMock.Object);

            Assert.That(exception.Is401WebException, Is.True);
        }

    }
}
#endif
