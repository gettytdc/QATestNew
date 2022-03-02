using BluePrism.AutomateAppCore;
using BluePrism.AutomateAppCore.Auth;
using BluePrism.AutomateAppCore.Groups;
using BluePrism.UnitTesting;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace AutomateAppCore.UnitTests.Server
{
    [TestFixture]
    public class ServerCheckPermissionsTests
    {
        [SetUp]
        public void Setup()
        {
            LegacyUnitTestHelper.SetupDependencyResolver();
        }

        [TestCaseSource(nameof(PermissionTestData))]
        public void CorrectPermissionAttributesAttainedFromMethod(Action<clsServer> method, bool expectedAllowLocal, string[] expectedPermissions)
        {
            var helper = new Mock<IPermissionValidator>();
            var allowsLocalUnsecuredCalls = false;
            var permissions = new string[] { };
            helper.Setup(h => h.EnsurePermissions(It.IsAny<ServerPermissionsContext>())).Callback((ServerPermissionsContext context) =>
            {
                allowsLocalUnsecuredCalls = context.AllowAnyLocalCalls;
                permissions = context.Permissions;

                // Throw here to exit immediately
                throw new ShortCircuitingException();
            });
            var sut = new TestServer(helper.Object);
            try
            {
                method(sut);
            }
            catch (ShortCircuitingException)
            {
                // exception intentionally thrown to avoid full execution of method
            }

            Assert.That(allowsLocalUnsecuredCalls, Is.EqualTo(expectedAllowLocal));
            Assert.That(permissions, Is.EqualTo(expectedPermissions));
        }

        public static IEnumerable<TestCaseData> PermissionTestData
        {
            get
            {
                return new List<TestCaseData>
                {
                    new TestCaseData(new Action<clsServer>(s => s.RebuildDependencies()), true, new string[] { }),
                    new TestCaseData(new Action<clsServer>(s => s.GetTree(GroupTreeType.None, new TreeAttributes())), true, new string[] { }),
                    new TestCaseData(new Action<clsServer>(s => s.GetWebServiceDefinitions()), true, new string[] { }),
                    new TestCaseData(new Action<clsServer>(s => s.IsServer()), false, null),
                    new TestCaseData(new Action<clsServer>(s => s.GetValidationCategories()), false, new[] { Permission.SystemManager.Audit.ConfigureDesignControls, Permission.SystemManager.Audit.ViewDesignControls })
                };
            }
        }

        private class TestServer : clsServer
        {
            public TestServer(IPermissionValidator helper)
            {
                mPermissionValidator = helper;
            }
        }

        /// <summary>
        /// This exception is used to exit from a method early to avoid further execution
        /// </summary>
        private class ShortCircuitingException : Exception
        {
        }
    }
}
