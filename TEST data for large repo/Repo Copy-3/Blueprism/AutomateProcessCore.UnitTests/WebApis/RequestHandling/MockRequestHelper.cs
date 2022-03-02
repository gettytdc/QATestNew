#if UNITTESTS
using System;
using System.IO;
using System.Net;
using System.Reflection;
using Moq;

namespace AutomateProcessCore.UnitTests.RequestHandling
{
    public class MockRequestHelper
    {
        private static readonly FieldInfo HeadersField;

        static MockRequestHelper()
        {
            HeadersField = typeof(HttpWebRequest).GetField("_HttpRequestHeaders", BindingFlags.Instance | BindingFlags.NonPublic);
            if (HeadersField is null)
            {
                throw new MissingFieldException("Unable to access HttpWebRequest field required for testing");
            }
        }
        public static Mock<HttpWebRequest> Create(WebHeaderCollection headers = null)
        {
            var mock = new Mock<HttpWebRequest>
            {
                CallBase = true
            };

            // Headers setup - HttpWebRequest's own methods access headers directly 
            // via a field hence the reflection
            headers = headers ?? new WebHeaderCollection();
            mock.Setup(r => r.Headers).Returns(headers);
            HeadersField.SetValue(mock.Object, headers);
            var requestStream = new MemoryStream();
            mock.Setup(r => r.GetRequestStream()).Returns(requestStream);
            mock.Setup(r => r.Method).Returns("POST");
            return mock;
        }
    }
}
#endif
