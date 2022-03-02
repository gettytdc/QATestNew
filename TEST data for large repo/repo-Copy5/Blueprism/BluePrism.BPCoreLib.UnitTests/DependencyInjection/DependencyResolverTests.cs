#if UNITTESTS

using System;
using Autofac;
using BluePrism.BPCoreLib.DependencyInjection;
using NUnit.Framework;

namespace BluePrism.BPCoreLib.UnitTests.DependencyInjection
{
    public class DependencyResolverTests
    {
        [SetUp]
        public void SetUp() => DependencyResolver.Reset();

        [Test]
        public void Initialise_ShouldInitialiseContainer()
        {
            var container = CreateContainer();
            DependencyResolver.Initialise(container);
            var service = DependencyResolver.Resolve<TestService1>();
            Assert.That(service, Is.Not.Null);
        }

        [Test]
        public void Initialise_WhenAlreadyInitialised_ShouldThrow()
        {
            var container = CreateContainer();
            DependencyResolver.Initialise(container);
            Assert.Throws<InvalidOperationException>(() => DependencyResolver.Initialise(container));
        }

        [Test]
        public void Resolve_WithKnownService_ShouldReturnObject()
        {
            var container = CreateContainer();
            DependencyResolver.Initialise(container);
            var service1 = DependencyResolver.Resolve<TestService1>();
            var service2 = DependencyResolver.Resolve<TestService2>();
            Assert.That(service1, Is.Not.Null);
            Assert.That(service2, Is.Not.Null);
        }

        [Test]
        public void InScope_WithKnownServiceAndAction_ShouldResolveInSeparateScope()
        {
            var container = CreateContainer();
            DependencyResolver.Initialise(container);
            DependencyResolver.InScope<TestService1>(instance1 =>
            {
                Assert.That(instance1, Is.Not.Null);
                DependencyResolver.InScope<TestService1>(instance2 => Assert.That(instance1, Is.Not.SameAs(instance2)));
            });
        }

        [Test]
        public void InScope_WithConfiguration_ShouldUseScopeSpecificRegistration()
        {
            var container = CreateContainer();
            DependencyResolver.Initialise(container);
            var service1 = new TestService1();
            DependencyResolver.InScope<TestService3>(builder => builder.RegisterInstance(service1).AsSelf(), instance1 =>
            {
                Assert.That(instance1.Dependency1, Is.SameAs(service1));
                Assert.That(instance1, Is.Not.Null);
            });
        }

        [Test]
        public void FromScope_WithKnownService_ShouldResolveInSeparateScope()
        {
            var container = CreateContainer();
            DependencyResolver.Initialise(container);
            var instance1 = DependencyResolver.FromScope((TestService1 s) => s);
            var instance2 = DependencyResolver.FromScope((TestService1 s) => s);
            Assert.That(instance1, Is.Not.SameAs(instance2));
        }

        [Test]
        public void FromScope_WithConfiguration_ShouldUseScopeSpecificRegistration()
        {
            var container = CreateContainer();
            DependencyResolver.Initialise(container);
            var dependencyInstance1 = new TestService1();
            var dependencyInstance2 = new TestService1();
            var instance1 = DependencyResolver.FromScope((builder) => builder.RegisterInstance(dependencyInstance1).AsSelf(), (TestService3 s) => s);
            var instance2 = DependencyResolver.FromScope((builder) => builder.RegisterInstance(dependencyInstance2).AsSelf(), (TestService3 s) => s);
            Assert.That(instance1.Dependency1, Is.SameAs(dependencyInstance1));
            Assert.That(instance2.Dependency1, Is.SameAs(dependencyInstance2));
        }

        private IContainer CreateContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<TestService1>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<TestService2>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<TestService3>().AsSelf().InstancePerLifetimeScope();
            return builder.Build();
        }
    }

    public class TestService1
    {
    }

    public class TestService2
    {
    }

    public class TestService3
    {
        public TestService3(TestService1 dependency1)
        {
            Dependency1 = dependency1;
        }

        public TestService1 Dependency1 { get; private set; }
    }
}

#endif
