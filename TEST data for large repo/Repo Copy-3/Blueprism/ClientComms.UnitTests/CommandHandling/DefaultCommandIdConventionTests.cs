using System;
using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandling;
using FluentAssertions;
using NUnit.Framework;

namespace ClientComms.UnitTests.ClientComms.UnitTests.CommandHandling
{
    public class DefaultCommandIdConventionTests
    {
        private static readonly DefaultCommandIdConvention Convention = new DefaultCommandIdConvention();

        [Test]
        public void GetCommandId_WithAttribute_ShouldUseIdValue()
        {
            string id = Convention.GetId(typeof(HandlerWithAttribute));
            id.Should().Be("TestHandler1");
        }

        [Test]
        public void GetCommandId_WithoutAttribute_ShouldUseFullTypeName()
        {
            string id = Convention.GetId(typeof(HandlerWithoutAttribute));
            id.Should().Be(typeof(HandlerWithoutAttribute).FullName);
        }

        [CommandId("TestHandler1")]
        internal class HandlerWithAttribute : ICommandHandler
        {
            public Reply Execute(CommandContext context)
            {
                throw new NotImplementedException();
            }
        }

        internal class HandlerWithoutAttribute : ICommandHandler
        {
            public Reply Execute(CommandContext context)
            {
                throw new NotImplementedException();
            }
        }
    }
}