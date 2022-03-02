#if UNITTESTS

using System.Drawing;
using NUnit.Framework;

namespace BluePrism.BPCoreLib.UnitTests
{
    [TestFixture]
    public class GridSchemaTests
    {
        [Test]
        public void TestGetCell()
        {
            var s = new GridSchema("10%,30,20%;10,10,10,10");
            Assert.That(s.GetCell(0, 0, new Size(120, 40)), Is.EqualTo(new Rectangle(0, 0, 30, 10)));
            Assert.That(s.GetCell(1, 2, new Size(120, 40)), Is.EqualTo(new Rectangle(30, 20, 30, 10)));
        }
    }
}

#endif
