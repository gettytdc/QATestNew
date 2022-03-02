using BluePrism.AutomateAppCore.Groups;
using BluePrism.AutomateProcessCore;
using BluePrism.BPCoreLib.Data;
using NUnit.Framework;
using System.Collections;

namespace AutomateAppCore.UnitTests.Groups
{
    [TestFixture]
    public class ObjectGroupMemberTests
    {
        [Test]
        public void TestSharableAndNotRetiredFilter_ShareableAndNotRetiredObject()
        {
            var predicate = ObjectGroupMember.ShareableAndNotRetired;
            var hashtable = new Hashtable
            {
                { "sharedObject", true },
                { "attributes", ProcessAttributes.None }
            };
            var mockDataProvider = new DictionaryDataProvider(hashtable);

            IGroupMember obj = new ObjectGroupMember(mockDataProvider);

            Assert.IsTrue(predicate(obj));
        }

        [Test]
        public void TestSharableAndNotRetiredFilter_ShareableAndRetiredObject()
        {
            var predicate = ObjectGroupMember.ShareableAndNotRetired;
            var hashtable = new Hashtable
            {
                { "sharedObject", true },
                { "attributes", ProcessAttributes.Retired }
            };
            var mockDataProvider = new DictionaryDataProvider(hashtable);

            IGroupMember obj = new ObjectGroupMember(mockDataProvider);

            Assert.IsFalse(predicate(obj));
        }

        [Test]
        public void TestSharableAndNotRetiredFilter_NotShareableAndNotRetiredObject()
        {
            var predicate = ObjectGroupMember.ShareableAndNotRetired;
            var hashtable = new Hashtable
            {
                { "sharedObject", false },
                { "attributes", ProcessAttributes.None }
            };
            var mockDataProvider = new DictionaryDataProvider(hashtable);

            IGroupMember obj = new ObjectGroupMember(mockDataProvider);

            Assert.IsFalse(predicate(obj));
        }

        [Test]
        public void TestSharableAndNotRetiredFilter_NotShareableAndRetiredObject()
        {
            var predicate = ObjectGroupMember.ShareableAndNotRetired;
            var hashtable = new Hashtable
            {
                { "sharedObject", false },
                { "attributes", ProcessAttributes.Retired }
            };
            var mockDataProvider = new DictionaryDataProvider(hashtable);

            IGroupMember obj = new ObjectGroupMember(mockDataProvider);

            Assert.IsFalse(predicate(obj));
        }
    }
}
