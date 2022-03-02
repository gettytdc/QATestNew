#if UNITTESTS

using System;
using NUnit.Framework;

namespace BluePrism.BPCoreLib.UnitTests
{
    [TestFixture]
    public class EnumTests
    {
        public enum Synonymised
        {
            Zeroeth = 0,
            First,
            Second,
            Third,
            Primary = First,
            Initial = First,
            Tertiary = Third
        }

        [Test]
        public void TestSynonymised()
        {
            var se = default(Synonymised);
            Assert.That(clsEnum.TryParse("zeroeth", true, ref se), Is.True);
            Assert.That(se, Is.EqualTo(Synonymised.Zeroeth));
            Assert.That(clsEnum.TryParse("First", ref se), Is.True);
            Assert.That(se, Is.EqualTo(Synonymised.First));
            Assert.That(clsEnum.TryParse("Primary", ref se), Is.True);
            Assert.That(se, Is.EqualTo(Synonymised.Primary));
            Assert.That(clsEnum.TryParse("Second", ref se), Is.True);
            Assert.That(se, Is.EqualTo(Synonymised.Second));
            Assert.That(clsEnum.TryParse("initial", false, ref se), Is.False);
            Assert.That(clsEnum<Synonymised>.Parse("First"), Is.EqualTo(Synonymised.First));
        }

        [Test]
        public void TestParse()
        {
            var ts = default(TriState);
            Assert.That(clsEnum<TriState>.Parse("Enabled"), Is.EqualTo(TriState.Enabled));
            Assert.That(clsEnum<TriState>.Parse("Disabled"), Is.EqualTo(TriState.Disabled));
            Assert.That(clsEnum<TriState>.Parse("enabled", true), Is.EqualTo(TriState.Enabled));

            try
            {
                clsEnum<TriState>.Parse("enabled");
                Assert.Fail("Parse(\"enabled\") should have failed");
            }
            catch (ArgumentException)
            {
                // Do nothing
            }

            Assert.That(clsEnum<TriState>.TryParse("enabled", ref ts), Is.False);
            Assert.That(clsEnum<TriState>.TryParse("enabled", true, ref ts), Is.True);
            Assert.That(ts, Is.EqualTo(TriState.Enabled));
            Assert.That(clsEnum.TryParse("disabled", ref ts), Is.False);
            Assert.That(ts, Is.EqualTo((TriState)0)); // ie. the default value
            Assert.That(clsEnum.TryParse("disabled", true, ref ts), Is.True);
            Assert.That(ts, Is.EqualTo(TriState.Disabled));
            Assert.That(clsEnum<TriState>.TryParse("Indeterminate", ref ts), Is.True);
            Assert.That(ts, Is.EqualTo(TriState.Indeterminate));
            Assert.That(clsEnum<TriState>.TryParse("Indeterminate", true, ref ts), Is.True);
            Assert.That(clsEnum.TryParse("Disabled", ref ts), Is.True);
            Assert.That(ts, Is.EqualTo(TriState.Disabled));
            Assert.That(clsEnum.TryParse(null, ref ts), Is.False);
            Assert.That(ts, Is.EqualTo((TriState)0)); // ie. the default value
            ts = TriState.Disabled;
            Assert.That(clsEnum.TryParse("", ref ts), Is.False);
            Assert.That(ts, Is.EqualTo((TriState)0)); // ie. the default value
            ts = TriState.Disabled;
            Assert.That(clsEnum.TryParse(Environment.NewLine, ref ts), Is.False);
            Assert.That(ts, Is.EqualTo((TriState)0)); // ie. the default value
            ts = TriState.Disabled;
            Assert.That(clsEnum<TriState>.TryParse(null, ref ts), Is.False);
            Assert.That(ts, Is.EqualTo((TriState)0)); // ie. the default value

            Assert.That(clsEnum.Parse("fish", TriState.Disabled), Is.EqualTo(TriState.Disabled));
            Assert.That(clsEnum.Parse("fish", TriState.Indeterminate), Is.EqualTo(TriState.Indeterminate));
            Assert.That(clsEnum.Parse("enabled", TriState.Disabled), Is.EqualTo(TriState.Disabled));
            Assert.That(clsEnum.Parse("enabled", TriState.Indeterminate), Is.EqualTo(TriState.Indeterminate));
            Assert.That(clsEnum.Parse("Enabled", TriState.Disabled), Is.EqualTo(TriState.Enabled));
            Assert.That(clsEnum.Parse("Enabled", TriState.Indeterminate), Is.EqualTo(TriState.Enabled));
            Assert.That(clsEnum.Parse("fish", true, TriState.Disabled), Is.EqualTo(TriState.Disabled));
            Assert.That(clsEnum.Parse("fish", true, TriState.Indeterminate), Is.EqualTo(TriState.Indeterminate));
            Assert.That(clsEnum.Parse("enabled", true, TriState.Disabled), Is.EqualTo(TriState.Enabled));
            Assert.That(clsEnum.Parse("enabled", true, TriState.Indeterminate), Is.EqualTo(TriState.Enabled));
        }
    }
}

#endif
