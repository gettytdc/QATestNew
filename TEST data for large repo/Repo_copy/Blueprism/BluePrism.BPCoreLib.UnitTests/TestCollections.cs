#if UNITTESTS

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using BluePrism.BPCoreLib.Collections;
using NUnit.Framework;

namespace BluePrism.BPCoreLib.UnitTests
{

    /// <summary>
    /// Overarching test fixture for the collections
    /// </summary>
    [TestFixture]
    public class TestCollections
    {
        /// <summary>
        /// The predicates used throughout the tests - especially for Filter tests
        /// </summary>
        private class Predicates
        {
            /// <summary>
            /// Predicate to test for an odd number
            /// </summary>
            /// <param name="i">The number to test</param>
            /// <returns>True if the given number was even</returns>
            public static bool IsOdd(int i) => i % 2 == 1;

            /// <summary>
            /// Predicate to test for an even number
            /// </summary>
            /// <param name="i">The number to test</param>
            /// <returns>True if the given number was even</returns>
            public static bool IsEven(int i) => !IsOdd(i);

            /// <summary>
            /// Unnecessarily correct test for a prime number, taken largely from the
            /// 'Primality test' wikipedia article
            /// </summary>
            /// <param name="i">The integer to test for primality</param>
            /// <returns>True if the given number was prime; False otherwise.</returns>
            public static bool IsPrime(int i)
            {
                if (i < 2)
                {
                    return false;
                }

                if (i < 4)
                {
                    return true; // Remove 2 or 3 from the tests..
                }

                // We only need to test for divisibility up to root(i)
                var root = (int)Math.Ceiling(Math.Sqrt(i));

                // Since integers can be expressed as 6k+i for some integer k and for
                // i = -1, 0, 1, 2, 3, 4; We can disregard 0, 2, 4 by checking
                // dividing by 2, we can disregard 3 by dividing by 3 which leaves
                // us i = -1, 1 meaning all primes must be able to be represented by 6k+/-i.
                // Thus check /2 and /3 first, then /(6k+/-1) up to (6k-1)<=sqrt(number)

                // Remove i=0,2,3,4 from the equation first.
                if (i % 2 == 0 || i % 3 == 0)
                {
                    return false;
                }

                var testBase = 6; // Start with k = 1
                while (testBase - 1 <= root)
                {
                    if (i % (testBase + 1) == 0)
                    {
                        return false;
                    }

                    if (i % (testBase - 1) == 0)
                    {
                        return false;
                    }

                    testBase += 6; // effectively k++
                }

                return true;
            }
        }

        /// <summary>
        /// Utility method to test the order of a collection.
        /// </summary>
        /// <typeparam name="T">The type of collection</typeparam>
        /// <param name="coll">The collection to test</param>
        /// <param name="elements">The elements in the order expected.</param>
        private void AssertOrder<T>(ICollection<T> coll, params T[] elements)
        {
            var i = 0;
            foreach (var element in coll)
            {
                Assert.ByVal(element, Is.EqualTo(elements[i]), "Order differs - expected element index {0} to be {1}",
                    i, elements[i]);
                i += 1;
            }
        }

        /// <summary>
        /// Method to ensure that the ByVal style of Assert is called - trying with a
        /// property was causing that property to be set, thus altering the actual data
        /// being tested as a side-effect of the test. Damn VB and its lack of ref
        /// keyword
        /// </summary>
        /// <param name="toTest">The value to test</param>
        /// <param name="expected">The expected value</param>
        private void AssertInt(int toTest, int expected) => Assert.ByVal(toTest, Is.EqualTo(expected));

        /// <summary>
        /// Tests the reverse comparer
        /// </summary>
        [Test]
        public void TestReverseComparer()
        {
            var revComp = new clsReverseComparer<int>();
            Assert.ByVal(revComp.Compare(1, 2), Is.GreaterThan(0));
            Assert.ByVal(revComp.Compare(123, 123), Is.EqualTo(0));
            Assert.ByVal(revComp.Compare(7, -1), Is.LessThan(0));
            var s = new SortedDictionary<int, string>(revComp)
            {
                [3] = "three",
                [4] = "four",
                [5] = "five",
                [1] = "one",
                [2] = "two",
                [6] = "six"
            };
            Assert.ByVal(s.Keys, Is.EqualTo(new[] {6, 5, 4, 3, 2, 1}));
        }

        /// <summary>
        /// Tests the order of the ordered set
        /// </summary>
        [Test]
        public void OrderedSetTest()
        {
            var os = new clsOrderedSet<int>();

            // Test opening Union's order - note that 4 is in there twice - should appear at the
            // point of first entry
            os.Union(new[] {1, 4, 6, 2, 3, 4, 7, 5});
            AssertOrder(os, 1, 4, 6, 2, 3, 7, 5);

            // The following modifications will append to the set (if they add any elements)
            // so any new elements should appear in the order they were provided after the
            // original elements in the set...
            // eg. {1, 5, 10}.Union({5, 2, 7}) => {1, 5, 10, 2, 7}
            // ... so 5 isn't added (position unchanged) and 2 and 7 were, in that order.
            // This rule follows through all appending operations

            os.Union(new[] {1, 9, 10});
            AssertOrder(os, 1, 4, 6, 2, 3, 7, 5, 9, 10);
            os.Subtract(new[] {6, 7});
            AssertOrder(os, 1, 4, 2, 3, 5, 9, 10);

            // Effectively mask out 1-3
            os.Intersect(new[] {10, 9, 8, 7, 6, 5, 4});
            AssertOrder(os, 4, 5, 9, 10);
            os.Difference(new[] {1, 2, 3, 5, 10});
            AssertOrder(os, 4, 9, 1, 2, 3);
        }

