#if UNITTESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using AutomateProcessCore.UnitTests.RequestHandling;
using BluePrism.AutomateProcessCore;
using BluePrism.AutomateProcessCore.WebApis;
using BluePrism.AutomateProcessCore.WebApis.CustomCode;
using BluePrism.AutomateProcessCore.WebApis.RequestHandling;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace AutomateProcessCore.UnitTests.WebApis.BusinessObjects
{
    [TestFixture]
    [Category("Web APIs")]
    public class OutputParameterMapperTests
    {
        public OutputParameterMapperTests() => _parameters = new Dictionary<string, clsProcessValue> { { "Parameter 1", "Parameter Value 1" }, { "Parameter 2", 2 }, { "Action Parameter 1", "Action Parameter Value 1" }, { "Action Parameter 2", 4 } };

        private readonly Guid _webApiId = Guid.NewGuid();
        private readonly clsSession _session = new clsSession(Guid.NewGuid(), 105, new WebConnectionSettings(5, 5, 5, new List<UriWebConnectionSettings>()));
        private readonly string _action1Name = "Action 1";
        private readonly Dictionary<string, clsProcessValue> _parameters;
        private Mock<ICustomCodeBuilder> _codeRunnerMock;
        private OutputParameterMapper _outputMapper;

        [SetUp]
        public void SetUp()
        {
            _codeRunnerMock = new Mock<ICustomCodeBuilder>();
            _outputMapper = new OutputParameterMapper(_codeRunnerMock.Object);
        }

        [Test]
        public void CreateParameters_StandardOnly_WithStringContent_ShouldIncludeResponseContentParameterWithValue()
        {
            const string testContent = "Test content";
            var response = CreateResponseData(HttpStatusCode.OK, testContent);
            var context = new ActionContext(_webApiId, CreateSimpleConfiguration(), _action1Name, _parameters, _session);
            var parameters = _outputMapper.CreateParameters(response, false, context);
            var parameter = parameters.SingleOrDefault(a => (a.Name ?? string.Empty) == OutputParameters.ResponseContent);
            parameter.Should().NotBeNull();
            parameter.Value.FormattedValue.Should().Be(testContent);
        }

        [TestCase(HttpStatusCode.OK)]
        [TestCase(HttpStatusCode.Created)]
        public void CreateParameters_StandardOnly_WithAnyResponse_ShouldMapStatusCodeToParameter(HttpStatusCode statusCode)
        {
            var response = CreateResponseData(statusCode, "na");
            var context = new ActionContext(_webApiId, CreateSimpleConfiguration(), _action1Name, _parameters, _session);
            var parameters = _outputMapper.CreateParameters(response, false, context);
            var parameter = parameters.SingleOrDefault(a => (a.Name ?? string.Empty) == OutputParameters.StatusCode);
            parameter.Should().NotBeNull();
            parameter.Value.FormattedValue.Should().Be(statusCode.GetHashCode().ToString());
        }

        [Test]
        public void CreateParameters_StandardOnly_WithAnyResponse_ShouldCreateCollectionParameter()
        {
            var response = CreateResponseData(HttpStatusCode.OK, "na");
            var context = new ActionContext(_webApiId, CreateSimpleConfiguration(), _action1Name, _parameters, _session);
            var parameters = _outputMapper.CreateParameters(response, false, context);
            var parameter = parameters.SingleOrDefault(a => (a.Name ?? string.Empty) == OutputParameters.ResponseHeaders);
            parameter.Should().NotBeNull();
            var collection = parameter.Value?.Collection;
            collection.Should().NotBeNull();
            collection.Definition.Count.Should().Be(2);
            collection.Definition[0].Name.Should().Be("Column1");
            collection.Definition[1].Name.Should().Be("Column2");
        }

        [Test]
        public void CreateParameters_StandardOnly_WithResponseHeaders_ShouldAddEachHeaderToCollectionParameter()
        {
            var headers = new WebHeaderCollection
            {
                { "x-header1", "value1" },
                { "x-header2", "value2" }
            };
            var responseData = CreateResponseData(HttpStatusCode.OK, "na", headers);
            var context = new ActionContext(_webApiId, CreateSimpleConfiguration(), _action1Name, _parameters, _session);
            var parameters = _outputMapper.CreateParameters(responseData, false, context);
            var parameter = parameters.SingleOrDefault(a => (a.Name ?? string.Empty) == OutputParameters.ResponseHeaders);
            parameter.Should().NotBeNull();
            var collection = parameter.Value?.Collection;
            collection.Should().NotBeNull();
            var values = collection.Rows.ToDictionary(r => r["Column1"].FormattedValue, r => r["Column2"].FormattedValue);
            var expected = new Dictionary<string, string> { { "x-header1", "value1" }, { "x-header2", "value2" }, { "Content-Type", "text/plain; charset=utf-8" } };
            values.Should().Equal(expected);
        }

        [Test]
        public void CreateParameters_WithJsonOutputParameters_CheckAllParamsAreGenerated()
        {
            var responseData = CreateResponseData(HttpStatusCode.OK, "{ 'Name':'Bob', 'Age':35 }");
            var outputParameterConfiguration = new OutputParameterConfiguration(CreateJsonOutputs(), string.Empty);
            var context = new ActionContext(_webApiId, CreateSimpleConfiguration(outputParameterConfiguration), _action1Name, _parameters, _session);
            var parameters = _outputMapper.CreateParameters(responseData, false, context);
            var customNameParameter = parameters.SingleOrDefault(a => a.Name == "Name");
            customNameParameter.Should().NotBeNull();
            customNameParameter.Value.FormattedValue.Should().Be("Bob");
            var customAgeParameter = parameters.SingleOrDefault(a => a.Name == "Age");
            customAgeParameter.Should().NotBeNull();
            customAgeParameter.Value.FormattedValue.Should().Be("35");
        }

        [Test]
        public void CreateParameters_WithJsonOutputParameters_ResponseNotJson_ShouldThrowException()
        {
            var responseData = CreateResponseData(HttpStatusCode.OK, "this is not json");
            var outputParameterConfiguration = new OutputParameterConfiguration(CreateJsonOutputs(), string.Empty);
            var context = new ActionContext(_webApiId, CreateSimpleConfiguration(outputParameterConfiguration), _action1Name, _parameters, _session);
            _outputMapper.Invoking(x => x.CreateParameters(responseData, false, context)).ShouldThrow<InvalidOperationException>().WithMessage(WebApiResources.ErrorParsingJson);
        }

        [Test]
        public void CreateParameters_WithRequestContentOutput_CheckParamCorrectlyGenerated()
        {
            var headers = new WebHeaderCollection
            {
                { "header1", "firstHeader" },
                { "header2", "secondHeader" }
            };
            var request = MockRequestHelper.Create(headers);
            request.Setup(r => r.RequestUri).Returns(new Uri("http://test.test"));
            var requestData = new HttpRequestData(request.Object, "requestContent");
            var response = CreateResponseData(HttpStatusCode.OK, "{ 'Name':'Bob', 'Age':35 }", requestData: requestData);
            var context = new ActionContext(_webApiId, CreateSimpleConfiguration(), _action1Name, _parameters, _session);
            var parameters = _outputMapper.CreateParameters(response, true, context);
            var requestParameter = parameters.SingleOrDefault(a => (a.Name ?? string.Empty) == OutputParameters.RequestData);
            requestParameter.Should().NotBeNull();
            var expected = new StringBuilder();
            expected.AppendLine("POST http://test.test/");
            expected.AppendLine("header1: firstHeader");
            expected.AppendLine("header2: secondHeader");
            expected.Append("requestContent");
            requestParameter.Value.FormattedValue.Should().Be(expected.ToString());
        }

        [Test]
        public void CreateParameters_RequestNotSent_HandledCorrectly()
        {
            var headers = new WebHeaderCollection
            {
                { "header1", "firstHeader" }
            };
            var request = MockRequestHelper.Create(headers);
            request.Setup(r => r.RequestUri).Returns(new Uri("http://test.test"));
            var requestData = new HttpRequestData(request.Object, "{ 'Name':'Bob', 'Age':35 }");
            var response = new HttpResponseData(requestData, null);
            var outputParameterConfiguration = new OutputParameterConfiguration(CreateJsonOutputs(), string.Empty);
            var context = new ActionContext(_webApiId, CreateSimpleConfiguration(outputParameterConfiguration), _action1Name, _parameters, _session);
            var parameters = _outputMapper.CreateParameters(response, true, context);
            var requestParameter = parameters.SingleOrDefault(a => (a.Name ?? string.Empty) == OutputParameters.RequestData);
            requestParameter.Should().NotBeNull();
            var expectedRequestData = new StringBuilder();
            expectedRequestData.AppendLine("POST http://test.test/");
            expectedRequestData.AppendLine("header1: firstHeader");
            expectedRequestData.Append("{ 'Name':'Bob', 'Age':35 }");
            AssertParameterValueCorrect(parameters, OutputParameters.RequestData, expectedRequestData.ToString());
            AssertParameterValueCorrect(parameters, OutputParameters.ResponseContent, string.Empty);
            AssertParameterValueCorrect(parameters, OutputParameters.ResponseHeaders, "Empty");
            AssertParameterValueCorrect(parameters, OutputParameters.StatusCode, string.Empty);
            AssertParameterValueCorrect(parameters, "Name", string.Empty);
            AssertParameterValueCorrect(parameters, "Age", string.Empty);
        }

        private void AssertParameterValueCorrect(clsArgumentList parameters, string parameterName, string expectedContent)
        {
            var parameter = parameters.SingleOrDefault(a => (a.Name ?? string.Empty) == (parameterName ?? string.Empty));
            parameter.Should().NotBeNull();
            parameter.Value.FormattedValue.Should().Be(expectedContent);
        }

        private WebApiConfiguration CreateSimpleConfiguration(OutputParameterConfiguration outputParameterConfiguration = null)
        {
            var action1Parameters = new[] { new ActionParameter("Action Parameter 1", string.Empty, DataType.text, true, ""), new ActionParameter("Action Parameter 2", string.Empty, DataType.number, true, 0) };
            return new WebApiConfigurationBuilder().WithParameter("Parameter 1", DataType.text, true)
                .WithParameter("Parameter 2", DataType.number, true)
                .WithAction(_action1Name, HttpMethod.Post, "/action1", outputParameterConfiguration: outputParameterConfiguration ?? new OutputParameterConfiguration(new ResponseOutputParameter[] { }, string.Empty), parameters: action1Parameters).Build();
        }

        private static HttpResponseData CreateResponseData(HttpStatusCode statusCode, string content, WebHeaderCollection headers = null,HttpRequestData requestData = null)
        {
            var expected = content;
            var expectedBytes = Encoding.UTF8.GetBytes(expected);
            var responseStream = new MemoryStream();
            responseStream.Write(expectedBytes, 0, expectedBytes.Length);
            responseStream.Seek(0L, SeekOrigin.Begin);
            headers = headers ?? new WebHeaderCollection();
            // Used by CharacterSet property
            headers.Add(HttpRequestHeader.ContentType, "text/plain; charset=utf-8");
            if (requestData is null)
            {
                var requestMock = MockRequestHelper.Create();
                requestData = new HttpRequestData(requestMock.Object, null);
            }

            var responseMock = new Mock<HttpWebResponse>();
            responseMock.Setup(r => r.GetResponseStream()).Returns(responseStream);
            responseMock.Setup(r => r.Headers).Returns(headers);
            responseMock.Setup(r => r.StatusCode).Returns(statusCode);
            var headersFieldInfo = typeof(HttpWebResponse).GetField("m_HttpResponseHeaders", BindingFlags.Instance | BindingFlags.NonPublic);
            headersFieldInfo.SetValue(responseMock.Object, headers);
            return new HttpResponseData(requestData, responseMock.Object);
        }

        private static List<JsonPathOutputParameter> CreateJsonOutputs() => new List<JsonPathOutputParameter> { new JsonPathOutputParameter("Name", "Description", "$.Name", DataType.text), new JsonPathOutputParameter("Age", "Description", "$.Age", DataType.number) };
    }
}
#endif
