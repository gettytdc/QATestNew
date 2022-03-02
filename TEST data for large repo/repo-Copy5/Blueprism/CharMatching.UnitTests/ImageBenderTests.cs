#if UNITTESTS

using System.Collections.Generic;
using System.Drawing;
using BluePrism.UnitTesting.TestSupport;
using NUnit.Framework;
using Corner = BluePrism.CharMatching.ImageBender.Corner;

namespace BluePrism.CharMatching.UnitTests
{
    [TestFixture]
    class ImageBenderTests
    {
        /// <summary>
        /// Gets a bitmap generator with 5 colors defined:
        /// R: Red; G: Green; B: Blue; W: White; K: Black
        /// </summary>
        /// <returns>A bitmap generator populated with 5 colours</returns>
        private TestBitmapGenerator GetBitmapGenerator()
        {
            return new TestBitmapGenerator().WithColours(new Dictionary<char, Color>
            {
                { 'R', Color.Red },
                { 'G', Color.Green },
                { 'B', Color.Blue },
                { 'W', Color.White },
                { 'K', Color.Black },
            });
        }

        [Test]
        public void TestFindCornerOfColour_SinglePixel()
        {
            using (var image = GetBitmapGenerator().WithPixels(@"
                RRR
                RRR
                RRK").Create())
            {

                Assert.That(
                    ImageBender.FindCornerOfColour(image, Color.Black, Corner.TopLeft),
                    Is.EqualTo(new Point(2, 2))
                );
                Assert.That(
                    ImageBender.FindCornerOfColour(image, Color.Black, Corner.TopRight),
                    Is.EqualTo(new Point(2, 2))
                );
                Assert.That(
                    ImageBender.FindCornerOfColour(image, Color.Black, Corner.BottomLeft),
                    Is.EqualTo(new Point(2, 2))
                );
                Assert.That(
                    ImageBender.FindCornerOfColour(image, Color.Black, Corner.BottomRight),
                    Is.EqualTo(new Point(2, 2))
                );
            }
        }

        [Test]
        public void TestFindCornerOfColour_DisparatePixels()
        {
            using (var image = GetBitmapGenerator().WithPixels(@"
                RKRR
                RRRR
                RRKR
                RRRR").Create())
            {
                Assert.That(
                    ImageBender.FindCornerOfColour(image, Color.Black, Corner.TopLeft),
                    Is.EqualTo(new Point(1, 0))
                );
                Assert.That(
                    ImageBender.FindCornerOfColour(image, Color.Black, Corner.TopRight),
                    Is.EqualTo(new Point(2, 0))
                );
                Assert.That(
                    ImageBender.FindCornerOfColour(image, Color.Black, Corner.BottomLeft),
                    Is.EqualTo(new Point(1, 2))
                );
                Assert.That(
                    ImageBender.FindCornerOfColour(image, Color.Black, Corner.BottomRight),
                    Is.EqualTo(new Point(2, 2))
                );
            }
        }

        [Test]
        public void TestFindCornerOfColour_BasicRectangle()
        {
            using (var image = GetBitmapGenerator().WithPixels(@"
                RRK
                RRK
                KKK").Create())
            {
                Assert.That(
                    ImageBender.FindCornerOfColour(image, Color.Red, Corner.TopLeft),
                    Is.EqualTo(new Point(0, 0))
                );
                Assert.That(
                    ImageBender.FindCornerOfColour(image, Color.Red, Corner.TopRight),
                    Is.EqualTo(new Point(1, 0))
                );
                Assert.That(
                    ImageBender.FindCornerOfColour(image, Color.Red, Corner.BottomLeft),
                    Is.EqualTo(new Point(0, 1))
                );
                Assert.That(
                    ImageBender.FindCornerOfColour(image, Color.Red, Corner.BottomRight),
                    Is.EqualTo(new Point(1, 1))
                );
            }

            using (var image = GetBitmapGenerator().WithPixels(@"
                KRR
                KRR
                KKK").Create())
            {
                Assert.That(
                    ImageBender.FindCornerOfColour(image, Color.Red, Corner.TopLeft),
                    Is.EqualTo(new Point(1, 0))
                );
                Assert.That(
                    ImageBender.FindCornerOfColour(image, Color.Red, Corner.TopRight),
                    Is.EqualTo(new Point(2, 0))
                );
                Assert.That(
                    ImageBender.FindCornerOfColour(image, Color.Red, Corner.BottomLeft),
                    Is.EqualTo(new Point(1, 1))
                );
                Assert.That(
                    ImageBender.FindCornerOfColour(image, Color.Red, Corner.BottomRight),
                    Is.EqualTo(new Point(2, 1))
                );
            }

            using (var image = GetBitmapGenerator().WithPixels(@"
                KKK
                KRR
                KRR").Create())
            {
                Assert.That(
                    ImageBender.FindCornerOfColour(image, Color.Red, Corner.TopLeft),
                    Is.EqualTo(new Point(1, 1))
                );
                Assert.That(
                    ImageBender.FindCornerOfColour(image, Color.Red, Corner.TopRight),
                    Is.EqualTo(new Point(2, 1))
                );
                Assert.That(
                    ImageBender.FindCornerOfColour(image, Color.Red, Corner.BottomLeft),
                    Is.EqualTo(new Point(1, 2))
                );
                Assert.That(
                    ImageBender.FindCornerOfColour(image, Color.Red, Corner.BottomRight),
                    Is.EqualTo(new Point(2, 2))
                );
            }

            using (var image = GetBitmapGenerator().WithPixels(@"
                KKK
                RRK
                RRK").Create())
            {
                Assert.That(
                    ImageBender.FindCornerOfColour(image, Color.Red, Corner.TopLeft),
                    Is.EqualTo(new Point(0, 1))
                );
                Assert.That(
                    ImageBender.FindCornerOfColour(image, Color.Red, Corner.TopRight),
                    Is.EqualTo(new Point(1, 1))
                );
                Assert.That(
                    ImageBender.FindCornerOfColour(image, Color.Red, Corner.BottomLeft),
                    Is.EqualTo(new Point(0, 2))
                );
                Assert.That(
                    ImageBender.FindCornerOfColour(image, Color.Red, Corner.BottomRight),
                    Is.EqualTo(new Point(1, 2))
                );
            }

        }

        [Test]
        public void TestFindCornerOfColour_RectMustBoundAllColourPixels()
        {
            using (var image = GetBitmapGenerator().WithPixels(@"
                RRR
                RRR
                RRK").Create())
            {
                Assert.That(
                    ImageBender.FindCornerOfColour(image, Color.Red, Corner.TopLeft),
                    Is.EqualTo(new Point(0, 0))
                );
                Assert.That(
                    ImageBender.FindCornerOfColour(image, Color.Red, Corner.TopRight),
                    Is.EqualTo(new Point(2, 0))
                );
                Assert.That(
                    ImageBender.FindCornerOfColour(image, Color.Red, Corner.BottomLeft),
                    Is.EqualTo(new Point(0, 2))
                );
                Assert.That(
                    ImageBender.FindCornerOfColour(image, Color.Red, Corner.BottomRight),
                    Is.EqualTo(new Point(2, 2))
                );
            }
        }
    }
}

#endif