        /// <summary>
        /// Test the ordered dictionary
        /// </summary>
        [Test]
        public void OrderedDictionaryTest()
        {
            var od = new clsOrderedDictionary<int, string>
            {
                [0] = "zero",
                [1] = "one",
                [2] = "NOT two", // Cunning, eh?
                [3] = "three"
            };
            AssertOrder(od.Keys, 0, 1, 2, 3);
            AssertOrder(od.Values, "zero", "one", "NOT two", "three");

            // Test replace (same value) doesn't affect the order
            od[2] = "NOT two";
            AssertOrder(od.Keys, 0, 1, 2, 3);
            AssertOrder(od.Values, "zero", "one", "NOT two", "three");

            // Test replace (different value) doesn't affect the order
            od[2] = "two";
            AssertOrder(od.Keys, 0, 1, 2, 3);
            AssertOrder(od.Values, "zero", "one", "two", "three");

            // Test adding after enumeration
            od[5] = "five";
            AssertOrder(od.Keys, 0, 1, 2, 3, 5);
            AssertOrder(od.Values, "zero", "one", "two", "three", "five");

            // Test after inserting "in between" value (if sorting, this would drop between 3 and 5)
            od[4] = "four";
            AssertOrder(od.Keys, 0, 1, 2, 3, 5, 4);
            AssertOrder(od.Values, "zero", "one", "two", "three", "five", "four");

            // Test after removing non-boundary value
            od.Remove(5);
            AssertOrder(od.Keys, 0, 1, 2, 3, 4);
            AssertOrder(od.Values, "zero", "one", "two", "three", "four");

            // Test re-adding previously removed entry
            od[5] = "five";
            AssertOrder(od.Keys, 0, 1, 2, 3, 4, 5);
            AssertOrder(od.Values, "zero", "one", "two", "three", "four", "five");
            var od2 = new clsOrderedDictionary<string, int> {{"one", 1}, {"two", 2}, {"zero", 0}, {"three", 3}};
            AssertOrder(od2.Keys, "one", "two", "zero", "three");
            AssertOrder(od2.Values, 1, 2, 0, 3);
        }

        [Test]
        public void TestOrderedDictionaryAddTwice()
        {
            var od = new clsOrderedDictionary<string, int> {{"first", 1}};

            // We shouldn't be able to add the same key twice.
            try
            {
                od.Add("first", int.MinValue);
                Assert.Fail("Managed to add \"zero\" twice to the dictionary");
            }
            catch (ArgumentException)
            {
                // Correct
            }
            catch (Exception e)
            {
                Assert.Fail("Failed with the wrong exception: {0}", e);
            }

            // We should, however, be able to 'set' it twice
            od["first"] = int.MaxValue;
            Assert.ByVal(od["first"], Is.EqualTo(int.MaxValue));
        }

        [Test]
        public void TestOrderedDictionaryAddNull()
        {
            var od = new clsOrderedDictionary<string, int>();
            // Shouldn't be able to add null
            try
            {
                od.Add(null, int.MinValue);
                Assert.Fail("Managed to add a null key to the dictionary");
            }
            catch (ArgumentNullException)
            {
                // Correct
            }
            catch (Exception e)
            {
                Assert.Fail("Failed with the wrong exception: {0}", e);
            }

            try
            {
                od[null] = int.MinValue;
                Assert.Fail("Managed to set a null key in the dictionary");
            }
            catch (ArgumentNullException)
            {
                // Correct
            }
            catch (Exception e)
            {
                Assert.Fail("Failed with the wrong exception: {0}", e);
            }
        }

        [Test]
        public void TestCounterMap()
        {
            var cm = new clsCounterMap<Color>();
            cm[Color.Black] += 1;
            cm[Color.Red] += 1;
            cm[Color.Green] += 2;
            cm.Increment(Color.Black);

            // If we Assert.That(<property>, ...) it calls the 'ByRef' version
            // meaning that it sets the property after testing it. Hence we
            // need to put this into an integer variable first. We do this via AssertInt
            AssertInt(cm[Color.Black], 2);
            AssertInt(cm[Color.Red], 1);
            AssertInt(cm[Color.Green], 2);
            AssertInt(cm[Color.Blue], 0);
            foreach (var col in cm.Keys)
            {
                switch (col)
                {
                    // ok
                    case var @case when @case == Color.Black:
                    case var case1 when case1 == Color.Red:
                    case var case2 when case2 == Color.Green:
                    {
                        break;
                    }

                    default:
                    {
                        Assert.Fail("Found color: " + col.ToString());
                        break;
                    }
                }
            }

            cm.Clear();
            AssertInt(cm[Color.Black], 0);
            AssertInt(cm[Color.Red], 0);
            AssertInt(cm[Color.Green], 0);
            AssertInt(cm[Color.Blue], 0);
            foreach (var _ in cm.Keys)
            {
                Assert.Fail();
            }

            var sm = new clsCounterMap<string>(true);
            sm.Increment("b");
            sm.Increment("b");
            sm.Increment("c");
            AssertOrder(sm.Keys, "b", "c");
            sm.Increment("a");
            sm.Increment("c");
            sm.Increment("c");
            AssertOrder(sm.Keys, "a", "b", "c");
            AssertInt(sm["a"], 1);
            AssertInt(sm["b"], 2);
            AssertInt(sm["c"], 3);
            AssertOrder(sm.CounterOrderedMap.Keys, "c", "b", "a");
            sm.Remove("b");
            AssertInt(sm["a"], 1);
            AssertInt(sm["b"], 0);
            AssertInt(sm["c"], 3);
        }

