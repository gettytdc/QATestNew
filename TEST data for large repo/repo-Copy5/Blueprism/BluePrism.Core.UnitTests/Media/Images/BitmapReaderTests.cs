#if UNITTESTS
using System.Drawing;
using System.Drawing.Imaging;
using BluePrism.Core.Media.Images;
using NUnit.Framework;
using BluePrism.UnitTesting.TestSupport;

namespace BluePrism.Core.UnitTests.Media.Images
{
    public class BitmapReaderTests
    {
        // 0 denotes an Exact match in FullColorMatcher which give the same functionality as
        // the old ExactColorMatcher class.
        private static readonly IColorMatcher ExactColorMatcher = new FullColorMatcher(0);

        [Test]
        public void ShouldGetPixelColorAtPoint()
        {
            var red = Color.FromArgb(255, Color.Red);
            var green = Color.FromArgb(255, Color.Green);
            var blue = Color.FromArgb(255, Color.Blue);
            var white = Color.FromArgb(255, Color.White);

            var image = new TestBitmapGenerator()
                .WithColour('R', red)
                .WithColour('G', green)
                .WithColour('B', blue)
                .WithColour('W', white)
                .WithPixels(@"
RRRRRRRR
RRRRRRRR
RRGGGGRR
RRBBBBRR
RRWWWWRR
RRRRRRRR
RRRRRRRR
")
                .Create();

            using (var reader = new BitmapReader(image))
            {
                Assert.That(reader.GetPixel(0, 0), Is.EqualTo(red));
                Assert.That(reader.GetPixel(7, 0), Is.EqualTo(red));
                Assert.That(reader.GetPixel(2, 2), Is.EqualTo(green));
                Assert.That(reader.GetPixel(5, 2), Is.EqualTo(green));
                Assert.That(reader.GetPixel(2, 3), Is.EqualTo(blue));
                Assert.That(reader.GetPixel(5, 3), Is.EqualTo(blue));
                Assert.That(reader.GetPixel(2, 4), Is.EqualTo(white));
                Assert.That(reader.GetPixel(5, 4), Is.EqualTo(white));
            }
        }

        protected static object[] Cases =
        {
            new object[] {0, 0, 0, 0, true },
            new object[] {2, 0, 2, 0, true },
            new object[] {1, 1, 1, 1, true },
            new object[] { 0, 0, 2, 1, true},
            new object[] {0, 0, 0, 1, false },
            new object[] { 0, 0, 0, 2, false }
        };

        [TestCaseSource("Cases")]
        public void MatchesOtherAtPointShouldDetectMatchingPoints(int x1, int y1, int x2, int y2, bool expectedResult)
        {
            var generator = GetRGBTestMapGenerator()
                .WithPixels(@"
RGB
BGR
");
            var image1 = generator.Create();
            var image2 = generator.Create();

            using (var reader1 = new BitmapReader(image1))
            using (var reader2 = new BitmapReader(image2))
            {
                Assert.AreEqual(expectedResult, reader1.MatchesOtherAtPoint(new Point(x1, y1), reader2, new Point(x2, y2), ExactColorMatcher));
            }
        }


        [Test]
        public void DisposeShouldUnlockBits()
        {
            var image = new TestBitmapGenerator()
                .WithColour('R', Color.Red)
                .WithPixels("RRRR")
                .Create();
            var reader = new BitmapReader(image);
            reader.Dispose();
            // LockBits would fail with "Bitmap region is already locked" error if still locked
            var data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly, image.PixelFormat);
            image.UnlockBits(data);
        }

        [TestCase(10)]
        [TestCase(20)]
        public void MatchesOtherAtPoint_MatchesBitmapUsingTolerance_ReturnsTrue(int tolerance)
        {
            //arrange
            var origImageGenerator = GetRGBTestMapGenerator()
                            .WithPixels(@"
RGB
BGR
");

            var colorVarianceImageGenerator = GetRGBTestMapGenerator()
                            .WithPixels(@"
rgb
bgr
");

            var originalImage = origImageGenerator.Create();
            var testAgainstImage = colorVarianceImageGenerator.Create();
            var matchFound = false;

            //act
            using (var originalReader = new BitmapReader(originalImage))
            using (var toleranceReader = new BitmapReader(testAgainstImage))
            {
                var colorMatcher = new FullColorMatcher(20);
                matchFound = originalReader.MatchesOtherAtPoint(new Point(0, 0), toleranceReader, new Point(0, 0), colorMatcher);
            }

            Assert.IsTrue(matchFound);
        }


        [TestCase(0)]
        [TestCase(5)]
        public void MatchesOtherAtPoint_MatchesBitmapGivenTolerance_ReturnsFalse(int tolerance)
        {
            //arrange
            var origImageGenerator = GetRGBTestMapGenerator()
                            .WithPixels(@"
RGB
BGR
");

            var colorVarianceImageGenerator = GetRGBTestMapGenerator()
                            .WithPixels(@"
rgb
bgr
");

            var originalImage = origImageGenerator.Create();
            var testAgainstImage = colorVarianceImageGenerator.Create();
            var matchFound = false;

            //act
            using (var originalReader = new BitmapReader(originalImage))
            using (var toleranceReader = new BitmapReader(testAgainstImage))
            {
                var colorMatcher = new FullColorMatcher(tolerance);
                matchFound = originalReader.MatchesOtherAtPoint(new Point(0, 0), toleranceReader, new Point(0, 0), colorMatcher);
            }

            Assert.IsFalse(matchFound);
        }

        private TestBitmapGenerator GetRGBTestMapGenerator()
        {
            var red = Color.FromArgb(255, Color.Red);
            var green = Color.FromArgb(255, Color.Green);
            var blue = Color.FromArgb(255, Color.Blue);

            return new TestBitmapGenerator()
                .WithColour('R', red)
                .WithColour('G', green)
                .WithColour('B', blue)
                // Add colours with slight rgb variance
                .WithColour('r', Color.FromArgb(245, 10, 10))
                .WithColour('g', Color.FromArgb(10, 118, 10))
                .WithColour('b', Color.FromArgb(10, 10, 245));

        }
    }
}
#endif