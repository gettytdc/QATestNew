using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using BluePrism.ApplicationManager;
using BluePrism.BPCoreLib;
using BluePrism.CharMatching;
using BluePrism.CharMatching.UI;
using NUnit.Framework;

namespace ClientComms.UnitTests.ClientComms.UnitTests
{
    [TestFixture]
    public class RegionFinderTests
    {
        private static readonly Dictionary<char, Color> Colors = new Dictionary<char, Color>() { { 'R', Color.FromArgb(255, Color.Red) }, { 'G', Color.FromArgb(255, Color.Green) }, { 'B', Color.FromArgb(255, Color.Blue) }, { 'W', Color.FromArgb(255, Color.White) } };
        private static readonly string[] Window1Pixels = new string[] { "RRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRR", "RWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWR", "RWRRRWWWWWWWWWWWWWWWWWWWWWWWWWWWWR", "RWGGGWWWWWWWWWWWWWWWWWWWWWWWWWWWWR", "RWBBBWWWWWWWBBBWWWWWWWWWWBBBWWWWWR", "RWWWWWWWWWWWGGGWWWWWWWWWWBGBWWWWWR", "RWWWWWWWWWWWRRRWWWWWWWWWWBBBWWWWWR", "RWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWR", "RWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWR", "RWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWR", "RWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWR", "RWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWR", "RRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRR" };

        /// <summary>
        /// Pixel data for region 1 (located at 2,2 within window 1)
        /// </summary>
        private static readonly string[] Region1Pixels = new string[] { "RRR", "GGG", "BBB" };

        /// <summary>
        /// Pixel data for region 2 (located at 12,5 within window 1)
        /// </summary>
        private static readonly string[] Region2Pixels = new string[] { "BBB", "GGG", "RRR" };

        /// <summary>
        /// Pixel data for region 3 (located at 25,5 within window 1)
        /// </summary>
        private static readonly string[] Region3Pixels = new string[] { "BBB", "BGB", "BBB" };

        /// <summary>
        /// Pixel data for region that does not exist within window 1
        /// </summary>
        private static readonly string[] MissingRegion1Pixels = new string[] { "GGG", "GGG", "BBB" };
        private static readonly Bitmap Region1Image;
        private static readonly Bitmap Region2Image;
        private static readonly Bitmap Region3Image;
        private static readonly Bitmap MissingRegion1Image;

        static RegionFinderTests()
        {
            Region1Image = new BluePrism.UnitTesting.TestSupport.TestBitmapGenerator().WithColours(Colors).WithPixels(Region1Pixels).Create();


            Region2Image = new BluePrism.UnitTesting.TestSupport.TestBitmapGenerator().WithColours(Colors).WithPixels(Region2Pixels).Create();


            Region3Image = new BluePrism.UnitTesting.TestSupport.TestBitmapGenerator().WithColours(Colors).WithPixels(Region3Pixels).Create();


            MissingRegion1Image = new BluePrism.UnitTesting.TestSupport.TestBitmapGenerator().WithColours(Colors).WithPixels(MissingRegion1Pixels).Create();


        }

        private Bitmap CreateWindow1Image()
        {
            // Match PixelFormat of clsPixRect bitmaps
            var image = new BluePrism.UnitTesting.TestSupport.TestBitmapGenerator().WithColours(Colors).WithPixels(Window1Pixels).WithPixelFormat(PixelFormat.Format24bppRgb).Create();



            return image;
        }

        [Test]
        public void ShouldFindRegionWithoutParentLocatedUsingCoordinates()
        {
            var @params = new RegionLocationParams(new Rectangle(0, 20, 0, 20), RegionLocationMethod.Coordinates, RegionPosition.Anywhere, Padding.Empty, null, null, 0, false);
            var finder = new RegionFinder(() => null);
            var coordinates = finder.FindRegion(@params);
            Assert.That(coordinates, Is.EqualTo(@params.Coordinates));
        }