        [Test]
        public void TestCollectionUtil_AreEquivalent()
        {
            var nums = new[] {1, 3, 5};
            // Check for various orders
            Assert.True(CollectionUtil.AreEquivalent(nums, new[] {3, 5, 1}));
            Assert.True(CollectionUtil.AreEquivalent(nums, new[] {1, 3, 5}));
            Assert.True(CollectionUtil.AreEquivalent(nums, new[] {5, 1, 3}));

            // Check multiple elements of same value mismatches...
            Assert.False(CollectionUtil.AreEquivalent(nums, new[] {1, 3, 5, 3}));
            Assert.False(CollectionUtil.AreEquivalent(new[] {1, 3, 5, 3}, nums));
            Assert.False(CollectionUtil.AreEquivalent(new[] {9, 9, 9}, new[] {9, 9, 9, 9}));
            Assert.False(CollectionUtil.AreEquivalent(new[] {9, 9, 9, 9}, new[] {9, 9, 9}));

            // ...and matches
            Assert.True(CollectionUtil.AreEquivalent(new[] {1, 3, 5, 3}, new[] {3, 3, 5, 1}));
            Assert.True(CollectionUtil.AreEquivalent(new[] {9, 9, 9}, new[] {9, 9, 9}));


            // Check that the right out ones are right out
            Assert.False(CollectionUtil.AreEquivalent(nums, new[] {7, 1, 3, 5}));
            Assert.False(CollectionUtil.AreEquivalent(nums, new[] {1, 3}));
            Assert.False(CollectionUtil.AreEquivalent(nums, new[] {2, 4, 6}));

            // And even further out
            Assert.False(CollectionUtil.AreEquivalent(nums, new[] {"1", "3", "5"}));

            // Null is equivalent to an empty collection
            Assert.False(CollectionUtil.AreEquivalent(nums, null));
            Assert.False(CollectionUtil.AreEquivalent(null, nums));
            Assert.True(CollectionUtil.AreEquivalent(new int[] { }, null));
            Assert.True(CollectionUtil.AreEquivalent(null, new int[] { }));
            Assert.True(CollectionUtil.AreEquivalent(new int[] { }, new int[] { }));
            Assert.True(CollectionUtil.AreEquivalent(null, null));

            // An empty collection is equivalent to an empty collection of a different
            // type, odd though that may seem
            Assert.True(CollectionUtil.AreEquivalent(new int[] { }, new string[] { }));

            // Check for null values
            Assert.False(CollectionUtil.AreEquivalent(new[] {null, "one"}, new[] {"one"}));
            Assert.False(CollectionUtil.AreEquivalent(new[] {"one"}, new[] {null, "one"}));
            Assert.True(CollectionUtil.AreEquivalent(new[] {"one", null}, new[] {null, "one"}));
            Assert.True(CollectionUtil.AreEquivalent(new string[] {null}, new string[] {null}));
        }

        [Test]
        public void TestCollectionUtil_Join()
        {
            Assert.ByVal(CollectionUtil.Join(null, ", "), Is.EqualTo(""));
            Assert.ByVal(CollectionUtil.Join("aeiou", "-"), Is.EqualTo("a-e-i-o-u"));
            var nums = new[] {1, 3, 5};
            Assert.ByVal(CollectionUtil.Join(nums, ";"), Is.EqualTo("1;3;5"));
            Assert.ByVal(CollectionUtil.Join(nums, null), Is.EqualTo("135"));
            // Let's get zen... arbitrary objects
            Assert.ByVal(CollectionUtil.Join(new object[] {"What", typeof(Is), this, '?'}, " "),
                Is.EqualTo("What NUnit.Framework.Is BluePrism.BPCoreLib.UnitTests.TestCollections ?"));
            var sb = new System.Text.StringBuilder("The first 5 primes (larger than 1) are: ");
            var lst = new List<int>();
            var i = 1;
            while (lst.Count < 5)
            {
                i += 1;
                if (Predicates.IsPrime(i))
                {
                    lst.Add(i);
                }
            }

            Assert.ByVal(CollectionUtil.JoinInto(lst, ", ", sb).ToString(),
                Is.EqualTo("The first 5 primes (larger than 1) are: 2, 3, 5, 7, 11"));
        }

        [Test]
        public void TestCollectionUtil_Last()
        {
            Assert.ByVal(CollectionUtil.Last(new int[] { }), Is.EqualTo(0));
            Assert.ByVal(CollectionUtil.Last(new[] {5, 3, 1}), Is.EqualTo(1));
            Assert.Null(CollectionUtil.Last(new string[] { }));
            Assert.ByVal(CollectionUtil.Last(new[] {"five", "three", "one"}), Is.EqualTo("one"));
            var os = new clsOrderedSet<string>();
            Assert.Null(CollectionUtil.Last(os));
            os.Add("five");
            os.Add("three");
            os.Add("one");
            Assert.ByVal(CollectionUtil.Last(os), Is.EqualTo("one"));
            var map = new SortedDictionary<int, string> {[5] = "five", [3] = "three", [1] = "one"};
            Assert.ByVal(CollectionUtil.Last(map.Keys), Is.EqualTo(5));
            map[13] = "thirteen";
            map[7] = "seven";
            Assert.ByVal(CollectionUtil.Last(map.Values), Is.EqualTo("thirteen"));
            map.Remove(13);
            Assert.ByVal(CollectionUtil.Last(map.Values), Is.EqualTo("seven"));
        }

