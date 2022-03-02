using System.Threading;
using BluePrism.Utilities.Testing;

namespace BluePrism.Caching.UnitTests
{
    using System;
    using System.Data;
    using FluentAssertions;
    using NUnit.Framework;
    using UnitTesting;

    [TestFixture]
    public class InMemoryCacheWithDatabaseRefreshTests : UnitTestBase<InMemoryCacheWithDatabaseRefresh<int>>
    {
        const int databaseRefreshtimer = 10;

        protected override InMemoryCacheWithDatabaseRefresh<int> TestClassConstructor() =>
            new InMemoryCacheWithDatabaseRefresh<int>(
                "Test",
                "",
                databaseRefreshtimer,
                _ => GetMock<IDbConnection>().Object,
                (x, y) => GetMock<IDbCommand>().Object);

        public override void Setup()
        {
            base.Setup();

            var dbCommandMock = GetMock<IDbCommand>();
            dbCommandMock
                .Setup(m => m.CreateParameter())
                .Returns(() => GetMock<IDbDataParameter>().Object);
            dbCommandMock
                .SetupGet(m => m.Parameters)
                .Returns(() => GetMock<IDataParameterCollection>().Object);
        }

        public override void TearDown()
        {
            base.TearDown();
            ClassUnderTest.Dispose();
        }

        [Test]
        public void GetValueCallsFunctionWhenNotPreviouslySet()
        {
            var called = false;
            ClassUnderTest.GetValue("Test", () =>
            {
                called = true;
                return 1;
            });

            called.Should().BeTrue();
        }

        [Test]
        public void GetValueReturnsValueFromFunctionWhenNotPreviouslySet()
        {
            var result = ClassUnderTest.GetValue("Test", () => 42);
            result.Should().Be(42);
        }

        [Test]
        public void GetValueReturnsValueFromCache()
        {
            ClassUnderTest.GetValue("Test", () => 42);
            var result = ClassUnderTest.GetValue("Test", () => 12345);

            result.Should().Be(42);
        }

        [Test]
        public void GetValueReturnsCorrectValueAfterSetValue()
        {
            ClassUnderTest.GetValue("Test", () => 12345);
            ClassUnderTest.SetValue("Test", 42);
            var result = ClassUnderTest.GetValue("Test", () => 12345);

            result.Should().Be(42);
        }

        [Test]
        public void GetValueReturnsValueFromFunctionAfterRemove()
        {
            ClassUnderTest.GetValue("Test", () => 12345);
            ClassUnderTest.Remove("Test");
            var result = ClassUnderTest.GetValue("Test", () => 42);

            result.Should().Be(42);
        }

        [Test]
        public void OnRefreshRequiredEventRaisedWhenDataIsInvalidated()
        {
            using (var signal = new ManualResetEventSlim())
            {
                var entityTag = Guid.NewGuid();
                var databaseCommandMock = GetMock<IDbCommand>();
                var timeToWait = new TimeSpan(0, 0, 5);
                databaseCommandMock
                    .Setup(m => m.ExecuteScalar())
                    // ReSharper disable once AccessToModifiedClosure
                    .Returns(() => entityTag);

                ClassUnderTest.GetValue("Test", () => 12345);
                // ReSharper disable once AccessToDisposedClosure
                ClassUnderTest.OnRefreshRequired += (s, e) => { signal.Set(); };
                entityTag = Guid.NewGuid();
                signal.Wait(timeToWait);
                var result = signal.IsSet;

                result.Should().BeTrue();
            }
        }

        [Test]
        public void ValueRefreshedAfterDataInvalidated()
        {
            var entityTag = Guid.Parse("81AB4B57-C4BB-4F26-A164-B5F789CAFE9C");

            var databaseCommandMock = GetMock<IDbCommand>();
            databaseCommandMock
                .Setup(m => m.ExecuteScalar())            
                .Returns(() => entityTag);

            using (var signal = new ManualResetEventSlim())
            {            
                ClassUnderTest.OnRefreshRequired += (s, e) => 
                {
                    signal.Set();
                    Thread.Yield();
                };
                //wait the first set
                ClassUnderTest.GetValue("Test", () => 12345);
                signal.Wait((databaseRefreshtimer + 1) * 1000);
                signal.Reset();

                entityTag = Guid.Parse("015A7F52-8441-45D2-AD6B-20FC3AF669EB");
                signal.Wait((databaseRefreshtimer + 1) * 1000); // to ensure a full cycle of database refesh check

                var result = ClassUnderTest.GetValue("Test", () => 42);
                result.Should().Be(42);
            }
        }

        [Test]
        public void ClearInvalidatesData()
        {
            var entityTag = Guid.NewGuid();

            var databaseCommandMock = GetMock<IDbCommand>();
            databaseCommandMock
                .Setup(m => m.ExecuteScalar())
                .Returns(() => entityTag);
            databaseCommandMock
                .Setup(m => m.ExecuteNonQuery())
                .Callback(() => entityTag = Guid.NewGuid());

            ClassUnderTest.GetValue("Test", () => 12345);

            using (var signal = new ManualResetEventSlim())
            {
                // ReSharper disable once AccessToDisposedClosure
                ClassUnderTest.OnRefreshRequired += (s, e) => { signal.Set(); };
                entityTag = Guid.NewGuid();

                ClassUnderTest.Clear();

                signal.Wait(5000);

                var result = ClassUnderTest.GetValue("Test", () => 42);
                result.Should().Be(42);
            }
        }
    }
}