        [Test]
        public void ShouldFindRegionWithZeroPaddingIfImageAtOriginalPosition()
        {
            var regionImage = new clsPixRect(Region1Image);
            var @params = new RegionLocationParams(new Rectangle(2, 2, 3, 3), RegionLocationMethod.Image, RegionPosition.Fixed, Padding.Empty, regionImage, null, 0, false);
            var finder = new RegionFinder(() => CreateWindow1Image());
            var coordinates = finder.FindRegion(@params);
            Assert.That(coordinates, Is.EqualTo(@params.Coordinates));
        }

        [Test]
        public void ShouldNotFindRegionWithZeroPaddingIfImageNotAtOriginalPosition()
        {
            var regionImage = new clsPixRect(Region1Image);
            var @params = new RegionLocationParams(new Rectangle(3, 3, 3, 3), RegionLocationMethod.Image, RegionPosition.Fixed, Padding.Empty, regionImage, null, 0, false);
            var finder = new RegionFinder(() => CreateWindow1Image());
            Assert.That(() => finder.FindRegion(@params), Throws.InstanceOf<NoSuchImageRegionException>());
        }

        [Test]
        public void ShouldFindImageRegionThatHasMovedWhenSearchPaddingIsUsed()
        {
            var regionImage = new clsPixRect(Region1Image);
            var @params = new RegionLocationParams(new Rectangle(8, 8, 3, 3), RegionLocationMethod.Image, RegionPosition.Fixed, new Padding(10), regionImage, null, 0, false);
            var finder = new RegionFinder(() => CreateWindow1Image());
            var coordinates = finder.FindRegion(@params);
            var expectedCoordinates = new Rectangle(2, 2, 3, 3);
            Assert.That(coordinates, Is.EqualTo(expectedCoordinates));
        }

        [Test]
        public void ShouldNotFindMissingImageRegionWithPaddingThatExtendsBeyondContainer()
        {
            var regionImage = new clsPixRect(MissingRegion1Image);
            var @params = new RegionLocationParams(new Rectangle(8, 8, 3, 3), RegionLocationMethod.Image, RegionPosition.Fixed, new Padding(10000), regionImage, null, 0, false);
            var finder = new RegionFinder(() => CreateWindow1Image());
            Assert.That(() => finder.FindRegion(@params), Throws.InstanceOf<NoSuchImageRegionException>());
        }

        [Test]
        public void ShouldFindRegionLocatedUsingImageViaParentWithoutPadding()
        {
            var image1 = new clsPixRect(Region1Image);
            var image2 = new clsPixRect(Region2Image);
            var region1Params = new RegionLocationParams(new Rectangle(2, 2, 3, 3), RegionLocationMethod.Image, RegionPosition.Anywhere, Padding.Empty, image1, null, 0, false);
            var region2Params = new RegionLocationParams(new Rectangle(12, 4, 3, 3), RegionLocationMethod.Image, RegionPosition.Relative, Padding.Empty, image2, region1Params, 0, false);
            var finder = new RegionFinder(() => CreateWindow1Image());
            var coordinates = finder.FindRegion(region2Params);
            Assert.That(coordinates, Is.EqualTo(region2Params.Coordinates));
        }

        [Test]
        public void ShouldFindRegionImageWithGreyScaleLocatedUsingImageViaParentWithoutPaddingAndHasColour()
        {
            Bitmap greyImage2 = (Bitmap)ImageBender.GrayscaleImage(Region2Image, default);
            var image1 = new clsPixRect(Region1Image);
            var image2 = new clsPixRect(greyImage2);
            var region1Params = new RegionLocationParams(new Rectangle(2, 2, 3, 3), RegionLocationMethod.Image, RegionPosition.Anywhere, Padding.Empty, image1, null, 0, false);
            var region2Params = new RegionLocationParams(new Rectangle(12, 4, 3, 3), RegionLocationMethod.Image, RegionPosition.Relative, Padding.Empty, image2, region1Params, 0, true);
            var finder = new RegionFinder(() => CreateWindow1Image());
            var coordinates = finder.FindRegion(region2Params);
            Assert.That(coordinates, Is.EqualTo(region2Params.Coordinates));
        }

