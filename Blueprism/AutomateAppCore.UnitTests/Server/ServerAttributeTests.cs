using BluePrism.AutomateAppCore;
using BluePrism.AutomateAppCore.Auth;
using BluePrism.Server.Domain.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;

namespace AutomateAppCore.UnitTests.Server
{
    /// <summary>
    /// Tests the WCF and security attributes are added to IServer, clsServer and partial
    /// classes.
    /// </summary>
    [TestFixture]
    public class ServerAttributeTests
    {
        [Test]
        public void TestWcfOperationContracts()
        {
            var total = 0;
            var tagged = 0;
            var opNames = new List<string>();

            // Process all methods in the IServer Interface
            foreach (var m in typeof(IServer).GetMethods().OrderBy(f => f.Name))
            {
                total += 1;
                var oc = (OperationContractAttribute[])m.GetCustomAttributes(typeof(OperationContractAttribute), false);
                if (!oc.Any())
                {
                    continue; // Contract not applied yet
                }

                var defaultFaultContract = false;
                foreach (var faultContract in (FaultContractAttribute[])m.GetCustomAttributes(typeof(FaultContractAttribute), false))
                {
                    if (faultContract.DetailType == typeof(BPServerFault))
                        defaultFaultContract  = true;
                }

                // Check that the default fault contract BPServerFault is present
                if (!defaultFaultContract )
                    Assert.Fail("BPServerFault not found on {0}", m.Name);
                tagged += 1;
                var exposedName = oc[0].Name;
                if (string.IsNullOrEmpty(exposedName))
                    exposedName = m.Name;
                if (opNames.Contains(exposedName))
                {
                    // Fail if operation name is not unique
                    Assert.Fail("Operation name not unique ({0})", exposedName);
                }

                opNames.Add(exposedName);
            }

            // Check that all methods are tagged
            Assert.That(tagged, Is.EqualTo(total));
        }

        [Test]
        [TestCaseSource("SecuredMethods")]
        public void TestSecured(Mono.Cecil.MethodDefinition method)
        {
            Assert.That(CheckPermissionsExistsFor(method), "Method {0} does not start with CheckPermissions()", method.Name);
            Assert.That(method.IsPublic, "Method {0} is not public", method.Name);
            Assert.IsTrue(method.HasOverrides, "Method {0} does not have a public interface", method.Name);
        }

        [Test]
        [TestCaseSource("UnsecuredMethods")]
        public void TestUnsecured(Mono.Cecil.MethodDefinition method)
        {
            Assert.That(CheckPermissionsExistsFor(method), Is.False, "Method {0} should not start with CheckPermissions()", method.Name);
            Assert.That(method.IsPublic, "Method {0} is not public", method.Name);
            Assert.IsTrue(method.HasOverrides, "Method {0} does not have a public interface", method.Name);
            Assert.That(method.Name, Does.Not.StartWith("Set").IgnoreCase, "Method {0} starts with 'Set'", method.Name);
        }

        [Test]
        [TestCaseSource("UnattributedMethods")]
        public void TestUnattributed(Mono.Cecil.MethodDefinition method)
        {
            Assert.That(method.IsPrivate || method.IsFamily || method.IsAssembly, "Method {0} is not private or protected or friend", method.Name);
            Assert.IsFalse(method.HasOverrides, "Method {0} has a public interface", method.Name);
        }

        #region "Test Support code"
        // A List of methods marked with the SecuredMethod  attribute
        protected static readonly ICollection<Mono.Cecil.MethodDefinition> SecuredMethods;

        // A List of methods marked with the UnsecuredMethod  attribute
        protected static readonly ICollection<Mono.Cecil.MethodDefinition> UnsecuredMethods;

        // A list of unattributed methods
        protected static readonly ICollection<Mono.Cecil.MethodDefinition> UnattributedMethods;

        // The signature of the CheckPermissions() method call
        protected static readonly Mono.Cecil.MethodDefinition CheckPermissionsMethod;

        /// <summary>
        /// Initialise the test case source data.
        /// </summary>
        static ServerAttributeTests()
        {
            SecuredMethods = new List<Mono.Cecil.MethodDefinition>();
            UnsecuredMethods = new List<Mono.Cecil.MethodDefinition>();
            UnattributedMethods = new List<Mono.Cecil.MethodDefinition>();

            var skippedMethods = new[] { "PublishProcess", "UnpublishProcess", "SetActualGroupPermissions", "GetProcessHistoryXML", "UpdatePasswordExpiryDate" };
            var server = typeof(clsServer);
            var methods = GetMethods(server, null).Where(m => m.IsPublic == true & m.IsConstructor == false & m.IsGetter == false & m.IsSetter == false).ToArray();
            CheckPermissionsMethod = GetMethods(server, "CheckPermissions").FirstOrDefault();
            foreach (var method in methods)
            {
                var secure = method.CustomAttributes.Any(t => (t.AttributeType.FullName ?? "") == (typeof(SecuredMethodAttribute).FullName ?? ""));
                var unsecure = method.CustomAttributes.Any(t => (t.AttributeType.FullName ?? "") == (typeof(UnsecuredMethodAttribute).FullName ?? ""));
                if (secure && !skippedMethods.Contains(method.Name))
                {
                    SecuredMethods.Add(method);
                }

                if (unsecure)
                {
                    UnsecuredMethods.Add(method);
                }

                if (!secure && !unsecure)
                {
                    UnattributedMethods.Add(method);
                }
            }
        }

        private static Mono.Cecil.MethodDefinition[] GetMethods(Type server, string methodName)
        {
            var assembly = Mono.Cecil.AssemblyDefinition.ReadAssembly(Assembly.Load("AutomateAppCore").Location);
            var methods = assembly.MainModule.GetTypes().SelectMany(t => t.Methods.Select(m => m))
                .Where(x => x.HasBody & (x.DeclaringType.FullName ?? "") == (server.FullName ?? "") & (methodName is null | (x.Name ?? "") == (methodName ?? ""))).ToArray();
            return methods;
        }

        /// <summary>
        /// Ensure the CheckPermissions() call exists inside the given methods body
        /// </summary>
        private static bool CheckPermissionsExistsFor(Mono.Cecil.MethodDefinition method)
        {
            foreach (var il in method.Body.Instructions)
            {
                if (il.OpCode == Mono.Cecil.Cil.OpCodes.Call)
                {
                    var mRef = (Mono.Cecil.MethodReference)il.Operand;
                    if (mRef is object & string.Equals(mRef.FullName, CheckPermissionsMethod.FullName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        #endregion
    }
}
