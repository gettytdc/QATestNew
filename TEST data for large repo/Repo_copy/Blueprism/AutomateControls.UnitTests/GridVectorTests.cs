using NUnit.Framework;
using BluePrism.BPCoreLib;

namespace AutomateControls.UnitTests
{
    [TestFixture]
    public class GridVectorTests
    {
        [Test]
        public void TestGetAbsoluteSizes()
        {
            Assert.That(GridVector.GetAbsolutes(new GridVector[] { }, 100),
                Is.EqualTo(new int[] { }));

            Assert.That(GridVector.GetAbsolutes(new GridVector[] { new GridRow() }, 100),
                Is.EqualTo(new int[] { 100 }));

            Assert.That(GridVector.GetAbsolutes(new GridVector[] {
                new GridRow(), new GridRow() }, 100),
                Is.EqualTo(new int[] { 50, 50 }));

            Assert.That(GridVector.GetAbsolutes(new GridVector[] {
                new GridRow(), new GridRow(VectorSizeType.Absolute, 40),new GridRow() }, 100),
                Is.EqualTo(new int[] { 30, 40, 30 }));

            Assert.That(GridVector.GetAbsolutes(new GridVector[] {
                new GridRow(), new GridRow(VectorSizeType.Absolute, 40),new GridRow() }, 100),
                Is.EqualTo(new int[] { 30, 40, 30 }));

            Assert.That(GridVector.GetAbsolutes(new GridVector[] {
                new GridRow(VectorSizeType.Proportional, 20),
                new GridRow(VectorSizeType.Absolute, 50),
                new GridRow(VectorSizeType.Proportional, 30) }, 150),
                Is.EqualTo(new int[] { 40, 50, 60 }));

            Assert.That(GridVector.GetAbsolutes(new GridVector[] {
                new GridRow(VectorSizeType.Proportional, 33),
                new GridRow(VectorSizeType.Proportional, 33),
                new GridRow(VectorSizeType.Proportional, 33) }, 99),
                Is.EqualTo(new int[] { 33, 33, 33 }));

            // Should extend the last entry to fill the space
            Assert.That(GridVector.GetAbsolutes(new GridVector[] {
                new GridRow(VectorSizeType.Proportional, 33),
                new GridRow(VectorSizeType.Proportional, 33),
                new GridRow(VectorSizeType.Proportional, 33) }, 100),
                Is.EqualTo(new int[] { 33, 33, 34 }));

            // Should leave the fixed one at 50 and extend the last proportional entry
            Assert.That(GridVector.GetAbsolutes(new GridVector[] {
                new GridRow(VectorSizeType.Proportional, 33),
                new GridRow(VectorSizeType.Proportional, 33),
                new GridRow(VectorSizeType.Proportional, 33),
                new GridRow(VectorSizeType.Absolute, 50)}, 150),
                Is.EqualTo(new int[] { 33, 33, 34, 50 }));
        }


        [Test]
        public void TestInvalidSize()
        {
            GridRow r = new GridRow();
            GridColumn c = new GridColumn();

            try
            {
                r.Value = 0;
                Assert.Fail("Setting a grid row size to 0 should fail");
            }
            catch { }

            try
            {
                c.Value = 0;
                Assert.Fail("Setting a grid column size to 0 should fail");
            }
            catch { }

            try
            {
                r = new GridRow(VectorSizeType.Absolute, 0);
                Assert.Fail("new GridRow(Fixed, 0) should fail");
            }
            catch { }

            try
            {
                c = new GridColumn(VectorSizeType.Absolute, 0);
                Assert.Fail("new GridColumn(Fixed, 0) should fail");
            }
            catch { }

            try
            {
                r = new GridRow(VectorSizeType.Proportional, 0);
                Assert.Fail("new GridRow(Proportional, 0) should fail");
            }
            catch { }

            try
            {
                c = new GridColumn(VectorSizeType.Proportional, 0);
                Assert.Fail("new GridColumn(Proportional, 0) should fail");
            }
            catch { }
        }
    }
}