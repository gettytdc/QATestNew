namespace BluePrism.Core.UnitTests.TestSupport
{
    using System.Collections.Generic;
    using BluePrism.UnitTesting.TestSupport;
    using NUnit.Framework;

    [TestFixture]
    public class MethodReplacerTests
    {
        [Test]
        public void ReplaceInstanceNoParametersNoReturnWithExpression()
        {
            var classUnderTest = new TestClass();

            classUnderTest.InstanceNoParametersNoReturn();

            MethodReplacer<TestClass, InjectClass>.ReplaceMethod(
                dst => dst.InstanceNoParametersNoReturn(),
                src => src.InstanceNoParametersNoReturn());

            classUnderTest.InstanceNoParametersNoReturn();

            Assert.AreEqual(1, TestClass.GetCallCount("InstanceNoParametersNoReturn"));
            Assert.AreEqual(1, InjectClass.GetCallCount("InstanceNoParametersNoReturn"));
        }

        [Test]
        public void ReplaceInstanceNoParametersReturnsWithExpression()
        {
            var classUnderTest = new TestClass();

            classUnderTest.InstanceNoParametersReturns();

            MethodReplacer<TestClass, InjectClass>.ReplaceMethod(
                dst => dst.InstanceNoParametersReturns(),
                src => src.InstanceNoParametersReturns());

            classUnderTest.InstanceNoParametersReturns();

            Assert.AreEqual(1, TestClass.GetCallCount("InstanceNoParametersReturns"));
            Assert.AreEqual(1, InjectClass.GetCallCount("InstanceNoParametersReturns"));
        }

        [Test]
        public void ReplaceInstanceOneParameterNoReturn()
        {
            var classUnderTest = new TestClass();

            classUnderTest.InstanceOneParameterNoReturn(1);

            MethodReplacer<TestClass, InjectClass>.ReplaceMethod(
                dst => dst.InstanceOneParameterNoReturn(Parameter.Of<int>()),
                src => src.InstanceOneParameterNoReturn(Parameter.Of<int>()));

            classUnderTest.InstanceOneParameterNoReturn(1);

            Assert.AreEqual(1, TestClass.GetCallCount("InstanceOneParameterNoReturn"));
            Assert.AreEqual(1, InjectClass.GetCallCount("InstanceOneParameterNoReturn"));
        }

        [Test]
        public void ReplaceInstanceOneParameterReturns()
        {
            var classUnderTest = new TestClass();

            classUnderTest.InstanceOneParameterReturns(1);

            MethodReplacer<TestClass, InjectClass>.ReplaceMethod(
                dst => dst.InstanceOneParameterReturns(Parameter.Of<int>()),
                src => src.InstanceOneParameterReturns(Parameter.Of<int>()));

            classUnderTest.InstanceOneParameterReturns(1);

            Assert.AreEqual(1, TestClass.GetCallCount("InstanceOneParameterReturns"));
            Assert.AreEqual(1, InjectClass.GetCallCount("InstanceOneParameterReturns"));
        }

        [Test]
        public void ReplaceInstanceTwoParametersNoReturn()
        {
            var classUnderTest = new TestClass();

            classUnderTest.InstanceTwoParametersNoReturn(1, 2);

            MethodReplacer<TestClass, InjectClass>.ReplaceMethod(
                dst => dst.InstanceTwoParametersNoReturn(Parameter.Of<int>(), Parameter.Of<int>()),
                src => src.InstanceTwoParametersNoReturn(Parameter.Of<int>(), Parameter.Of<int>()));

            classUnderTest.InstanceTwoParametersNoReturn(1, 2);

            Assert.AreEqual(1, TestClass.GetCallCount("InstanceTwoParametersNoReturn"));
            Assert.AreEqual(1, InjectClass.GetCallCount("InstanceTwoParametersNoReturn"));
        }

        [Test]
        public void ReplaceInstanceTwoParametersReturns()
        {
            var classUnderTest = new TestClass();

            classUnderTest.InstanceTwoParametersReturns(1, 2);

            MethodReplacer<TestClass, InjectClass>.ReplaceMethod(
                dst => dst.InstanceTwoParametersReturns(Parameter.Of<int>(), Parameter.Of<int>()),
                src => src.InstanceTwoParametersReturns(Parameter.Of<int>(), Parameter.Of<int>()));

            classUnderTest.InstanceTwoParametersReturns(1, 2);

            Assert.AreEqual(1, TestClass.GetCallCount("InstanceTwoParametersReturns"));
            Assert.AreEqual(1, InjectClass.GetCallCount("InstanceTwoParametersReturns"));
        }

        [Test]
        public void ReplaceStaticNoParametersNoReturn()
        {
            TestClass.StaticNoParametersNoReturn();

            MethodReplacer.ReplaceMethod(
                () => TestClass.StaticNoParametersNoReturn(),
                () => InjectClass.StaticNoParametersNoReturn());

            TestClass.StaticNoParametersNoReturn();
            Assert.AreEqual(1, TestClass.GetCallCount("StaticNoParametersNoReturn"));
            Assert.AreEqual(1, InjectClass.GetCallCount("StaticNoParametersNoReturn"));
        }

        [Test]
        public void ReplaceStaticNoParametersReturns()
        {
            TestClass.StaticNoParametersReturns();

            MethodReplacer.ReplaceMethod(
                () => TestClass.StaticNoParametersReturns(),
                () => InjectClass.StaticNoParametersReturns());

            TestClass.StaticNoParametersReturns();

            Assert.AreEqual(1, TestClass.GetCallCount("StaticNoParametersReturns"));
            Assert.AreEqual(1, InjectClass.GetCallCount("StaticNoParametersReturns"));
        }

        [Test]
        public void ReplaceStaticOneParameterNoReturn()
        {
            TestClass.StaticOneParameterNoReturn(1);

            MethodReplacer.ReplaceMethod(
                () => TestClass.StaticOneParameterNoReturn(Parameter.Of<int>()),
                () => InjectClass.StaticOneParameterNoReturn(Parameter.Of<int>()));

            TestClass.StaticOneParameterNoReturn(1);

            Assert.AreEqual(1, TestClass.GetCallCount("StaticOneParameterNoReturn"));
            Assert.AreEqual(1, InjectClass.GetCallCount("StaticOneParameterNoReturn"));
        }

        [Test]
        public void ReplaceStaticOneParameterReturns()
        {
            TestClass.StaticOneParameterReturns(1);

            MethodReplacer.ReplaceMethod(
                () => TestClass.StaticOneParameterReturns(Parameter.Of<int>()),
                () => InjectClass.StaticOneParameterReturns(Parameter.Of<int>()));

            TestClass.StaticOneParameterReturns(1);

            Assert.AreEqual(1, TestClass.GetCallCount("StaticOneParameterReturns"));
            Assert.AreEqual(1, InjectClass.GetCallCount("StaticOneParameterReturns"));
        }

        [Test]
        public void ReplaceStaticTwoParametersNoReturn()
        {
            TestClass.StaticTwoParametersNoReturn(1, 2);

            MethodReplacer.ReplaceMethod(
                () => TestClass.StaticTwoParametersNoReturn(Parameter.Of<int>(), Parameter.Of<int>()),
                () => InjectClass.StaticTwoParametersNoReturn(Parameter.Of<int>(), Parameter.Of<int>()));

            TestClass.StaticTwoParametersNoReturn(1, 2);

            Assert.AreEqual(1, TestClass.GetCallCount("StaticTwoParametersNoReturn"));
            Assert.AreEqual(1, InjectClass.GetCallCount("StaticTwoParametersNoReturn"));
        }

        [Test]
        public void ReplaceStaticTwoParametersReturns()
        {
            TestClass.StaticTwoParametersReturns(1, 2);

            MethodReplacer.ReplaceMethod(
                () => TestClass.StaticTwoParametersReturns(Parameter.Of<int>(), Parameter.Of<int>()),
                () => InjectClass.StaticTwoParametersReturns(Parameter.Of<int>(), Parameter.Of<int>()));

            TestClass.StaticTwoParametersReturns(1, 2);

            Assert.AreEqual(1, TestClass.GetCallCount("StaticTwoParametersReturns"));
            Assert.AreEqual(1, InjectClass.GetCallCount("StaticTwoParametersReturns"));
        }

        private class TestClass
        {
            public static readonly Dictionary<string, int> CalledMethods = new Dictionary<string, int>();

            public void InstanceNoParametersNoReturn()
            {
                IncrementMethodCallCount(nameof(InstanceNoParametersNoReturn));
            }

            public int InstanceNoParametersReturns()
            {
                IncrementMethodCallCount(nameof(InstanceNoParametersReturns));
                return 12;
            }

            public void InstanceOneParameterNoReturn(int value)
            {
                IncrementMethodCallCount(nameof(InstanceOneParameterNoReturn));
            }

            public int InstanceOneParameterReturns(int value)
            {
                IncrementMethodCallCount(nameof(InstanceOneParameterReturns));
                return value + 1;
            }

            public void InstanceTwoParametersNoReturn(int value1, int value2)
            {
                IncrementMethodCallCount(nameof(InstanceTwoParametersNoReturn));
            }

            public int InstanceTwoParametersReturns(int value1, int value2)
            {
                IncrementMethodCallCount(nameof(InstanceTwoParametersReturns));
                return value1 + value2;
            }

            public static void StaticNoParametersNoReturn()
            {
                IncrementMethodCallCount(nameof(StaticNoParametersNoReturn));
            }

            public static int StaticNoParametersReturns()
            {
                IncrementMethodCallCount(nameof(StaticNoParametersReturns));
                return 34;
            }

            public static void StaticOneParameterNoReturn(int value)
            {
                IncrementMethodCallCount(nameof(StaticOneParameterNoReturn));
            }

            public static int StaticOneParameterReturns(int value)
            {
                IncrementMethodCallCount(nameof(StaticOneParameterReturns));
                return value - 1;
            }

            public static void StaticTwoParametersNoReturn(int value1, int value2)
            {
                IncrementMethodCallCount(nameof(StaticTwoParametersNoReturn));
            }

            public static int StaticTwoParametersReturns(int value1, int value2)
            {
                IncrementMethodCallCount(nameof(StaticTwoParametersReturns));
                return value1 - value2;
            }

            public int InstanceProperty { get; set; } = 12;

            public static int StaticProperty { get; set; } = 34;

            private static void IncrementMethodCallCount(string methodName)
            {
                CalledMethods[methodName] = GetCallCount(methodName) + 1;
            }

            public static int GetCallCount(string methodName) =>
                CalledMethods.ContainsKey(methodName)
                    ? CalledMethods[methodName]
                    : 0;
        }

        private class InjectClass
        {
            public static readonly Dictionary<string, int> CalledMethods = new Dictionary<string, int>();

            public void InstanceNoParametersNoReturn()
            {
                IncrementMethodCallCount(nameof(InstanceNoParametersNoReturn));
            }

            public int InstanceNoParametersReturns()
            {
                IncrementMethodCallCount(nameof(InstanceNoParametersReturns));
                return 12;
            }

            public void InstanceOneParameterNoReturn(int value)
            {
                IncrementMethodCallCount(nameof(InstanceOneParameterNoReturn));
            }

            public int InstanceOneParameterReturns(int value)
            {
                IncrementMethodCallCount(nameof(InstanceOneParameterReturns));
                return value + 1;
            }

            public void InstanceTwoParametersNoReturn(int value1, int value2)
            {
                IncrementMethodCallCount(nameof(InstanceTwoParametersNoReturn));
            }

            public int InstanceTwoParametersReturns(int value1, int value2)
            {
                IncrementMethodCallCount(nameof(InstanceTwoParametersReturns));
                return value1 + value2;
            }

            public static void StaticNoParametersNoReturn()
            {
                IncrementMethodCallCount(nameof(StaticNoParametersNoReturn));
            }

            public static int StaticNoParametersReturns()
            {
                IncrementMethodCallCount(nameof(StaticNoParametersReturns));
                return 34;
            }

            public static void StaticOneParameterNoReturn(int value)
            {
                IncrementMethodCallCount(nameof(StaticOneParameterNoReturn));
            }

            public static int StaticOneParameterReturns(int value)
            {
                IncrementMethodCallCount(nameof(StaticOneParameterReturns));
                return value - 1;
            }

            public static void StaticTwoParametersNoReturn(int value1, int value2)
            {
                IncrementMethodCallCount(nameof(StaticTwoParametersNoReturn));
            }

            public static int StaticTwoParametersReturns(int value1, int value2)
            {
                IncrementMethodCallCount(nameof(StaticTwoParametersReturns));
                return value1 - value2;
            }

            public int InstanceProperty { get; set; } = 56;

            public static int StaticProperty { get; set; } = 78;

            private static void IncrementMethodCallCount(string methodName)
            {
                CalledMethods[methodName] = GetCallCount(methodName) + 1;
            }

            public static int GetCallCount(string methodName) =>
                CalledMethods.ContainsKey(methodName)
                    ? CalledMethods[methodName]
                    : 0;
        }
    }


}
