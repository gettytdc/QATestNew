#if UNITTESTS

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using BluePrism.BPCoreLib;
using BluePrism.Core.Media.Images;
using NUnit.Framework;

namespace BluePrism.Core.UnitTests.Media.Images
{
    /// <summary>
    /// Designed to be run manually during development - these should be run in release mode to 
    /// get a realistic idea of execution time (and to do that you'll need to remove the #if UNITTESTS
    /// and #endif directives). Note that paths to images depend on the tests being run on assemblies
    /// in the bin directory with the source code being present.
    /// </summary>
    [Explicit("Intended for manual testing in image-based automation development")]
    public class ImageSearcherPerformanceTests
    {
        static ImageSearcherPerformanceTests()
        {
            string directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\BluePrism.Core\\_UnitTests\\Media\\Images\\TempImages");
            AppImage = new Bitmap(Path.Combine(directory, "application.bmp"));
            RegionImage = new Bitmap(Path.Combine(directory, "temporary-address-label.bmp"));
        }

        private static readonly Bitmap AppImage;

        private static readonly Bitmap RegionImage;
            
        [Test]
        public void FindUsingImageSearcher()
        {
            TestSpeed(() =>
            {
                using (var imageSearcher = new ImageSearcher(AppImage))
                {
                    var colorMatcher = new FullColorMatcher(0);
                    var result = imageSearcher.FindSubImage(RegionImage, new Point(0, 0), Rectangle.Empty, colorMatcher);
                    Assert.That(result, Is.Not.Null);
                    Assert.That(result, Is.Not.EqualTo(Point.Empty));
                }
            });
        }

        [Test]
        public void FindUsingPixRect()
        {
            TestSpeed(() =>
            {
                var containerRect = new clsPixRect(AppImage);
                var clsPixRect = new clsPixRect(RegionImage);
                Point point = Point.Empty;
                bool result = containerRect.Contains(clsPixRect, ref point);
                Assert.That(result, Is.True);
                Assert.That(point, Is.Not.EqualTo(Point.Empty));
            });
        }

        private void TestSpeed(Action action)
        {
            // Warmup
            var watch = Stopwatch.StartNew();
            action();
            watch.Stop();
            Console.WriteLine("Warmup time: {0}", watch.Elapsed);
            
            // Repeat
            const int repeatCount = 10;
            var repeatTimes = new List<TimeSpan>();
            for (int i = 0; i < repeatCount; i++)
            {
                watch = Stopwatch.StartNew();
                action();
                watch.Stop();
                repeatTimes.Add(watch.Elapsed);
            }
            var average = TimeSpan.FromMilliseconds(repeatTimes.Select(x => x.TotalMilliseconds).Average());
            Console.WriteLine("Average repeat time: {0} - Repeat times: {1}", average, string.Join(", ", repeatTimes));
            Assert.Pass();
        }
    }
}
#endif