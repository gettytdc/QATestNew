#if UNITTESTS

using System;
using System.Collections.Generic;
using System.Diagnostics;
using BluePrism.BPCoreLib.Collections;
using NUnit.Framework;

namespace BluePrism.BPCoreLib.UnitTests
{
    /// Project  : BPCoreLib
    /// Class    : TestCovariantCollection
    /// <summary>
    /// Test class for the clsCovariantCollection and its sibling, the
    /// clsCovariantEnumerator.
    /// </summary>
    [TestFixture]
    public class TestCovariantCollection
    {
        /// <summary>
        /// Test the collection.
        /// </summary>
        [Test]
        public void TestCollection()
        {
            // create a list of strings, wrap into a collection of objects.
            var strList = new List<string>
            {
                "This",
                "Is",
                "A",
                "Test"
            };
            ICollection<object> oColl = new clsCovariantCollection<object, string>(strList);

            // check that the objects returned from the enumerator match those
            // in the original list.
            var index = 0;
            foreach (var obj in oColl)
            {
                Assert.That(obj is string);
                Assert.That(obj, Is.EqualTo(strList[index]));
                Debug.Print(obj.ToString());
                index += 1;
            }

            // Test ToArray() on a String array and an Object array.

            // Copy to the middle of the array... ensure that the
            // items match those in the initial list.
            var objArr = new object[17];
            oColl.CopyTo(objArr, 5);
            for (int i = 0, loopTo = objArr.Length - 1; i <= loopTo; i++)
            {
                var listInd = i - 5;
                if (listInd >= 0 && listInd < strList.Count)
                {
                    Assert.That(strList[listInd], Is.EqualTo(objArr[i]));
                }
            }

            // Again, copy to the middle of the array.
            // use an array of type string this time.
            var strArr = new object[17];
            oColl.CopyTo(strArr, 5);
            for (int i = 0, loopTo1 = strArr.Length - 1; i <= loopTo1; i++)
            {
                var listInd = i - 5;
                if (listInd >= 0 && listInd < strList.Count)
                {
                    Assert.That(strList[listInd], Is.EqualTo(strArr[i]));
                }
            }

            // Check that adding to the collection is mirrored to the original list.
            oColl.Add("And");
            oColl.Add("This");
            oColl.Add("Is");
            oColl.Add("Another");
            Assert.That(oColl.Contains("Another"));
            Assert.That(strList.Contains("Another"));
            try
            {
                oColl.Add(new object());
                Assert.Fail("Covariant collection allowed a non-String object to be added");
            }
            catch (InvalidCastException)
            {
                // Exactly what it should do
            }
        }
    }
}

#endif
