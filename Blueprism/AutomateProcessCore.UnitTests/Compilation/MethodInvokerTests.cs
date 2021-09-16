#if UNITTESTS
using BluePrism.AutomateProcessCore;
using BluePrism.AutomateProcessCore.Compilation;
using FluentAssertions;
using NUnit.Framework;

namespace AutomateProcessCore.UnitTests.Compilation
{
    public class MethodInvokerTests
    {
        [Test]
        public void Invoke_ShouldSetParametersFromInputs()
        {
            var value1 = new clsProcessValue("value 1");
            var value2 = new clsProcessValue("value 2");
            var inputs = new[] { new clsArgument("input1", value1), new clsArgument("input1", value2) };
            var outputs = new[] { new clsArgument("output1", new clsProcessValue("")), new clsArgument("output2", new clsProcessValue(string.Empty)) };
            var target = new TestClass();
            MethodInvoker.InvokeMethod(target, "Sub1", inputs, outputs);
            target.LastInput1.Should().Be(value1.EncodedValue);
            target.LastInput2.Should().Be(value2.EncodedValue);
        }

        [Test]
        public void Invoke_ShouldUpdateOutputs()
        {
            var value1 = new clsProcessValue("value 1");
            var value2 = new clsProcessValue("value 2");
            var inputs = new[] { new clsArgument("input1", value1), new clsArgument("input1", value2) };
            var outputs = new[] { new clsArgument("output1", new clsProcessValue("")), new clsArgument("output2", new clsProcessValue(string.Empty)) };
            var target = new TestClass();
            MethodInvoker.InvokeMethod(target, "Sub1", inputs, outputs);
            outputs[0].Value.Should().Be(value1);
            outputs[1].Value.Should().Be(value2);
        }

        protected class TestClass
        {
            public string LastInput1 { get; set; }
            public string LastInput2 { get; set; }

            public void Sub1(string input1, string input2, ref string output1, ref string output2)
            {
                LastInput1 = input1;
                LastInput2 = input2;
                output1 = input1;
                output2 = input2;
            }
        }
    }
}
#endif
