using BluePrism.ApplicationManager;
using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandling;
using BluePrism.UIAutomation;
using BluePrism.Utilities.Testing;
using Moq;
using NUnit.Framework;

namespace ClientComms.UnitTests.CommandHandlers.UIAutomation.Shared
{

    /// <summary>
    /// Base class for testing UIAutomation command handlers. Contains shared
    /// functionality to initialise handler class. Note that this class inherits
    /// from UnitTestBase, which initialises the handler using AutoFac.Extra.Moq
    /// auto-mocking container (AMC). Any dependencies that are injected via the
    /// constructor will be automatically created by the AMC and can be accessed
    /// via the GetMock function.
    /// </summary>
    /// <typeparam name="THandler">The type of handler being tested</typeparam>
    internal abstract class UIAutomationHandlerTestBase<THandler> : UnitTestBase<THandler> where THandler : ICommandHandler
    {
        private Mock<IAutomationElement> mElementMock;
        private Mock<ILocalTargetApp> mApplicationMock;
        private Mock<IUIAutomationIdentifierHelper> mIdentifierHelperMock;

        /// <summary>
        /// The test PID value used for handler during test
        /// </summary>
        protected const int TestPID = 11;

        /// <summary>
        /// Sets up the handler before each individual test
        /// </summary>
        [SetUp]
        public override void Setup()
        {
            base.Setup();

            // Mock shared dependencies. 
            // By default, the application is set up to return
            // a mock IAutomationElement object that can be further configured in test code
            mElementMock = new Mock<IAutomationElement>();
            mApplicationMock = GetMock<ILocalTargetApp>();
            mApplicationMock.Setup(a => a.PID).Returns(TestPID);
            mIdentifierHelperMock = GetMock<IUIAutomationIdentifierHelper>();
            mIdentifierHelperMock.Setup(h => h.FindUIAutomationElement(It.IsAny<clsQuery>(), TestPID)).Returns(mElementMock.Object);
        }

        /// <summary>
        /// The handler instance under test - same as ClassUnderTest
        /// </summary>
        protected THandler Handler
        {
            get
            {
                return ClassUnderTest;
            }
        }

        /// <summary>
        /// A mock IAutomationElement object for the handler's target element. This
        /// is the target identified in the query that the stage in the application
        /// object is acting on.
        /// </summary>
        protected Mock<IAutomationElement> ElementMock
        {
            get
            {
                return mElementMock;
            }
        }

        /// <summary>
        /// The mock used for the handler's ILocalTargetApp dependency
        /// </summary>
        protected Mock<ILocalTargetApp> ApplicationMock
        {
            get
            {
                return mApplicationMock;
            }
        }

        /// <summary>
        /// The mock used for the handler's IUIAutomationIdentifierHelper dependency
        /// </summary>
        protected Mock<IUIAutomationIdentifierHelper> IdentifierHelperMock
        {
            get
            {
                return mIdentifierHelperMock;
            }
        }

        /// <summary>
        /// Sets up a mock
        /// </summary>
        protected void SetupElement()
        {
            // Throw New NotImplementedException
        }

        /// <summary>
        /// Calls the Execute function of the handler using the specified query and
        /// returns the result
        /// </summary>
        /// <param name="query">The query to execute</param>
        /// <returns>The return value from the Execute function</returns>
        protected Reply Execute(clsQuery query = null)
        {
            var context = new CommandContext(query);
            return Handler.Execute(context);
        }
    }
}