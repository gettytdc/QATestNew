#if UNITTESTS

using System;
using BluePrism.Core.Utility;
using NUnit.Framework;

namespace BluePrism.Core.UnitTests.Utility
{
    public class MemberAccessHelperTests
    {
        [Test]
        public void ShouldProvideNamesOfMembers()
        {
            string propertyName = MemberAccessHelper.GetMemberName((TestClass x) => x.Property1);
            Assert.That(propertyName, Is.EqualTo("Property1"));
            
            string fieldName = MemberAccessHelper.GetMemberName((TestClass x) => x.Field1);
            Assert.That(fieldName, Is.EqualTo("Field1"));
        }

        [Test]
        public void ShouldThrowIfAccessingNestedMember()
        {
            Assert.Throws<ArgumentException>(() => MemberAccessHelper.GetMemberName((TestClass x) => x.Nested.Property1));
        }

        [Test]
        public void ShouldThrowIfAccessingOtherValue()
        {
            Assert.Throws<ArgumentException>(() => MemberAccessHelper.GetMemberName((TestClass x) => new TestClass().Property1));
        }


        [Test]
        public void ShouldThrowIfAccessingMethod()
        {
            Assert.Throws<ArgumentException>(() => MemberAccessHelper.GetMemberName((TestClass x) => x.Method1()));
        }

        protected class TestClass
        {
            public string Property1 { get; set; }

            public string Field1 { get; set; }

            public TestClass Nested { get; set; }

            public string Method1()
            {
                return "";
            }
        }
    }
}
#endif