        /// <summary>
        /// Object which holds a single member - a parent of the same type.
        /// Here to test the cloning of objects in <see cref="CollectionUtil.CloneInto"/>
        /// </summary>
        private class TestCloneable : ICloneable
        {
            public TestCloneable(string name) : this(name, null)
            {
            }

            public TestCloneable(string name, TestCloneable parent)
            {
                Name = name;
                Parent = parent;
            }

            public object Clone()
            {
                var copy = (TestCloneable)MemberwiseClone();
                if (Parent != null)
                {
                    copy.Parent = (TestCloneable)Parent.Clone();
                }

                return copy;
            }

            public override bool Equals(object o)
            {
                if (ReferenceEquals(o, this))
                {
                    return true;
                }

                var tc = o as TestCloneable;
                if (tc is null)
                {
                    return false;
                }

                return (Name ?? "") == (tc.Name ?? "") && Equals(Parent, tc.Parent);
            }

            public string Name { get; }

            public TestCloneable Parent { get; private set; }
        }

        /// <summary>
        /// Tests the <see cref="CollectionUtil.CloneInto"/> method
        /// </summary>
        [Test]
        public void TestCollectionUtil_Clone()
        {
            var tc1 = new TestCloneable("tc1");
            var tc2 = new TestCloneable("tc2", tc1);
            var tc3 = new TestCloneable("tc3", tc2);
            var inList = new List<TestCloneable>
            {
                null,
                tc1,
                tc2,
                tc3,
                tc3
            };
            var outList = new List<TestCloneable>();
            CollectionUtil.CloneInto(inList, outList);
            Assert.ByVal(outList[0], Is.Null);
            Assert.ByVal(inList[1], Is.Not.SameAs(outList[1]));
            Assert.ByVal(inList[2], Is.Not.SameAs(outList[2]));
            Assert.ByVal(inList[3], Is.Not.SameAs(outList[3]));
            Assert.ByVal(inList[4], Is.Not.SameAs(outList[4]));
            // It re-clones all objects, so the fact that in the inList
            // inList(3) is the same reference as inList(4) has no
            // effect on the outlist - not a bug, but potentially a
            // breaking change, so it's here in the tests
            Assert.ByVal(outList[3], Is.Not.SameAs(outList[4]));

            // They should be different objects, but they should have equivalent value
            Assert.ByVal(inList[1], Is.EqualTo(outList[1]));
            Assert.ByVal(inList[2], Is.EqualTo(outList[2]));
            Assert.ByVal(inList[3], Is.EqualTo(outList[3]));
            Assert.ByVal(inList[4], Is.EqualTo(outList[4]));

            // Same goes for this...
            Assert.ByVal(outList[3], Is.EqualTo(outList[4]));
        }

        [Test]
        public void TestCollectionUtil_Filter()
        {
            var values = CollectionUtil.ToCollection(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19,
                20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30);

            Assert.ByVal(CollectionUtil.Filter(values, Predicates.IsEven),
                Is.EquivalentTo(CollectionUtil.ToCollection(2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30)));

            Assert.ByVal(CollectionUtil.Filter(values, Predicates.IsOdd),
                Is.EquivalentTo(CollectionUtil.ToCollection(1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25, 27, 29)));

            Assert.ByVal(CollectionUtil.Filter(values, Predicates.IsPrime),
                Is.EquivalentTo(CollectionUtil.ToCollection(2, 3, 5, 7, 11, 13, 17, 19, 23, 29)));
        }

        [Test]
        public void TestCollectionUtil_FilterByKey()
        {
            var map = new Dictionary<int, int>
            {
                [1] = 1,
                [2] = 4,
                [3] = 9,
                [4] = 16,
                [5] = 25,
                [6] = 36,
                [7] = 49,
                [8] = 64,
                [9] = 81,
                [10] = 100,
                [11] = 121,
                [12] = 144
            };
            var evenKeys = new Dictionary<int, int>
            {
                [2] = 4,
                [4] = 16,
                [6] = 36,
                [8] = 64,
                [10] = 100,
                [12] = 144
            };
            var oddKeys = new Dictionary<int, int>
            {
                [1] = 1,
                [3] = 9,
                [5] = 25,
                [7] = 49,
                [9] = 81,
                [11] = 121
            };
            var primeKeys = new Dictionary<int, int>
            {
                [2] = 4,
                [3] = 9,
                [5] = 25,
                [7] = 49,
                [11] = 121
            };
            Assert.ByVal(CollectionUtil.FilterByKey(map, Predicates.IsEven), Is.EquivalentTo(evenKeys));
            Assert.ByVal(CollectionUtil.FilterByKey(map, Predicates.IsOdd), Is.EquivalentTo(oddKeys));
            Assert.ByVal(CollectionUtil.FilterByKey(map, Predicates.IsPrime), Is.EquivalentTo(primeKeys));
        }

        [Test]
        public void TestCollectionUtil_FilterByValue()
        {
            var map = new Dictionary<string, int>
            {
                ["one"] = 1,
                ["two"] = 4,
                ["three"] = 9,
                ["four"] = 16,
                ["five"] = 25,
                ["six"] = 36,
                ["seven"] = 49,
                ["eight"] = 64,
                ["nine"] = 81,
                ["ten"] = 100,
                ["eleven"] = 121,
                ["twelve"] = 144
            };

            Assert.ByVal(CollectionUtil.FilterByValue(map, Predicates.IsEven).Keys,
                Is.EquivalentTo(CollectionUtil.ToCollection("two", "four", "six", "eight", "ten", "twelve")));

            Assert.ByVal(CollectionUtil.FilterByValue(map, Predicates.IsOdd).Keys,
                Is.EquivalentTo(CollectionUtil.ToCollection("one", "three", "five", "seven", "nine", "eleven")));

            Assert.ByVal(CollectionUtil.FilterByValue(map, Predicates.IsPrime).Keys, Is.Empty);
        }

