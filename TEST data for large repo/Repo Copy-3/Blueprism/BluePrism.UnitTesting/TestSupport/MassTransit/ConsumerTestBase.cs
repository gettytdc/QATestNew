using System;
using Autofac;
using BluePrism.Utilities.Testing;
using MassTransit;
using MassTransit.Testing;

namespace BluePrism.UnitTesting.TestSupport.MassTransit
{
    public abstract class ConsumerTestBase<T> : UnitTestBase<T>
        where T : class, IConsumer
    {
        protected virtual void ConfigureBus(InMemoryTestHarness bus) { }

        public override void Setup(Action<ContainerBuilder> container)
        {
            base.Setup(container);
            Bus = new InMemoryTestHarness { TestTimeout = TimeSpan.FromSeconds(30), TestInactivityTimeout = TimeSpan.FromSeconds(1) };
            Bus.Consumer(() => ClassUnderTest);
            ConfigureBus(Bus);
            Bus.Start().GetAwaiter().GetResult();
        }

        public override void TearDown()
        {
            base.TearDown();

            Bus.Stop().GetAwaiter().GetResult();
        }

        protected InMemoryTestHarness Bus { get; private set; }
    }
}
