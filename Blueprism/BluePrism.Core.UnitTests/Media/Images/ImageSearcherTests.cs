#if UNITTESTS

using System;
using System.Collections.Generic;
using BluePrism.Core.Media.Images;
using BluePrism.UnitTesting.TestSupport;
using NUnit.Framework;

using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace BluePrism.Core.UnitTests.Media.Images
{
    public class ImageSearcherTests
    {
        private static readonly Dictionary<char, Color> Colors = new Dictionary<char, Color>
        {
            {'W', Color.FromArgb(255, Color.White)},
            {'R', Color.FromArgb(255, Color.Red)},
            {'G', Color.FromArgb(255, Color.Green)},
            {'B', Color.FromArgb(255, Color.Blue)},
            {'P', Color.FromArgb(255, Color.Purple)},
            // Colours with slight colour variations
            {'w', Color.FromArgb(255, 245, 245, 245)},
            {'r', Color.FromArgb(255, 245, 10, 10)},
            {'g', Color.FromArgb(255, 10, 138, 10)},
            {'b', Color.FromArgb(255, 10, 10, 245)},
        };

        private static readonly Size ContainerSize = new Size(10, 10);
        private const string ContainerPixels = @"
GGGWWWWBBB
WWWWWWWRRR
GGGWWWWBBB
WWWWWWWWWW
GGWRRRRWRR
GGWGWWGWRR
RRRBBBBGGG
BBWWGGWBGB
RRRWGGWRRR";

        private const string TopLeftPixels = @"
GGG
WWW
GGG";

        private const string TopLeftPixelsWithColourVariance = @"
ggg
www
ggg";

        private const string TopRightPixels = @"
BBB
RRR
BBB";

        private const string TopRightPixelsWithColourVariance = @"
bbb
rrr
bbb";

        private const string MiddlePixels = @"
RRRR
GWWG
BBBB";

        private const string MiddlePixelsWithColourVariance = @"
rrrr
gwwg
bbbb";

        private const string BottomLeftPixels = @"
RRR
BBW
RRR";

        private const string BottomLeftPixelsWithColourVariance = @"
rrr
bbw
rrr";

        
        private const string BottomRightPixels = @"
GGG
BGB
RRR";

        private const string BottomRightPixelsWithColourVariance = @"
ggg
bgb
rrr";
        private const string MissingPixels = @"
PPP
PPP
PPP";

        // Used by SaveImage function to generate filename (not enabled by default)
        private int imageFileNameCounter = 0;

        [TestCase(TopLeftPixels, 0, 0)]
        [TestCase(TopRightPixels, 7, 0)]
        [TestCase(MiddlePixels, 3, 4)]
        [TestCase(BottomLeftPixels, 0, 6)]
        [TestCase(BottomRightPixels, 7, 6)]
        public void ShouldFindSubImages(string pixels, int x, int y)
        {
            AssertFindsSubImage(pixels, new Point(0, 0), Rectangle.Empty, new Point(x, y));
        }

        [Test]
        public void ShouldFindSubImageWhenSearchAreaLargerThanContainer()
        {
            var searchArea = new Rectangle(-10000, -10000, 20000, 20000);
            AssertFindsSubImage(MiddlePixels, new Point(0, 0), searchArea, new Point(3, 4));
        }

        [Test]
        public void ShouldNotFindMissingSubImage()
        {
            AssertDoesNotFindSubImage(MissingPixels, new Point(0, 0), Rectangle.Empty);
        }

        [Test]
        public void ShouldNotFindMissingSubImageWithSearchAreaOutsideOfContainer()
        {
            var searchArea = new Rectangle(-10000, -10000, 20000, 20000);
            AssertDoesNotFindSubImage(MissingPixels, new Point(0, 0), searchArea);
        }

        [Test]
        public void ShouldFindSubImageUsingAnyOriginalPosition()
        {
            for (int y = 0; y < ContainerSize.Height; y++)
            {
                for (int x = 0; x < ContainerSize.Width; x++)
                {
                    AssertFindsSubImage(MiddlePixels, new Point(x, y), Rectangle.Empty, new Point(3,4));
                }
            }
        }

        [TestCase(TopLeftPixelsWithColourVariance, 0, 0)]
        [TestCase(TopRightPixelsWithColourVariance, 7, 0)]
        [TestCase(MiddlePixelsWithColourVariance, 3, 4)]
        [TestCase(BottomLeftPixelsWithColourVariance, 0, 6)]
        [TestCase(BottomRightPixelsWithColourVariance, 7, 6)]
        public void ShouldFindSubImagesWithColorTolerance(string pixels, int x, int y)
        {
            var colorMatcher = new FullColorMatcher(10);
            AssertFindsSubImage(pixels, new Point(0, 0), Rectangle.Empty, new Point(x, y), colorMatcher);
        }

        [Test]
        public void WhenDisposedShouldReleaseLockOnContainerImage()
        {
            var generator = new TestBitmapGenerator()
                .WithColours(Colors)
                .WithPixels(ContainerPixels);
            using (var image = generator.Create())
            {
                var searcher = new ImageSearcher(image);
                searcher.Dispose();
                AssertImageNotLocked(image);
            }
        }

        /// <summary>
        /// Searches for sub image within container using ImageSearcher and asserts that result matches
        /// expected values
        /// </summary>
        /// <param name="pixels">Pixels of the sub image</param>
        /// <param name="imageTopLeft">The parameter supplied to ImageSearcher.FindSubImage</param>
        /// <param name="searchArea">The parameter supplied to ImageSearcher.FindSubImage</param>
        /// <param name="expectedPoint">Expected location</param>
        /// <param name="colorMatcher">IColorMatcher implementation to use</param>
        private void AssertFindsSubImage(string pixels, Point imageTopLeft, Rectangle searchArea, Point expectedPoint, IColorMatcher colorMatcher = null)
        {
            var result = FindSubImage(pixels, imageTopLeft, searchArea, colorMatcher);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.EqualTo(expectedPoint));
        }

        /// <summary>
        /// Searches for sub image within container using ImageSearcher and asserts that image
        /// is not found
        /// </summary>
        /// <param name="pixels">Pixels of the sub image</param>
        /// <param name="imageTopLeft">The parameter supplied to ImageSearcher.FindSubImage</param>
        /// <param name="searchArea">The parameter supplied to ImageSearcher.FindSubImage</param>
        /// <param name="colorMatcher">IColorMatcher implementation to use</param>
        private void AssertDoesNotFindSubImage(string pixels, Point imageTopLeft, Rectangle searchArea, IColorMatcher colorMatcher = null)
        {
            var result = FindSubImage(pixels, imageTopLeft, searchArea, colorMatcher);
            Assert.That(result, Is.Null);
        }

        /// <summary>
        /// Tests that bitmap is not locked
        /// </summary>
        /// <param name="image">Bitmap to test</param>
        private static void AssertImageNotLocked(Bitmap image)
        {
            try
            {
                // LockBits would fail with "Bitmap region is already locked" error if still locked
                var data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                    ImageLockMode.ReadOnly, image.PixelFormat);
                image.UnlockBits(data);
            }
            catch (InvalidOperationException e)
            {
                Assert.Fail("Bitmap locked - attempting to lock threw exception {0}", e);
            }
        }


        /// <summary>
        /// Finds sub image within container using ImageSearcher
        /// </summary>
        /// <param name="pixels">Pixels of the sub image</param>
        /// <param name="imageTopLeft">The parameter supplied to ImageSearcher.FindSubImage</param>
        /// <param name="searchArea">The parameter supplied to ImageSearcher.FindSubImage</param>
        /// <param name="colorMatcher">IColorMatcher implementation to use</param>
        /// <returns>ImageSearcher.FindSubImage result</returns>
        private Point? FindSubImage(string pixels, Point imageTopLeft, Rectangle searchArea, IColorMatcher colorMatcher)
        {
            var generator = new TestBitmapGenerator()
                .WithColours(Colors);

            using (var containerImage = generator.WithPixels(ContainerPixels).Create())
            using (var subImage = generator.WithPixels(pixels).Create())
            using (var searcher = new ImageSearcher(containerImage))
            {
                colorMatcher = colorMatcher ?? new FullColorMatcher(0);
                var result = searcher.FindSubImage(subImage, imageTopLeft, searchArea, colorMatcher);
                return result;
            }
        }

        /// <summary>
        /// For use during development to view images being compared
        /// </summary>
        /// <param name="image"></param>
        private void SaveImage(Bitmap image)
        {
            string directory = "C:\\temp";
            string fileName = string.Format("image{0:hhmmss}-{1}.bmp", DateTime.Now, ++imageFileNameCounter);
            image.Save(Path.Combine(directory, fileName));
        }
    }
}
#endif