        [Test]
        public void TestCollectionUtil_AreEqual()
        {
            Assert.True(CollectionUtil.AreEqual<object>(null, null));
            Assert.True(CollectionUtil.AreEqual(null, new int[] { }));
            Assert.True(CollectionUtil.AreEqual(new int[] { }, null));
            Assert.True(CollectionUtil.AreEqual(new int[] { }, new int[] { }));
            Assert.True(CollectionUtil.AreEqual(new[] {1, 2, 5}, new[] {1, 2, 5}));
            Assert.False(CollectionUtil.AreEqual(new[] {1, 2, 5}, new[] {1, 5, 2}));
            Assert.False(CollectionUtil.AreEqual(new[] {1, 2, 5}, new int[] { }));
            Assert.False(CollectionUtil.AreEqual(null, new[] {1, 5, 2}));
            Assert.False(CollectionUtil.AreEqual(new[] {1, 2, 5}, null));
            Assert.True(CollectionUtil.AreEqual(GetReadOnly.ICollectionFrom("one", "two", "five"),
                new[] {"one", "two", "five"}));

            Assert.False(CollectionUtil.AreEqual(GetReadOnly.ICollectionFrom("one", "two", "five"),
                new[] {"one", null, "five"}));

            Assert.False(CollectionUtil.AreEqual(GetReadOnly.ICollectionFrom("one", "two", "five"),
                new[] {"five", "two", "five"}));

            Assert.False(CollectionUtil.AreEqual(GetReadOnly.ICollectionFrom<object>("one", "two", "five"),
                GetReadOnly.ICollectionFrom(1, 2, (object)5)));

            Assert.False(CollectionUtil.AreEqual(GetReadOnly.ICollectionFrom("one", "two", "five"),
                GetSingleton.ICollection("one")));
        }

        [Test]
        public void TestCollectionUtil_Flatten()
        {
            var a = new[]
            {
                new[] {new[] {1, 2}, new[] {3}, new int[] { }}, new[] {new[] {4, 5, 6}}, new[] {new int[] { }},
                new[] {new[] {7}, new[] {8}}, new[] {new[] {9}}
            };

            // The above normalised once.
            var b = new[]
            {
                new[] {1, 2}, new[] {3}, new int[] { }, new[] {4, 5, 6}, new int[] { }, new[] {7}, new[] {8},
                new[] {9}
            };

            // And the above, normalised again
            var c = new[] {1, 2, 3, 4, 5, 6, 7, 8, 9};

            // a[0] normalised
            var d = new[] {1, 2, 3};
            Assert.ByVal(CollectionUtil.Flatten(a), Is.EquivalentTo(b));
            Assert.ByVal(CollectionUtil.Flatten(b), Is.EquivalentTo(c));
            Assert.ByVal(CollectionUtil.Flatten(a[0]), Is.EquivalentTo(d));
            var map = new Dictionary<string, ICollection<int>>
            {
                ["ending-one"] = new[] {1, 11, 21},
                ["even"] = new[] {2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24},
                ["prime"] = new[] {2, 3, 5, 7, 11, 13, 17, 19, 23},
                ["teenies"] = new[] {13, 14, 15, 16, 17, 18, 19},
                ["leftovers"] = new[] {9, 21}
            };

            Assert.ByVal(CollectionUtil.FlattenInto(map.Values, new clsOrderedSet<int>()),
                Is.EquivalentTo(CollectionUtil.ToCollection(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17,
                    18, 19, 20, 21, 22, 23, 24)));

            try
            {
                CollectionUtil.Flatten<object>(null);
                Assert.Fail("CollectionUtil.Flatten(null) should throw ArgumentNull");
            }
            catch (ArgumentNullException)
            {
                // Correct
            }

            try
            {
                CollectionUtil.FlattenInto(new[] {new string[] { }}, null);
                Assert.Fail("CollectionUtil.FlattenInto(obj,null) should throw ArgumentNull");
            }
            catch (ArgumentNullException)
            {
                // Tick.
            }

            Assert.ByVal(CollectionUtil.Flatten(new[] {new string[] { }}), Is.Empty);
        }

