#if UNITTESTS
using System;
using NUnit.Framework;
using BluePrism.Scheduling;

namespace BPScheduler.UnitTests
{
    [TestFixture]
    public class TestDaySet
    {
        [Test]
        public void TestEmptySet()
        {
            DaySet set = new DaySet();
            // 4 tests - there's 3 constructors and a Clear() method being tested.
            for (int i = 0; ; i++)
            {
                Assert.That(set.IsEmpty(), Is.True, "i=" + i);
                Assert.That(set.IsFull(), Is.False, "i=" + i);
                Assert.That(set.ToInt(), Is.EqualTo(0), "i=" + i);
                Assert.That(set.Count, Is.EqualTo(0), "i=" + i);
                Assert.That(set.Contains(DayOfWeek.Monday), Is.False, "i=" + i);
                Assert.That(set.Contains(DayOfWeek.Tuesday), Is.False, "i=" + i);
                Assert.That(set.Contains(DayOfWeek.Wednesday), Is.False, "i=" + i);
                Assert.That(set.Contains(DayOfWeek.Thursday), Is.False, "i=" + i);
                Assert.That(set.Contains(DayOfWeek.Friday), Is.False, "i=" + i);
                Assert.That(set.Contains(DayOfWeek.Saturday), Is.False, "i=" + i);
                Assert.That(set.Contains(DayOfWeek.Sunday), Is.False, "i=" + i);

                // Handle the new test case...
                switch (i)
                {
                    case 0:
                        set.Clear();
                        continue;
                    case 1:
                        set = new DaySet(0);
                        continue;
                    case 2:
                        set = new DaySet(set);
                        continue;
                    case 3:
                        set = new DaySet(new DayOfWeek[0]);
                        continue;
                }
                // without break <label> - this ensures that each test case in the
                // 0..3 range is done 
                break;
            }

            // There should be no days here...
            foreach (DayOfWeek day in set)
            {
                Assert.Fail("Found a day: " + day + " in a supposedly empty set");
            }

            // Should be able to copy into an empty array
            try
            {
                set.CopyTo(new DayOfWeek[0], 0);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine(e.StackTrace);
                Assert.Fail("Copying an empty day set to an empty array failed");
            }
        }

        [Test]
        public void TestFullSet()
        {
            DaySet set = new DaySet(DaySet.ALL_DAYS);
            for (int i = 0; ; i++)
            {
                Assert.That(set.IsEmpty(), Is.False, "i=" + i);
                Assert.That(set.IsFull(), Is.True, "i=" + i);
                Assert.That(set.ToInt(), Is.EqualTo(DaySet.ALL_DAYS), "i=" + i);
                Assert.That(set.Count, Is.EqualTo(7), "i=" + i);
                Assert.That(set.Contains(DayOfWeek.Monday), Is.True, "i=" + i);
                Assert.That(set.Contains(DayOfWeek.Tuesday), Is.True, "i=" + i);
                Assert.That(set.Contains(DayOfWeek.Wednesday), Is.True, "i=" + i);
                Assert.That(set.Contains(DayOfWeek.Thursday), Is.True, "i=" + i);
                Assert.That(set.Contains(DayOfWeek.Friday), Is.True, "i=" + i);
                Assert.That(set.Contains(DayOfWeek.Saturday), Is.True, "i=" + i);
                Assert.That(set.Contains(DayOfWeek.Sunday), Is.True, "i=" + i);
                switch (i)
                {
                    case 0:
                        set = new DaySet(set);
                        continue;
                    case 1:
                        set = new DaySet();
                        set.Add(DayOfWeek.Monday);
                        set.Add(DayOfWeek.Tuesday);
                        set.Add(DayOfWeek.Wednesday);
                        set.Add(DayOfWeek.Thursday);
                        set.Add(DayOfWeek.Friday);
                        set.Add(DayOfWeek.Saturday);
                        set.Add(DayOfWeek.Sunday);
                        continue;
                    case 2:
                        set = new DaySet();
                        set.Set(DayOfWeek.Monday);
                        set.Set(DayOfWeek.Tuesday);
                        set.Set(DayOfWeek.Wednesday);
                        set.Set(DayOfWeek.Thursday);
                        set.Set(DayOfWeek.Friday);
                        set.Set(DayOfWeek.Saturday);
                        set.Set(DayOfWeek.Sunday);
                        continue;
                }
                break;
            }
        }

        [Test]
        public void TestAddingTwice()
        {
            DaySet set = new DaySet();
            // Adding the same thing twice has no effect on the contents...
            for (int i = 0; i < 2; i++)
            {
                set.Add(DayOfWeek.Monday);

                Assert.That(set.IsEmpty(), Is.False, "i=" + i);
                Assert.That(set.IsFull(), Is.False, "i=" + i);
                Assert.That(set.Count, Is.EqualTo(1), "i=" + i);
                Assert.That(set.Contains(DayOfWeek.Monday), Is.True, "i=" + i);
                Assert.That(set.Contains(DayOfWeek.Tuesday), Is.False, "i=" + i);
                Assert.That(set.Contains(DayOfWeek.Wednesday), Is.False, "i=" + i);
                Assert.That(set.Contains(DayOfWeek.Thursday), Is.False, "i=" + i);
                Assert.That(set.Contains(DayOfWeek.Friday), Is.False, "i=" + i);
                Assert.That(set.Contains(DayOfWeek.Saturday), Is.False, "i=" + i);
                Assert.That(set.Contains(DayOfWeek.Sunday), Is.False, "i=" + i);
                bool doneOnce = false;
                foreach (DayOfWeek day in set)
                {
                    if (doneOnce)
                    {
                        Assert.Fail("Returned >1 day in a Dayset with just Monday set: " + set +
                               " errored day: " + day);
                    }
                    doneOnce = true;
                    Assert.That(day, Is.EqualTo(DayOfWeek.Monday));
                }
                if (!doneOnce)
                {
                    Assert.Fail("Didn't enter into a DaySet with just Monday set at all: " + set);
                }
            }
        }

        [Test]
        public void TestSetTo()
        {
            DaySet one = new DaySet(DayOfWeek.Monday);
            DaySet two = new DaySet(DayOfWeek.Friday);
            one.SetTo(two);
            // The data originally on 'one' should be overwritten
            Assert.That(one.Contains(DayOfWeek.Monday), Is.False);
            Assert.That(one.Contains(DayOfWeek.Friday), Is.True);
            Assert.That(one, Is.EqualTo(two));
        }

        [Test]
        public void TestEnumeration()
        {
            DaySet set = DaySet.FullWeek;
            Assert.That(set.IsFull(), Is.True, "DaySet.FullWeek.IsFull() should be true");
            Assert.That(set, Contains.Item(DayOfWeek.Sunday));
            DaySet newSet = new DaySet();
            foreach (DayOfWeek day in set)
            {
                newSet.Add(day);
            }
            Assert.That(newSet.IsFull(), Is.True);
        }

    }
}

#endif