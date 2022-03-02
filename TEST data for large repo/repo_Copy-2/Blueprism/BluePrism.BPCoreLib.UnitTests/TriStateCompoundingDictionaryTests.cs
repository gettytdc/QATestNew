#if UNITTESTS

using BluePrism.BPCoreLib.Collections;
using NUnit.Framework;

namespace BluePrism.BPCoreLib.UnitTests
{
    [TestFixture]
    public class TriStateCompoundingDictionaryTests
    {
        [Test]
        public void TestAll()
        {
            var d = new clsTriStateCompoundingDictionary<string>();
            d["one"] = TriState.Enabled;
            d["one"] = TriState.Disabled;
            Assert.That(d["one"], Is.EqualTo(TriState.Indeterminate));
            Assert.False(d.IsEnabled("one"));
            Assert.False(d.IsDisabled("one"));
            Assert.That(d.Count, Is.EqualTo(1));
            d.Clear();
            d.Add("one", TriState.Enabled);
            d.Add("one", TriState.Disabled);
            Assert.That(d["one"], Is.EqualTo(TriState.Indeterminate));
            Assert.False(d.IsEnabled("one"));
            Assert.False(d.IsDisabled("one"));
            Assert.That(d.Count, Is.EqualTo(1));
            d.Clear();
            d["one"] = TriState.Enabled;
            Assert.That(d["one"], Is.EqualTo(TriState.Enabled));
            Assert.True(d.IsEnabled("one"));
            Assert.False(d.IsDisabled("one"));
            Assert.That(d.Count, Is.EqualTo(1));
            d.Clear();
            d.Add("one", true);
            Assert.That(d["one"], Is.EqualTo(TriState.Enabled));
            Assert.True(d.IsEnabled("one"));
            Assert.False(d.IsDisabled("one"));
            Assert.That(d.Count, Is.EqualTo(1));
            d.Clear();
            d.Add("one", true);
            d.Add("one", false);
            Assert.That(d["one"], Is.EqualTo(TriState.Indeterminate));
            Assert.False(d.IsEnabled("one"));
            Assert.False(d.IsDisabled("one"));
            Assert.That(d.Count, Is.EqualTo(1));
            d.Clear();
            d["one"] = TriState.Enabled;
            d["one"] = TriState.Disabled;
            d.Remove("one");
            d["one"] = TriState.Enabled;
            Assert.That(d["one"], Is.EqualTo(TriState.Enabled));
            Assert.True(d.IsEnabled("one"));
            Assert.False(d.IsDisabled("one"));
            Assert.That(d.Count, Is.EqualTo(1));
            d.Clear();
            d["one"] = TriState.Enabled;
            d["two"] = TriState.Disabled;
            d["three"] = TriState.Enabled;
            d["one"] = TriState.Disabled;
            d["three"] = TriState.Indeterminate;
            d["one"] = TriState.Enabled;
            d["three"] = TriState.Enabled;
            Assert.That(d["one"], Is.EqualTo(TriState.Indeterminate));
            Assert.That(d["two"], Is.EqualTo(TriState.Disabled));
            Assert.That(d["three"], Is.EqualTo(TriState.Indeterminate));
            Assert.That(d.Values.Count, Is.EqualTo(3));
            var counterMap = new clsCounterMap<TriState>();

            foreach (var ts in d.Values)
            {
                counterMap[ts] += 1;
            }

            Assert.That(counterMap[TriState.Enabled], Is.EqualTo(0));
            Assert.That(counterMap[TriState.Disabled], Is.EqualTo(1));
            Assert.That(counterMap[TriState.Indeterminate], Is.EqualTo(2));
        }
    }
}

#endif