        [Test]
        public void TestCollectionUtil_ContainsAll()
        {
            var master = new List<string>(new[] {"one", "two", "three", "four", "five", "", null, "six", "seven"});

            // A null/empty subset is always true
            Assert.True(CollectionUtil.ContainsAll(master, null));
            Assert.True(CollectionUtil.ContainsAll(master, new string[] { }));
            Assert.True(CollectionUtil.ContainsAll(new string[] { }, null));
            Assert.True(CollectionUtil.ContainsAll(new string[] { }, new string[] { }));

            // A null master should always fail
            try
            {
                CollectionUtil.ContainsAll(null, new string[] { });
                Assert.Fail("ContainsAll(null,?) should fail with ArgumentNullException");
            }
            catch (ArgumentNullException)
            {
                // Correct behaviour... carry on
            }

            try
            {
                CollectionUtil.ContainsAll<string>(null, null);
                Assert.Fail("ContainsAll(null,?) should fail with ArgumentNullException");
            }
            catch (ArgumentNullException)
            {
                // Correct behaviour... carry on
            }

            Assert.True(CollectionUtil.ContainsAll(master, new[] {"one"}));
            Assert.True(CollectionUtil.ContainsAll(master, new[] {"two"}));
            Assert.True(CollectionUtil.ContainsAll(master, new[] {"three"}));
            Assert.True(CollectionUtil.ContainsAll(master, new[] {"four"}));
            Assert.True(CollectionUtil.ContainsAll(master, new[] {"five"}));
            Assert.True(CollectionUtil.ContainsAll(master, new[] {"six"}));
            Assert.True(CollectionUtil.ContainsAll(master, new[] {"seven"}));
            Assert.True(CollectionUtil.ContainsAll(master, new[] {""}));
            Assert.True(CollectionUtil.ContainsAll(master, new string[] {null}));
            Assert.False(CollectionUtil.ContainsAll(master, new[] {"eight"}));
            Assert.False(CollectionUtil.ContainsAll(master, new[] {" "}));
            Assert.True(CollectionUtil.ContainsAll(master, new[] {"one", "two"}));

            Assert.True(CollectionUtil.ContainsAll(master, new[] {"one", "two", "three"}));

            Assert.True(CollectionUtil.ContainsAll(master, new[] {"one", "two", "three", "four"}));

            Assert.True(CollectionUtil.ContainsAll(master, new[] {"one", "two", "three", "four", "five"}));

            Assert.True(CollectionUtil.ContainsAll(master, new[] {"one", "two", "three", "four", "five", ""}));

            Assert.True(CollectionUtil.ContainsAll(master, new[] {"one", "two", "three", "four", "five", ""}));

            Assert.True(CollectionUtil.ContainsAll(master, new[] {"one", "two", "three", "four", "five", "", null}));

            Assert.True(CollectionUtil.ContainsAll(master,
                new[] {"one", "two", "three", "four", "five", "", null, "six"}));

            Assert.True(CollectionUtil.ContainsAll(master,
                new[] {"one", "two", "three", "four", "five", "", null, "six", "seven"}));

            Assert.True(CollectionUtil.ContainsAll(master,
                new[] {"", "three", "five", "one", "four", "two", "seven", "six", null}));

            Assert.False(CollectionUtil.ContainsAll(master,
                new[] {"one", "two", "three", "four", "five", "", null, "six", "seven", "8"}));

            var map = new Dictionary<string, string> {["one"] = "two"};
            Assert.True(CollectionUtil.ContainsAll(master, map.Keys));
            Assert.True(CollectionUtil.ContainsAll(master, map.Values));
        }

        [Test]
        public void TestDefaultValueDictionary()
        {
            clsDefaultValueDictionary<string, string> concreteMap;
            concreteMap = new clsDefaultValueDictionary<string, string> {["Yes"] = "True"};
            Assert.ByVal(concreteMap["Yes"], Is.EqualTo("True"));
            Assert.ByVal(concreteMap["No"], Is.Null);
            Assert.ByVal(concreteMap.Values, Is.EquivalentTo(new[] {"True"}));
            concreteMap = new clsDefaultValueDictionary<string, string>("False") {["Yes"] = "True"};
            Assert.ByVal(concreteMap["Yes"], Is.EqualTo("True"));
            Assert.ByVal(concreteMap["No"], Is.EqualTo("False"));
            Assert.True(concreteMap.ContainsKey("Yes"));
            Assert.True(concreteMap.Contains(new KeyValuePair<string, string>("Yes", "True")));
            Assert.False(concreteMap.ContainsKey("No"));
            Assert.False(concreteMap.Contains(new KeyValuePair<string, string>("Yes", "False")));
            clsDefaultValueDictionary<int, string> intMap;
            intMap = new clsDefaultValueDictionary<int, string>(new SortedDictionary<int, string>(), "right out")
            {
                [3] = "shalt thou count",
                [4] = "thou shalt not count",
                [2] = "neither count thou, excepting that thou then proceed to 3"
            };
            Assert.ByVal(intMap.Count, Is.EqualTo(3));
            Assert.ByVal(intMap[3], Is.EqualTo("shalt thou count"));
            Assert.ByVal(intMap[4], Is.EqualTo("thou shalt not count"));
            Assert.ByVal(intMap[2], Is.EqualTo("neither count thou, excepting that thou then proceed to 3"));
            Assert.ByVal(intMap[5], Is.EqualTo("right out"));
            Assert.ByVal(intMap.Keys, Is.EqualTo(new[] {2, 3, 4}).AsCollection);
            string argvalue = null;
            Assert.True(intMap.TryGetValue(2, ref argvalue));
            string argvalue1 = null;
            Assert.True(intMap.TryGetValue(3, ref argvalue1));
            string argvalue2 = null;
            Assert.True(intMap.TryGetValue(4, ref argvalue2));
            string argvalue3 = null;
            Assert.False(intMap.TryGetValue(5, ref argvalue3));

            // Just one quick test to ensure it works as an interface
            IDictionary<string, string> iMap;
            iMap = new clsDefaultValueDictionary<string, string> {["Yes"] = "True"};
            Assert.ByVal(iMap["Yes"], Is.EqualTo("True"));
            Assert.ByVal(iMap["No"], Is.Null);
            IDictionary<Point, Point> iValueMap;
            iValueMap = new clsDefaultValueDictionary<Point, Point>
            {
                [Point.Empty] = new Point(5, 5), [new Point(2, 7)] = new Point(1, 2)
            };
            Assert.ByVal(iValueMap[Point.Empty], Is.EqualTo(new Point(5, 5)));
            Assert.ByVal(iValueMap[new Point(2, 7)], Is.EqualTo(new Point(1, 2)));
            Assert.ByVal(iValueMap[new Point(1, 2)], Is.EqualTo(Point.Empty));
        }

