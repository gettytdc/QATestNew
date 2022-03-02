using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BluePrism.ApplicationManager.CommandHandling;
using ClientComms.UnitTests.ClientComms.UnitTests.CommandHandling.TestHandlers.Html;
using ClientComms.UnitTests.ClientComms.UnitTests.CommandHandling.TestHandlers.UIAutomation;
using FluentAssertions;
using NUnit.Framework;

namespace ClientComms.UnitTests.ClientComms.UnitTests.CommandHandling
{
    public class HandlerScannerTests
    {
        private static readonly string ParentNamespace;
        private static readonly Assembly TestAssembly = Assembly.GetExecutingAssembly();

        static HandlerScannerTests()
        {
            string testNamespace = typeof(ExampleUIAutomation1Handler).Namespace;
            ParentNamespace = testNamespace.Substring(0, testNamespace.LastIndexOf("."));
        }

        [Test]
        public void GetHandlers_WithNamespaceContainingHandlers_ShouldFindAllHandlers()
        {
            var descriptors = FindHandlersInParentNamespace();            
            var expected = new[]
            {
                    new { HandlerType = typeof(ExampleHtml1Handler), CommandId = "HtmlExample1" },
                    new { HandlerType = typeof(ExampleHtml2Handler), CommandId = "HtmlExample2" },
                    new { HandlerType = typeof(ExampleUIAutomation1Handler), CommandId = "UiaExample1" },
                    new { HandlerType = typeof(ExampleUIAutomation2Handler), CommandId = "UiaExample2" },

            };
            descriptors.ShouldAllBeEquivalentTo(expected);
        }

        [Test]
        public void GetHandlers_WithNamespaceContainingHandlers_ShouldExcludeNonConcreteTypes()
        {
            var descriptors = FindHandlersInParentNamespace();
            descriptors.Select(descriptor => descriptor.HandlerType).Should().NotContain(typeof(HtmlHandlerBase));
        }

        private List<HandlerDescriptor> FindHandlersInParentNamespace()
        {
            var scanner = new HandlerScanner(new DefaultCommandIdConvention());
            return scanner.FindHandlers(TestAssembly, ParentNamespace).ToList();
        }
    }
}