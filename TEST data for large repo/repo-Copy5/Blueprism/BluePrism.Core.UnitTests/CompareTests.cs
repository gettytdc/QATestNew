#if UNITTESTS

using System;
using NUnit.Framework;

namespace BluePrism.Core.UnitTests
{
    [TestFixture]
    public class CompareTests
    {

        [Test]
        public void TestEqualityComparer_null()
        {
            Assert.That(
                () => Compare.EqualityComparer<string, string>(null),
                Throws.InstanceOf<ArgumentNullException>()
            );
        }

        [Test]
        public void TestEqualityComparer_sameobject()
        {
            var comp = Compare.EqualityComparer((string s) => s);

            Assert.That(comp.Equals("same", "same"), Is.True);
            Assert.That(comp.Equals("", ""), Is.True);
            Assert.That(comp.Equals(null, null), Is.True);

            Assert.That(comp.Equals("different", "differing"), Is.False);
            Assert.That(comp.Equals("not the one", "NOT THE ONE"), Is.False);
            Assert.That(comp.Equals("", null), Is.False);
            Assert.That(comp.Equals(null, ""), Is.False);
            Assert.That(comp.Equals(" ", ""), Is.False);

            Assert.That(comp.GetHashCode(""), Is.EqualTo("".GetHashCode()));
            Assert.That(comp.GetHashCode(null), Is.EqualTo(0));
            Assert.That(comp.GetHashCode("this"), Is.EqualTo("this".GetHashCode()));
        }

        [Test]
        public void TestEqualityComparer_diffobject()
        {
            // case-insensitive
            var comp = Compare.EqualityComparer((string s) => s?.ToLowerInvariant());

            Assert.That(comp.Equals("same", "same"), Is.True);
            Assert.That(comp.Equals("", ""), Is.True);
            Assert.That(comp.Equals(null, null), Is.True);

            Assert.That(comp.Equals("the one", "THE ONE"), Is.True);
            Assert.That(comp.Equals("", null), Is.False);
            Assert.That(comp.Equals(null, ""), Is.False);
            Assert.That(comp.Equals(" ", ""), Is.False);

            Assert.That(
                comp.GetHashCode("lower"), Is.EqualTo(comp.GetHashCode("Lower")));

            comp = Compare.EqualityComparer((string s) => (s ?? "").Length);

            Assert.That(comp.Equals("one", "two"), Is.True);
            Assert.That(comp.Equals("two", "three"), Is.False);

            // Because of the way we're reporting the length, null and "" will work
            // out the same.
            Assert.That(comp.Equals("", null), Is.True);

        }

        [Test]
        public void TestEqualityComparer_valuetype()
        {
            // All integers whose remainder is the same when divided by 3 are
            // considered equal. The opportunities afforded by this algorithm are 0.
            var comp = Compare.EqualityComparer((int i) => i % 3);

            Assert.That(comp.Equals(0, 0), Is.True);
            Assert.That(comp.Equals(0, 3), Is.True);
            Assert.That(comp.Equals(6, 3), Is.True);
            Assert.That(comp.Equals(6, 0), Is.True);
            Assert.That(comp.Equals(1, 4), Is.True);
            Assert.That(comp.Equals(2, 5), Is.True);
            Assert.That(comp.Equals(34, 967), Is.True);
            Assert.That(comp.Equals(999, 3), Is.True);
            Assert.That(comp.Equals(31850, 1467569), Is.True);

            Assert.That(comp.Equals(0, 1), Is.False);
            Assert.That(comp.Equals(0, 2), Is.False);
            Assert.That(comp.Equals(2, 1), Is.False);
            Assert.That(comp.Equals(2, 2), Is.True);

            Assert.That(comp.GetHashCode(2), Is.EqualTo(comp.GetHashCode(5)));
            Assert.That(comp.GetHashCode(0), Is.EqualTo(comp.GetHashCode(3)));
            Assert.That(
                comp.GetHashCode(31850), Is.EqualTo(comp.GetHashCode(1467569)));

            Assert.That(comp.GetHashCode(1), Is.Not.EqualTo(comp.GetHashCode(3)));
            Assert.That(comp.GetHashCode(1), Is.Not.EqualTo(comp.GetHashCode(0)));

        }

        [Test]
        public void TestComparer_null()
        {
            Assert.That(
                () => Compare.Comparer<string>(null),
                Throws.InstanceOf<ArgumentNullException>()
            );
        }

        [Test]
        public void TestComparer_int()
        {
            // reverse comparison
            var comp = Compare.Comparer((int x, int y) => y - x);

            Assert.That(comp.Compare(2, 1), Is.Negative);
            Assert.That(comp.Compare(1, 2), Is.Positive);
            Assert.That(comp.Compare(0, 0), Is.EqualTo(0));
            Assert.That(comp.Compare(2, 2), Is.EqualTo(0));
        }

        [Test]
        public void TestComparer_string()
        {
            // A redundantly redundant IComparer
            var comp = Compare.Comparer(
                (string x, string y) => StringComparer.OrdinalIgnoreCase.Compare(x, y)
            );

            Assert.That(comp.Compare("same", "SAME"), Is.EqualTo(0));
            Assert.That(comp.Compare("earlier?", "later?"), Is.EqualTo(
                StringComparer.OrdinalIgnoreCase.Compare("earlier?", "later?")));
            Assert.That(comp.Compare("A", "b"), Is.Negative);
            Assert.That(comp.Compare("C", "b"), Is.Positive);
        }

    }
}

#endif