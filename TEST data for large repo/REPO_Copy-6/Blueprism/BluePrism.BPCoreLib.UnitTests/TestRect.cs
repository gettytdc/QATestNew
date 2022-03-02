#if UNITTESTS

using System.Drawing;
using NUnit.Framework;

namespace BluePrism.BPCoreLib.UnitTests
{
    /// <summary>
    /// Test fixture for Win32 Interop
    /// </summary>
    [TestFixture]
    public class TestRect
    {
        [Test]
        public void TestDefaults()
        {
            var r = default(RECT);
            Assert.That(r.Top, Is.EqualTo(0));
            Assert.That(r.Left, Is.EqualTo(0));
            Assert.That(r.Bottom, Is.EqualTo(0));
            Assert.That(r.Right, Is.EqualTo(0));
            Assert.That(r.Width, Is.EqualTo(0));
            Assert.That(r.Height, Is.EqualTo(0));
            Assert.That(r.Centre, Is.EqualTo(Point.Empty));
            Assert.That(r.Location, Is.EqualTo(Point.Empty));
            Assert.That(r.Size, Is.EqualTo(Size.Empty));
        }

        [Test]
        public void TestRectangleInterop()
        {
            RECT r;
            r.Top = 5;
            r.Left = 10;
            r.Right = 300;
            r.Bottom = 100;
            Rectangle rr = r;
            Assert.That(r.Top, Is.EqualTo(rr.Top));
            Assert.That(r.Left, Is.EqualTo(rr.Left));
            Assert.That(r.Bottom, Is.EqualTo(rr.Bottom));
            Assert.That(r.Right, Is.EqualTo(rr.Right));
            Assert.That(r.Width, Is.EqualTo(rr.Width));
            Assert.That(r.Height, Is.EqualTo(rr.Height));
            Assert.That(r.Location, Is.EqualTo(rr.Location));
            Assert.That(r.Size, Is.EqualTo(rr.Size));
            rr = new Rectangle(70, 30, 30, 30);
            r = rr;
            Assert.That(r.Top, Is.EqualTo(rr.Top));
            Assert.That(r.Left, Is.EqualTo(rr.Left));
            Assert.That(r.Bottom, Is.EqualTo(rr.Bottom));
            Assert.That(r.Right, Is.EqualTo(rr.Right));
            Assert.That(r.Width, Is.EqualTo(rr.Width));
            Assert.That(r.Height, Is.EqualTo(rr.Height));
            Assert.That(r.Location, Is.EqualTo(rr.Location));
            Assert.That(r.Size, Is.EqualTo(rr.Size));
            Assert.That(r.Centre, Is.EqualTo(new Point(85, 45)));
        }

        [Test]
        public void TestContains()
        {
            var r = new RECT(10, 90, 20, 50);

            // Corners
            Assert.That(r.Contains(new Point(10, 20)));
            Assert.That(r.Contains(new Point(10, 50)));
            Assert.That(r.Contains(new Point(90, 20)));
            Assert.That(r.Contains(new Point(90, 50)));

            // centre
            Assert.That(r.Contains(r.Centre));

            // One pixel inside the corners
            Assert.That(r.Contains(new Point(11, 20)));
            Assert.That(r.Contains(new Point(11, 50)));
            Assert.That(r.Contains(new Point(89, 20)));
            Assert.That(r.Contains(new Point(89, 50)));
            Assert.That(r.Contains(new Point(10, 21)));
            Assert.That(r.Contains(new Point(10, 49)));
            Assert.That(r.Contains(new Point(90, 21)));
            Assert.That(r.Contains(new Point(90, 49)));
            Assert.That(r.Contains(new Point(11, 21)));
            Assert.That(r.Contains(new Point(11, 49)));
            Assert.That(r.Contains(new Point(89, 21)));
            Assert.That(r.Contains(new Point(89, 49)));

            // One pixel outside the corners
            Assert.That(r.Contains(new Point(9, 20)), Is.False);
            Assert.That(r.Contains(new Point(9, 50)), Is.False);
            Assert.That(r.Contains(new Point(91, 20)), Is.False);
            Assert.That(r.Contains(new Point(91, 50)), Is.False);
            Assert.That(r.Contains(new Point(10, 19)), Is.False);
            Assert.That(r.Contains(new Point(10, 51)), Is.False);
            Assert.That(r.Contains(new Point(90, 19)), Is.False);
            Assert.That(r.Contains(new Point(90, 51)), Is.False);
            Assert.That(r.Contains(new Point(9, 19)), Is.False);
            Assert.That(r.Contains(new Point(9, 51)), Is.False);
            Assert.That(r.Contains(new Point(91, 19)), Is.False);
            Assert.That(r.Contains(new Point(91, 51)), Is.False);
        }

        [Test]
        public void TestModification()
        {
            var r = new RECT(10, 90, 20, 50);
            var rBak = r;
            Assert.That(r.Left, Is.EqualTo(10));
            r.Left = 5;
            r.Top = 5;
            r.Right = 5;
            r.Bottom = 5;
            Assert.That(r.Left, Is.EqualTo(5));
            Assert.That(r.Top, Is.EqualTo(5));
            Assert.That(r.Right, Is.EqualTo(5));
            Assert.That(r.Bottom, Is.EqualTo(5));
            r = rBak;
            r.Offset(new Point(10, 10));

            // Offset shouldn't change the size
            Assert.That(r.Width, Is.EqualTo(rBak.Width));
            Assert.That(r.Height, Is.EqualTo(rBak.Height));
            Assert.That(r.Size, Is.EqualTo(rBak.Size));
            Assert.That(r.Left, Is.EqualTo(rBak.Left + 10));
            Assert.That(r.Top, Is.EqualTo(rBak.Top + 10));
            Assert.That(r.Right, Is.EqualTo(rBak.Right + 10));
            Assert.That(r.Bottom, Is.EqualTo(rBak.Bottom + 10));
            r = rBak;
            r.Expand(new Size(5, 5));

            // Expand shouldn't affect the location
            Assert.That(r.Location, Is.EqualTo(rBak.Location));
            Assert.That(r.Top, Is.EqualTo(rBak.Top));
            Assert.That(r.Left, Is.EqualTo(rBak.Left));
            Assert.That(r.Width, Is.EqualTo(rBak.Width + 5));
            Assert.That(r.Height, Is.EqualTo(rBak.Height + 5));
            Assert.That(r.Size, Is.EqualTo(rBak.Size + new Size(5, 5)));
            Assert.That(r.Right, Is.EqualTo(rBak.Right + 5));
            Assert.That(r.Bottom, Is.EqualTo(rBak.Bottom + 5));
            r = rBak;
            r.Width += 55;
            r.Height += 25;

            // Again, changing width and height shouldn't affect location
            Assert.That(r.Location, Is.EqualTo(rBak.Location));
            Assert.That(r.Top, Is.EqualTo(rBak.Top));
            Assert.That(r.Left, Is.EqualTo(rBak.Left));
            Assert.That(r.Width, Is.EqualTo(rBak.Width + 55));
            Assert.That(r.Height, Is.EqualTo(rBak.Height + 25));
            Assert.That(r.Size, Is.EqualTo(rBak.Size + new Size(55, 25)));
            Assert.That(r.Right, Is.EqualTo(rBak.Right + 55));
            Assert.That(r.Bottom, Is.EqualTo(rBak.Bottom + 25));
        }
    }
}

#endif
