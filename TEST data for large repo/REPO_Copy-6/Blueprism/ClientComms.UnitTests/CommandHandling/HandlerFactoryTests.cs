using System;
using System.Collections.Generic;
using BluePrism.ApplicationManager;
using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandling;
using BluePrism.Server.Domain.Models;
using BluePrism.BPCoreLib.DependencyInjection;
using BluePrism.Utilities.Testing;
using Moq;
using NUnit.Framework;

namespace ClientComms.UnitTests.ClientComms.UnitTests.CommandHandling
{
    public class HandlerFactoryTests : UnitTestBase<clsLocalTargetApp>
    {
        private clsLocalTargetApp Application { get; set; }
        private IHandlerDependencyResolver Resolver { get; set; }
        private HandlerFactory Factory { get; set; }

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            DependencyResolver.SetContainer(Container);
            var handlers = new HandlerDescriptor[] { new HandlerDescriptor(typeof(TestHandler1), "test"), new HandlerDescriptor(typeof(TestHandlerMissingConstructor), "testmissingconstructor") };
            var dependencies = new Dictionary<Type, Func<clsLocalTargetApp, object>>() { { typeof(TestService1), app => new TestService1() }, { typeof(TestService2), app => new TestService2() } };
            Resolver = new HandlerDependencyResolver(dependencies);
            Application = new clsLocalTargetApp();
            Factory = new HandlerFactory(handlers, Resolver);
        }

        [Test]
        public void Constructor_WithDuplicateCommandIds_ShouldThrow()
        {
            var handlers = new HandlerDescriptor[] { new HandlerDescriptor(typeof(TestHandler1), "test"), new HandlerDescriptor(typeof(TestHandler2), "test") };
            var exception = Assert.Throws<DuplicateException>(() => { var j = new HandlerFactory(handlers, Moq.Mock.Of<IHandlerDependencyResolver>()); });
            string expected = "Duplicate command ids were found. The following handlers have duplicate ids:";
            Assert.That(exception.Message, Contains.Substring(expected));
            Assert.That(exception.Message, Contains.Substring("test: " + typeof(TestHandler1).FullName));
            Assert.That(exception.Message, Contains.Substring("test: " + typeof(TestHandler2).FullName));
        }

        [Test]
        public void GetHandler_WithRecognisedHandler_ShouldCreateHandler()
        {
            var handler = Factory.CreateHandler(Application, clsQuery.Parse("test"));
            Assert.That(handler, Is.Not.Null);
            Assert.That(handler, Is.InstanceOf(typeof(TestHandler1)));
        }

        [Test]
        public void GetHandler_WithRecognisedHandler_ShouldInitialiseDependencies()
        {
            TestHandler1 handler = (TestHandler1)Factory.CreateHandler(Application, clsQuery.Parse("test"));
            Assert.That(handler.Service1, Is.InstanceOf(typeof(TestService1)));
            Assert.That(handler.Service2, Is.InstanceOf(typeof(TestService2)));
        }

        [Test]
        public void GetHandler_WithUnrecognisedHandler_ShouldReturnNull()
        {
            var handler = Factory.CreateHandler(Application, clsQuery.Parse("unknown"));
            Assert.That(handler, Is.Null);
        }

        [Test]
        public void GetHandler_WithHandlerMissingPublicConstructor_ShouldThrow()
        {
            Assert.Throws<MissingConstructorException>(() => Factory.CreateHandler(Application, clsQuery.Parse("testmissingconstructor")));
        }

        private class TestService1
        {
        }

        private class TestService2
        {
        }

        private class TestHandler1 : ICommandHandler
        {
            public TestHandler1(TestService1 service1, TestService2 service2)
            {
                Service1 = service1;
                Service2 = service2;
            }

            public TestService1 Service1 { get; private set; }
            public TestService2 Service2 { get; private set; }

            public Reply Execute(CommandContext context)
            {
                throw new NotImplementedException();
            }
        }

        private class TestHandler2 : ICommandHandler
        {
            public Reply Execute(CommandContext context)
            {
                throw new NotImplementedException();
            }
        }

        internal class TestHandlerMissingConstructor : ICommandHandler
        {
            protected TestHandlerMissingConstructor()
            {
            }

            public Reply Execute(CommandContext context)
            {
                throw new NotImplementedException();
            }
        }
    }
}