        [Test]
        public void TestAutoSortedList()
        {
            var sl = new clsAutoSortedList<int>();
            Assert.ByVal(sl.IsReadOnly, Is.False);
            Assert.ByVal(sl.Count, Is.EqualTo(0));
            sl.Add(1);
            sl.Add(2);
            sl.Add(3);
            sl.Add(10);
            Assert.ByVal(sl.Count, Is.EqualTo(4));
            Assert.ByVal(sl, Is.EqualTo(new[] {1, 2, 3, 10}).AsCollection);
            sl.Add(5);
            Assert.ByVal(sl.Count, Is.EqualTo(5));
            Assert.ByVal(sl, Is.EqualTo(new[] {1, 2, 3, 5, 10}).AsCollection);
            foreach (var i in new[] {1, 2, 3, 5, 7, 10})
            {
                if (i == 7) // The deliberate mistake
                {
                    Assert.ByVal(sl.Contains(i), Is.False);
                }
                else
                {
                    Assert.ByVal(sl.Contains(i), Is.True);
                }
            }

            Assert.ByVal(sl.Remove(5), Is.True);
            Assert.ByVal(sl.Remove(5), Is.False);
            Assert.ByVal(sl.Contains(5), Is.False);
            var index = 0;
            var testArr = new[] {1, 2, 3, 10};
            foreach (var i in sl)
            {
                Assert.ByVal(i, Is.EqualTo(testArr[index]));
                index += 1;
            }

            for (int i = 0, loopTo = sl.Count - 1; i <= loopTo; i++)
            {
                var val = sl[i];
                Assert.ByVal(val, Is.EqualTo(testArr[i]));
            }

            sl.RemoveAt(2);
            Assert.ByVal(sl.Count, Is.EqualTo(3));
            Assert.ByVal(sl, Is.EqualTo(new[] {1, 2, 10}).AsCollection);
            sl.Clear();
            Assert.ByVal(sl.Count, Is.EqualTo(0));
            Assert.ByVal(sl.Contains(1), Is.False);
            var testArr2 = new[] {2, 6, 12};
            sl = new clsAutoSortedList<int>(testArr); // ie. {1, 2, 3, 10}
            sl.AddRange(testArr2);
            Assert.ByVal(sl.Count, Is.EqualTo(testArr.Length + testArr2.Length));
            Assert.ByVal(sl, Is.EqualTo(new[] {1, 2, 2, 3, 6, 10, 12}).AsCollection);
            Assert.ByVal(sl.Remove(2), Is.True);
            Assert.ByVal(sl.Count, Is.EqualTo((testArr.Length + testArr2.Length) - 1));
            Assert.ByVal(sl.Contains(2), Is.True);
            sl.Add(2);
            Assert.ByVal(sl.Count, Is.EqualTo(testArr.Length + testArr2.Length));
            Assert.ByVal(sl.Contains(2), Is.True);

            // Lets try a reverse sorted list - again 'testArr' is {1, 2, 3, 10}
            sl = new clsAutoSortedList<int>(new ReverseComparer<int>(Comparer<int>.Default), testArr);
            Assert.That(sl, Is.EqualTo(new[] {10, 3, 2, 1}));
            sl.AddRange(testArr2); // {2, 6, 12}
            Assert.ByVal(sl.Count, Is.EqualTo(testArr.Length + testArr2.Length));
            Assert.ByVal(sl, Is.EqualTo(new[] {12, 10, 6, 3, 2, 2, 1}).AsCollection);
            sl.Add(15);
            Assert.ByVal(sl, Is.EqualTo(new[] {15, 12, 10, 6, 3, 2, 2, 1}).AsCollection);
            sl.Add(1);
            Assert.ByVal(sl, Is.EqualTo(new[] {15, 12, 10, 6, 3, 2, 2, 1, 1}).AsCollection);
            sl.Add(0);
            Assert.ByVal(sl, Is.EqualTo(new[] {15, 12, 10, 6, 3, 2, 2, 1, 1, 0}).AsCollection);

            // Finally some strings
            var ssl = new clsAutoSortedList<string>();
            ssl.AddRange(new[] {null, "01", "02", "3", "04", null, "15"});
            Assert.ByVal(ssl, Is.EqualTo(new[] {null, null, "01", "02", "04", "15", "3"}).AsCollection);
            Assert.ByVal(ssl.Remove(null), Is.True);
            Assert.ByVal(ssl.Remove(null), Is.True);
            Assert.ByVal(ssl.Remove(null), Is.False);
            Assert.ByVal(ssl, Is.EqualTo(new[] {"01", "02", "04", "15", "3"}).AsCollection);
        }

        /// <summary>
        /// Class which implements the inverse of a given comparer.
        /// </summary>
        private class ReverseComparer<T> : IComparer<T>
        {
            private readonly IComparer<T> _comp;

            public ReverseComparer(IComparer<T> comp) => _comp = comp;

            public int Compare(T x, T y) => -_comp.Compare(x, y);
        }

