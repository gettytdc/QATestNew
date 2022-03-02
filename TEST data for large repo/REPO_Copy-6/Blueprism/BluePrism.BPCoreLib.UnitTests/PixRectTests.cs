#if UNITTESTS

using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using NUnit.Framework;

namespace BluePrism.BPCoreLib.UnitTests
{
    [TestFixture]
    public class PixRectTests
    {
        // A temp file used for a round trip test - cleaned up in DeleteTempFile()
        private string _tempFile;

        /// <summary>
        /// Tests the various round trips between PixRects, Bitmaps and the string
        /// representations used throughout PixRect
        /// </summary>
        [Test]
        public void TestRoundTrip()
        {
            clsPixRect pic;
            var col = Color.Black;
            using (var b = new Bitmap(30, 30))
            {
                for (int i = 0, loopTo = b.Width - 1; i <= loopTo; i++)
                {
                    for (int j = 0, loopTo1 = b.Height - 1; j <= loopTo1; j++)
                    {
                        b.SetPixel(i, j, col);
                        if (col.R < 253)
                        {
                            col = Color.FromArgb(col.R + 3, col.G + 2, col.B + 3);
                        }
                    }
                }

                pic = new clsPixRect(b);
                Assert.That(pic.ToString(), Is.EqualTo(clsPixRect.BitmapToString(b)));
            }

            // PNG can contain 'last modified' time info - ensure that it doesn't make
            // a difference to the string created
            var str = pic.ToString();
            Thread.Sleep(2500);
            Assert.That(pic.ToString(), Is.EqualTo(str));

            // Surives a trip to Bitmap and back
            var bmp2 = pic.ToBitmap();
            var pic2 = new clsPixRect(bmp2);
            Assert.That(pic2.ToString(), Is.EqualTo(str));

            // Survives a trip to output file and back
            _tempFile = BPUtil.GetRandomFilePath();
            using (var outStr = new FileStream(_tempFile, FileMode.Create, FileAccess.Write))
            {
                bmp2.Save(outStr, ImageFormat.Png);
            }

            Bitmap bmp3;
            using (var inStr = new FileStream(_tempFile, FileMode.Open))
            {
                bmp3 = (Bitmap)Image.FromStream(inStr);
            }

            var pic3 = new clsPixRect(bmp3);
            Assert.That(pic3.ToString(), Is.EqualTo(str));

            // Survives a ToString()/Parse() trip
            var bmp4 = clsPixRect.ParseBitmap(str);
            var pic4 = new clsPixRect(bmp4);
            Assert.That(pic4.ToString(), Is.EqualTo(str));

            // Test the direct BitmapToString() methods on all the above bitmaps
            Assert.That(clsPixRect.BitmapToString(bmp2), Is.EqualTo(str));
            var str3 = clsPixRect.BitmapToString(bmp3);
            Assert.That(str3, Is.EqualTo(str));
            Assert.That(clsPixRect.BitmapToString(bmp3), Is.EqualTo(str));
            Assert.That(clsPixRect.BitmapToString(bmp4), Is.EqualTo(str));
        }

        [Test]
        public void TestNormalise()
        {
            clsPixRect pic;
            var col = Color.Black;
            using (var b = new Bitmap(30, 30, PixelFormat.Format32bppArgb))
            {
                for (int i = 0, loopTo = b.Width - 1; i <= loopTo; i++)
                {
                    for (int j = 0, loopTo1 = b.Height - 1; j <= loopTo1; j++)
                    {
                        b.SetPixel(i, j, col);
                        if (col.R < 253)
                        {
                            col = Color.FromArgb(col.R + 3, col.G + 2, col.B + 3);
                        }
                    }
                }

                pic = new clsPixRect(b);
                Assert.That(pic.ToString(), Is.EqualTo(clsPixRect.BitmapToString(b)));
            }
        }

        /// <summary>
        /// Deletes the temp file if one has been created
        /// </summary>
        [TearDown]
        public void DeleteTempFile()
        {
            if (_tempFile != null)
            {
                File.Delete(_tempFile);
                _tempFile = null;
            }
        }
    }
}

#endif
