#if UNITTESTS

using BluePrism.Core.Expressions;
using NUnit.Framework;

namespace BluePrism.Core.UnitTests
{
    /// <summary>
    /// Tests the <see cref="BPExpression"/> class and its members.
    /// </summary>
    [TestFixture]
    public class ExpressionTests
    {
        /// <summary>
        /// Tests the <see cref="BPExpression.ReplaceDataItemName"/> method (and,
        /// tangentially, the "BPExpression.Equals" method and the '=='
        /// and '!=' operator overloads)
        /// </summary>
        [Test]
        public void TestReplaceDataItem()
        {
            BPExpression expr =
                BPExpression.FromNormalised("[one] + [two] = \"[three]\"");

            Assert.That(expr, Is.EqualTo(
                BPExpression.FromNormalised("[one] + [two] = \"[three]\"")));

            BPExpression result = expr.ReplaceDataItemName("one", "three");

            // Make sure that the original has not changed
            Assert.That(expr, Is.EqualTo(
                BPExpression.FromNormalised("[one] + [two] = \"[three]\"")));

            // Make sure that the result is correct
            Assert.That(result, Is.EqualTo(
                BPExpression.FromNormalised("[three] + [two] = \"[three]\"")));

            // Check that data item names within strings are not affected
            result = result.ReplaceDataItemName("three", "one");
            Assert.That(result, Is.EqualTo(
                BPExpression.FromNormalised("[one] + [two] = \"[three]\"")));
        }
    }
}

#endif