        [Test]
        public void TestCollectionDictionary()
        {
            // Ugly though it may be, you can mix and match the value collection type
            // (though obviously if you try to remove from a fixed size collection,
            // you will get a runtime error)
            var factors = new clsCollectionDictionary<int, int>
            {
                [1] = GetSingleton.ICollection(1),
                [2] = new[] {1, 2},
                [3] = new LinkedList<int>(new[] {1, 3}),
                [4] = new List<int>(new[] {1, 2, 4}),
                [5] = new[] {1, 5}
            };
            factors.Add(6, 1);
            factors.Add(6, 2);
            factors.Add(6, 3);
            factors.Add(6, 6);
            foreach (var num in factors.Keys)
            {
                switch (num)
                {
                    case 1:
                    {
                        Assert.ByVal(factors[num], Is.EquivalentTo(new[] {1}));
                        break;
                    }

                    case 2:
                    {
                        Assert.ByVal(factors[num], Is.EquivalentTo(new[] {1, 2}));
                        break;
                    }

                    case 3:
                    {
                        Assert.ByVal(factors[num], Is.EquivalentTo(new[] {1, 3}));
                        break;
                    }

                    case 4:
                    {
                        Assert.ByVal(factors[num], Is.EquivalentTo(new[] {1, 2, 4}));
                        break;
                    }

                    case 5:
                    {
                        Assert.ByVal(factors[num], Is.EquivalentTo(new[] {1, 5}));
                        break;
                    }

                    case 6:
                    {
                        Assert.ByVal(factors[num], Is.EquivalentTo(new[] {1, 2, 3, 6}));
                        break;
                    }
                }
            }

            // Remove things that aren't there
            Assert.ByVal(factors.Remove(7, 1), Is.False);
            Assert.ByVal(factors.Remove(4, 3), Is.False);
            Assert.ByVal(factors.Remove(6, 4), Is.False);
            // And something that is
            Assert.ByVal(factors.Remove(6, 3), Is.True);

            // We should still have the same number of entries
            Assert.ByVal(factors.Count, Is.EqualTo(6));

            // That last 'Remove' should have removed 3 from the '6' factors
            Assert.ByVal(factors[6], Is.EquivalentTo(new[] {1, 2, 6}));

            // Add it back again, using a different method
            factors[6].Add(3);
            Assert.ByVal(factors[6], Is.EquivalentTo(new[] {1, 2, 3, 6}));
            var synonyms =
                new clsCollectionDictionary<string, string>(clsCollectionDictionary<string, string>.CreateSet)
                {
                    {"David", "David"},
                    {"David", "Dave"},
                    {"David", "Dai"},
                    {"Stuart", "Stuart"},
                    {"Stuart", "Stu"},
                    {"Giles", "Giles"}
                };
            foreach (var key in synonyms.Keys)
            {
                switch (key ?? "")
                {
                    case "David":
                    {
                        Assert.ByVal(synonyms[key ?? throw new InvalidOperationException()],
                            Is.EquivalentTo(new[] {"David", "Dave", "Dai"}));
                        break;
                    }

                    case "Stuart":
                    {
                        Assert.ByVal(synonyms[key ?? throw new InvalidOperationException()],
                            Is.EquivalentTo(new[] {"Stuart", "Stu"}));
                        break;
                    }

                    case "Giles":
                    {
                        Assert.ByVal(synonyms[key ?? throw new InvalidOperationException()],
                            Is.EquivalentTo(new[] {"Giles"}));
                        break;
                    }
                }
            }

            synonyms["Stuart"].Add("Stuart");
            // Should have no effect
            Assert.ByVal(synonyms["Stuart"], Is.EquivalentTo(new[] {"Stuart", "Stu"}));
            Assert.ByVal(synonyms["Stuart"], Is.TypeOf(typeof(clsSet<string>)));

            // Copy the values into a new collection dictionary which uses the default
            // mechanism (list) for storing its values
            var synList = new clsCollectionDictionary<string, string>(synonyms);

            // Since it's a list, it should now allow duplicates
            synList["Stuart"].Add("Stuart");
            Assert.ByVal(synList["Stuart"], Is.EquivalentTo(new[] {"Stuart", "Stu", "Stuart"}));
        }

        private ICollection<T> Collate<T>(IEnumerator<T> enu)
        {
            var lst = new List<T>();
            while (enu.MoveNext())
            {
                lst.Add(enu.Current);
            }

            return lst;
        }

        [Test]
        public void TestFilteringEnumerator()
        {
            var lst = new List<int>()
            {
                1,
                2,
                3,
                4,
                5,
                6,
                7,
                8,
                9,
                10,
                11,
                12,
                13,
                14,
                15,
                16,
                17,
                18,
                19,
                20
            };
            Assert.That(Collate(new FilteringEnumerator<int>(lst.GetEnumerator(), Predicates.IsOdd)),
                Is.EqualTo(new[] {1, 3, 5, 7, 9, 11, 13, 15, 17, 19}));
            Assert.That(Collate(new FilteringEnumerator<int>(lst.GetEnumerator(), Predicates.IsEven)),
                Is.EqualTo(new[] {2, 4, 6, 8, 10, 12, 14, 16, 18, 20}));
            Assert.That(new FilteringEnumerator<int>(lst.GetEnumerator(), i => false).MoveNext(), Is.False);
        }

        [Test]
        public void GivenCollectionPassedToBatch_ThenCollectionWithBatchedElementsReturned()
        {
            var collection = new List<int>()
            {
                1,
                2,
                3,
                4,
                5,
                6,
                7,
                8,
                9,
                10,
                11
            };
            var maxNumberOfBatches = 2;
            var expectedNumberOfBatches = 6;
            var batchedCollection = collection.Batch(maxNumberOfBatches);
            Assert.AreEqual(expectedNumberOfBatches, batchedCollection.Count());
        }

        [Test]
        public void GivenCollectionPassedToBatch_WhenMaxNumberOfBatchesIsZero_ThenReturnResultsInASingleBatch()
        {
            var collection = new List<int>()
            {
                1,
                2,
                3,
                4,
                5,
                6,
                7,
                8,
                9,
                10,
                11
            };
            var maxNumberOfBatches = 0;
            var expectedNumberOfBatches = 1;
            var batchedCollection = collection.Batch(maxNumberOfBatches);
            Assert.AreEqual(expectedNumberOfBatches, batchedCollection.Count());
        }
    }
}

#endif