        [Test]
        public void ShouldFindImageRegionThatHasMovedUsingGreyScale()
        {
            Bitmap greyImage1 = (Bitmap)ImageBender.GrayscaleImage(Region1Image, default);
            var regionImage = new clsPixRect(greyImage1);
            var @params = new RegionLocationParams(new Rectangle(8, 8, 3, 3), RegionLocationMethod.Image, RegionPosition.Anywhere, Padding.Empty, regionImage, null, 0, true);
            var finder = new RegionFinder(() => CreateWindow1Image());
            var coordinates = finder.FindRegion(@params);
            var expectedCoordinates = new Rectangle(2, 2, 3, 3);
            Assert.That(coordinates, Is.EqualTo(expectedCoordinates));
        }

        /// <summary>
        /// Tests scenario where region 1 is located using a 3x3 pixel image originally recorded
        /// at (10,10) but which will actually be found at (2,2). The child region 2 located using
        /// coordinates and originally recorded at (18, 13) - translated (8, 3) from region 1. The
        /// final position of region 2 will be based on the point where region 1 was found, translated
        /// by the original offset recorded between region 1 and region 2.
        /// </summary>
        /// <remarks></remarks>
        [Test]
        public void ShouldTranslateRegionLocatedUsingCoordinatesRelativeToParentLocatedUsingImage()
        {
            var image1 = new clsPixRect(Region1Image);
            var image2 = new clsPixRect(Region2Image);
            var region1Params = new RegionLocationParams(new Rectangle(10, 10, 3, 3), RegionLocationMethod.Image, RegionPosition.Anywhere, Padding.Empty, image1, null, 0, false);
            var region2Params = new RegionLocationParams(new Rectangle(18, 13, 3, 3), RegionLocationMethod.Coordinates, RegionPosition.Relative, Padding.Empty, image2, region1Params, 0, false);
            var finder = new RegionFinder(() => CreateWindow1Image());
            var coordinates = finder.FindRegion(region2Params);
            var expectedCoordinates = new Rectangle(10, 5, 3, 3);
            Assert.That(coordinates, Is.EqualTo(expectedCoordinates));
        }

        [Test]
        public void ShouldDisposeCapturedWindowImageFollowingSearch()
        {
            var windowImage = CreateWindow1Image();
            var regionImage = new clsPixRect(Region1Image);
            var @params = new RegionLocationParams(new Rectangle(2, 2, 3, 3), RegionLocationMethod.Image, RegionPosition.Fixed, Padding.Empty, regionImage, null, 0, false);
            var finder = new RegionFinder(() => windowImage);
            var coordinates = finder.FindRegion(@params);
            Assert.That(windowImage.PixelFormat, Is.EqualTo(PixelFormat.DontCare));
        }

        [Test]
        public void ShouldGiveCorrectLevelInExceptionMessage()
        {
            var image1 = new clsPixRect(Region1Image);
            var region1Params = new RegionLocationParams(new Rectangle(3, 3, 3, 3), RegionLocationMethod.Image, RegionPosition.Fixed, Padding.Empty, image1, null, 0, false);
            var image2 = new clsPixRect(Region2Image);
            var region2Params = new RegionLocationParams(new Rectangle(12, 4, 3, 3), RegionLocationMethod.Image, RegionPosition.Relative, Padding.Empty, image2, region1Params, 0, false);
            var image3 = new clsPixRect(Region3Image);
            var region3Params = new RegionLocationParams(new Rectangle(12, 4, 3, 3), RegionLocationMethod.Image, RegionPosition.Relative, Padding.Empty, image3, region2Params, 0, false);
            var finder = new RegionFinder(() => CreateWindow1Image());
            NoSuchImageRegionException ex;
            ex = Assert.Catch<NoSuchImageRegionException>(() => finder.FindRegion(region1Params));
            StringAssert.Contains("Cannot find region identified as an image.", ex.Message);
            ex = Assert.Catch<NoSuchImageRegionException>(() => finder.FindRegion(region2Params));
            StringAssert.Contains("A relative parent (1 above the target region) could not be identified as an image.", ex.Message);
            ex = Assert.Catch<NoSuchImageRegionException>(() => finder.FindRegion(region3Params));
            StringAssert.Contains("A relative parent (2 above the target region) could not be identified as an image.", ex.Message);
        }
    }
}
