#if UNITTESTS

namespace AutomateProcessCore.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using BluePrism.UnitTesting.TestSupport;
    using BluePrism.AutomateAppCore;
    using BluePrism.AutomateProcessCore;
    using BluePrism.AutomateProcessCore.Processes;
    using BluePrism.AutomateProcessCore.Stages;
    using BluePrism.AutomateProcessCore.WebApis;
    using BluePrism.AutomateProcessCore.WebApis.Authentication;
    using FluentAssertions;
    using Moq;
    using NUnit.Framework;

    public class ActionStageTests
    {
        private clsActionStage _classUnderTest;
        private clsProcess _process;
        private Mock<IGroupObjectDetails> _groupObjectDetailsMock;

        private static readonly IEnumerable<clsProcessCredentialsDependency> emptyCredentialDependencyList = 
            new List<clsProcessCredentialsDependency>() { };

        private static readonly clsProcessCredentialsDependency dynamicReferenceProcessCredentialDependency =
            new clsProcessCredentialsDependency(string.Empty);
                
    [SetUp]
        public void SetUp()
        {
            _classUnderTest = new clsActionStage(null);

            _groupObjectDetailsMock = new Mock<IGroupObjectDetails>();
            _groupObjectDetailsMock.SetupGet(m => m.Children).Returns(new List<IObjectDetails>());

            _process = new clsProcess(_groupObjectDetailsMock.Object, DiagramType.Process, false);
            _process.Name = "Test Process";
            _process.AddStage(_classUnderTest);

            SetupMockServer();
        }
        
        private static IEnumerable<TestCaseData> TestCases_GetDependenciesForInternalBusinessObject()
        {
            Func<clsProcess, clsSession, clsInternalBusinessObject> workQueueBusinessObjectfactory = (p, s) => new clsWorkQueuesBusinessObject(p, s);
            Func<string, clsProcessQueueDependency> workQueueDependencyFactory = x => new clsProcessQueueDependency(x);
            yield return new TestCaseData(workQueueBusinessObjectfactory, "Blueprism.Automate.clsWorkQueuesActions",
                "Add To Queue", "Queue Name", workQueueDependencyFactory);

            Func<clsProcess, clsSession, clsInternalBusinessObject> credentialsBusinessObjectfactory = (p, s) => new clsCredentialsBusinessObject(p, s);
            Func<string, clsProcessCredentialsDependency> credentialDependencyFactory = x => new clsProcessCredentialsDependency(x);
            yield return new TestCaseData(credentialsBusinessObjectfactory, "Blueprism.Automate.clsCredentialsActions",
                "Get", "Credentials Name", credentialDependencyFactory);

            Func<clsProcess, clsSession, clsInternalBusinessObject> calendarBusinessObjectfactory = (p, s) => new clsCalendarsBusinessObject(p, s);
            Func<string, clsProcessCalendarDependency> calendarDependencyFactory = x => new clsProcessCalendarDependency(x);
            yield return new TestCaseData(calendarBusinessObjectfactory, "clsCalendarsBusinessObject",
                "Add Working Days", "Calendar Name", calendarDependencyFactory);

        }

        private static void SetupMockServer()
        {
            var serverManagerMock = new Mock<ServerManager>();
            serverManagerMock.SetupGet(m => m.Server).Returns(Mock.Of<IServer>());
            var serverFactoryMock = new Mock<BluePrism.AutomateAppCore.ClientServerConnection.IServerFactory>();
            serverFactoryMock.SetupGet(m => m.ServerManager).Returns(serverManagerMock.Object);
            ReflectionHelper.SetPrivateField(typeof(app), "ServerFactory", null, serverFactoryMock.Object);
        }

        [TestCaseSource(nameof(TestCases_GetDependenciesForInternalBusinessObject))]
        public void GetDependencies_InternalBusinessObject_ParameterWithTextValue_ShouldReturnDependency<TProcessDependency>(Func<clsProcess, clsSession, clsInternalBusinessObject> businessObjectFactory,
                                              string objectName, string actionName, string parameterName, Func<string, TProcessDependency> dependencyFactory) where TProcessDependency : clsProcessDependency
        {
            SetUpActionStageWithBusinessObjectAction(businessObjectFactory, objectName, actionName);
            _classUnderTest.AddParameter(ParamDirection.In, DataType.text, parameterName, string.Empty, MapType.Expr, "\"Value 1\"");

            _classUnderTest
                .GetDependencies(true)
                .OfType<TProcessDependency>()
                .ShouldBeEquivalentTo(new List<TProcessDependency>() { dependencyFactory("Value 1") });
        }

        [TestCaseSource(nameof(TestCases_GetDependenciesForInternalBusinessObject))]
        public void GetDependencies_InternalBusinessObject_ParameterWithNumberValue_ShouldReturnNothing<TProcessDependency>(Func<clsProcess, clsSession, clsInternalBusinessObject> businessObjectFactory,
                                         string objectName, string actionName, string parameterName, Func<string, TProcessDependency> dependencyFactory) where TProcessDependency : clsProcessDependency
        {
            SetUpActionStageWithBusinessObjectAction(businessObjectFactory, objectName, actionName);
            _classUnderTest.AddParameter(ParamDirection.In, DataType.number, parameterName, string.Empty, MapType.Expr, "6");

            _classUnderTest
                .GetDependencies(true)
                .OfType<TProcessDependency>()
                .ShouldBeEquivalentTo(new List<TProcessDependency>() { });
        }

        [TestCaseSource(nameof(TestCases_GetDependenciesForInternalBusinessObject))]
        public void GetDependencies_InternalBusinessObject_OutputParameter_ShouldReturnDependency<TProcessDependency>(Func<clsProcess, clsSession, clsInternalBusinessObject> businessObjectFactory,
                                      string objectName, string actionName, string parameterName, Func<string, TProcessDependency> dependencyFactory) where TProcessDependency : clsProcessDependency
        {
            SetUpActionStageWithBusinessObjectAction(businessObjectFactory, objectName, actionName);
            _classUnderTest.AddParameter(ParamDirection.Out, DataType.text, parameterName, string.Empty, MapType.Expr, "\"Value 1\"");

            _classUnderTest
                .GetDependencies(true)
                .OfType<TProcessDependency>()
                .ShouldBeEquivalentTo(new List<TProcessDependency>() { });
        }

        [TestCaseSource(nameof(TestCases_GetDependenciesForInternalBusinessObject))]
        public void GetDependencies_InternalBusinessObject_ParameterWithNullValue_ShouldReturnNothing<TProcessDependency>(Func<clsProcess, clsSession, clsInternalBusinessObject> businessObjectFactory,
                                              string objectName, string actionName, string parameterName, Func<string, TProcessDependency> dependencyFactory) where TProcessDependency : clsProcessDependency
        {
            SetUpActionStageWithBusinessObjectAction(businessObjectFactory, objectName, actionName);
            _classUnderTest.AddParameter(ParamDirection.In, DataType.text, parameterName, string.Empty, MapType.Expr, null);

            _classUnderTest
                .GetDependencies(true)
                .OfType<TProcessDependency>()
                .ShouldBeEquivalentTo(new List<TProcessDependency>() { });
        }

        [TestCaseSource(nameof(TestCases_GetDependenciesForInternalBusinessObject))]
        public void GetDependencies_InternalBusinessObject_ParameterWithEmptyStringValue_ShouldReturnNothing<TProcessDependency>(Func<clsProcess, clsSession, clsInternalBusinessObject> businessObjectFactory,
                                              string objectName, string actionName, string parameterName, Func<string, TProcessDependency> dependencyFactory) where TProcessDependency : clsProcessDependency
        {
            SetUpActionStageWithBusinessObjectAction(businessObjectFactory, objectName, actionName);
            _classUnderTest.AddParameter(ParamDirection.In, DataType.text, parameterName, string.Empty, MapType.Expr, string.Empty);

            _classUnderTest
                .GetDependencies(true)
                .OfType<TProcessDependency>()
                .ShouldBeEquivalentTo(new List<TProcessDependency>() { });
        }

        [TestCaseSource(nameof(TestCases_GetDependenciesForInternalBusinessObject))]
        public void GetDependencies_InternalBusinessObject_ParameterWithDataStageValue_ShouldReturnDependencyToDynamicReference<TProcessDependency>(Func<clsProcess, clsSession, clsInternalBusinessObject> businessObjectFactory,
                                              string objectName, string actionName, string parameterName, Func<string, TProcessDependency> dependencyFactory) where TProcessDependency : clsProcessDependency
        {
            SetUpActionStageWithBusinessObjectAction(businessObjectFactory, objectName, actionName);
            _classUnderTest.AddParameter(ParamDirection.In, DataType.text, parameterName, string.Empty, MapType.Expr, "[Data Stage 1]");

            _classUnderTest
                .GetDependencies(true)
                .OfType<TProcessDependency>()
                .ShouldBeEquivalentTo(new List<TProcessDependency>() { dependencyFactory(String.Empty) });
        }


        [TestCaseSource(nameof(TestCases_GetDependenciesForInternalBusinessObject))]
        public void GetDependencies_InternalBusinessObject_ParameterWithDataStageValueAndNotIncludeInternal_ShouldReturnNothing<TProcessDependency>(Func<clsProcess, clsSession, clsInternalBusinessObject> businessObjectFactory,
                                              string objectName, string actionName, string parameterName, Func<string, TProcessDependency> dependencyFactory) where TProcessDependency : clsProcessDependency
        {
            SetUpActionStageWithBusinessObjectAction(businessObjectFactory, objectName, actionName);
            _classUnderTest.AddParameter(ParamDirection.In, DataType.text, parameterName, string.Empty, MapType.Expr, "[Data Stage 1]");

            _classUnderTest
                .GetDependencies(false)
                .OfType<TProcessDependency>()
                .ShouldBeEquivalentTo(new List<TProcessDependency>() { });
        }
       

        [TestCaseSource(nameof(TestCases_GetDependenciesForInternalBusinessObject))]
        public void GetDependencies_InternalBusinessObject_ParameterWithInvalidValue_ShouldReturnNothing<TProcessDependency>(Func<clsProcess, clsSession, clsInternalBusinessObject> businessObjectFactory,
                                              string objectName, string actionName, string parameterName, Func<string, TProcessDependency> dependencyFactory) where TProcessDependency : clsProcessDependency
        {
            SetUpActionStageWithBusinessObjectAction(businessObjectFactory, objectName, actionName);
            _classUnderTest.AddParameter(ParamDirection.In, DataType.text, parameterName, string.Empty, MapType.Expr, "Text not enclosed in quotes");

            _classUnderTest
                .GetDependencies(true)
                .OfType<TProcessDependency>()
                .ShouldBeEquivalentTo(new List<TProcessDependency>() { });
        }


        private static IEnumerable<TestCaseData> TestCases_GetCredentialDependenciesForWebApiBusinessObject()
        {
            yield return new TestCaseData("Credential 1", true, "\"Credential 2\"",
                new List<clsProcessCredentialsDependency>() { new clsProcessCredentialsDependency("Credential 2") });

            yield return new TestCaseData("Credential 1", true, null,
                new List<clsProcessCredentialsDependency>() { new clsProcessCredentialsDependency("Credential 1") });

            yield return new TestCaseData("Credential 1", true, string.Empty,
                new List<clsProcessCredentialsDependency>() { new clsProcessCredentialsDependency("Credential 1") });

            yield return new TestCaseData("Credential 1", true, "[Data Stage]",
                new List<clsProcessCredentialsDependency>() { dynamicReferenceProcessCredentialDependency });

            yield return new TestCaseData("Credential 1", true, "Invalid value", emptyCredentialDependencyList);


            yield return new TestCaseData(string.Empty, true, "\"Credential 2\"",
              new List<clsProcessCredentialsDependency>() { new clsProcessCredentialsDependency("Credential 2") });

            yield return new TestCaseData(string.Empty, true, null, emptyCredentialDependencyList);

            yield return new TestCaseData(string.Empty, true, string.Empty, emptyCredentialDependencyList);

            yield return new TestCaseData(string.Empty, true, "[Data Stage]",
                new List<clsProcessCredentialsDependency>() { dynamicReferenceProcessCredentialDependency });

            yield return new TestCaseData(string.Empty, true, "Invalid value", emptyCredentialDependencyList);


            yield return new TestCaseData("Credential 1", false, null,
                new List<clsProcessCredentialsDependency>() { new clsProcessCredentialsDependency("Credential 1") });
            
        }
        
        [TestCaseSource(nameof(TestCases_GetCredentialDependenciesForWebApiBusinessObject))]
        public void GetCredentialDependenciesForWebApiBusinessObject_ShouldReturnExpectedResult(string defaultCredentialName, bool exposeParameterToProcess, string credentialParameterValue, IEnumerable<clsProcessCredentialsDependency> expectedResult)
        {
            var credentialParameterName = "Credential For Authentication";
            SetUpActionStageWithWebApiUsingCredentialAuthentication(defaultCredentialName, exposeParameterToProcess, credentialParameterName);
            if (exposeParameterToProcess)
                _classUnderTest.AddParameter(ParamDirection.In, DataType.text, credentialParameterName, string.Empty, MapType.Expr, credentialParameterValue);

            _classUnderTest
                .GetDependencies(true)
                .OfType<clsProcessCredentialsDependency>()
                .ShouldBeEquivalentTo(expectedResult);
        }


        private void SetUpActionStageWithBusinessObjectAction(Func<clsProcess, clsSession, clsBusinessObject> businessObjectFactory,
                                                        string objectName, string actionName)
        {
            var objectTree = new clsGroupBusinessObject(_groupObjectDetailsMock.Object)
            {
                Children =
                {
                    new clsGroupBusinessObject(_groupObjectDetailsMock.Object)
                    {
                        Children =
                        {
                            businessObjectFactory(_process, null)
                        }
                    }
                }
            };

            Func<IGroupObjectDetails, clsProcess, clsSession, bool, List<string>, clsGroupBusinessObject> objectTreeFactory =
                (x1, x2, x3, x4, x5) => objectTree;

            ReflectionHelper.SetPrivateField<clsProcess>("GroupBusinessObjectFactory", null, objectTreeFactory);

            _classUnderTest.SetResource(objectName, actionName);
        }


        private void SetUpActionStageWithWebApiUsingCredentialAuthentication(string credentialName, bool exposeCredentialParameterToProcess, string credentialParameterName)
        {
            var authenticationMock = new Mock<IAuthentication>();
            var authenticationCredentialMock = authenticationMock.As<ICredentialAuthentication>();

            var credential = new AuthenticationCredential(credentialName, exposeCredentialParameterToProcess, 
                                                            credentialParameterName);

            authenticationCredentialMock.Setup(x => x.Credential).Returns(credential);

            var webApiConfig = new WebApiConfigurationBuilder()
                                        .WithCommonAuthentication(authenticationMock.Object)
                                        .WithAction("Action1", HttpMethod.Get, "/action1")
                                        .Build();

            var webApi = new WebApi(Guid.NewGuid(), "My Test Api", true, webApiConfig);
            
            SetUpActionStageWithBusinessObjectAction((p, s) => new WebApiBusinessObject(ref webApi), "My Test Api", "Action1");
        }        
         



    }

}
#endif
