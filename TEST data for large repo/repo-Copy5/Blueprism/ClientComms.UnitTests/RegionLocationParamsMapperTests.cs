using System.Drawing;
using System.Windows.Forms;
using BluePrism.ApplicationManager;
using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.BPCoreLib;
using BluePrism.CharMatching.UI;
using NUnit.Framework;

namespace ClientComms.UnitTests.ClientComms.UnitTests
{
    [TestFixture]
    public class RegionLocationParamsMapperTests
    {
        private static readonly clsPixRect Image1;
        private static readonly clsPixRect Image2;
        private static readonly clsPixRect Image3;

        static RegionLocationParamsMapperTests()
        {
            var generator = new BluePrism.UnitTesting.TestSupport.TestBitmapGenerator().WithColour('R', Color.Red).WithColour('W', Color.White);

            Image1 = new clsPixRect(generator.WithPixels("RWWWWWWR").Create());
            Image2 = new clsPixRect(generator.WithPixels("RWWWWWR").Create());
            Image3 = new clsPixRect(generator.WithPixels("RWWWWR").Create());
        }

        [Test]
        public void FromQuery_AnyRegion_ShouldInitialiseDimensionsBasedOnStartAndEndPoints()
        {
            string queryString = "RegionMouseClickCentre StartX=378 StartY=390 EndX=412 EndY=448 ElementSnapshot= " + " LocationMethod=Coordinates RegionPosition=Fixed ImageSearchPadding=\"0, 0, 0, 0\"";
            var query = clsQuery.Parse(queryString);
            var @params = RegionLocationParamsMapper.FromQuery(query);
            Assert.That(@params, Is.Not.Null);
            Assert.That(@params.Coordinates.Width, Is.EqualTo(35));
            Assert.That(@params.Coordinates.Height, Is.EqualTo(59));
            Assert.That(@params.LocationMethod, Is.EqualTo(RegionLocationMethod.Coordinates));
        }

        [Test]
        public void FromQuery_SimpleImageRegion_ShouldInitialiseProperties()
        {
            string queryString = "RegionMouseClickCentre StartX=378 StartY=390 EndX=412 EndY=448 ElementSnapshot=" + Image1.ToString() + " LocationMethod=Image RegionPosition=Fixed ImageSearchPadding=\"5, 10, 15, 20\" ColourTolerance=10 Greyscale=True";
            var query = clsQuery.Parse(queryString);
            var @params = RegionLocationParamsMapper.FromQuery(query);
            Assert.That(@params, Is.Not.Null);
            Assert.That(@params.Coordinates, Is.EqualTo(new Rectangle(378, 390, 35, 59)));
            Assert.That(@params.LocationMethod, Is.EqualTo(RegionLocationMethod.Image));
            Assert.That(@params.Position, Is.EqualTo(RegionPosition.Fixed));
            Assert.That(@params.Padding, Is.EqualTo(new Padding(5, 10, 15, 20)));
            Assert.That(@params.Image, Is.EqualTo(Image1));
            Assert.That(@params.ColourTolerance, Is.EqualTo(10));
            Assert.That(@params.Greyscale, Is.EqualTo(true));
        }

        [Test]
        public void FromQuery_SimpleCoordinatesRegion_ShouldInitialiseProperties()
        {
            string queryString = "RegionMouseClickCentre StartX=378 StartY=390 EndX=412 EndY=448 ElementSnapshot= " + " LocationMethod=Coordinates RegionPosition=Fixed ImageSearchPadding=\"0, 0, 0, 0\"";
            var query = clsQuery.Parse(queryString);
            var @params = RegionLocationParamsMapper.FromQuery(query);
            Assert.That(@params, Is.Not.Null);
            // Ctor params: left, right, top, bottom, ToString format: left, top, bottom, right
            Assert.That(@params.Coordinates, Is.EqualTo(new Rectangle(378, 390, 35, 59)));
            Assert.That(@params.LocationMethod, Is.EqualTo(RegionLocationMethod.Coordinates));
            Assert.That(@params.Position, Is.EqualTo(RegionPosition.Fixed));
            Assert.That(@params.Padding, Is.EqualTo(Padding.Empty));
            Assert.That(@params.Image, Is.Null);
            Assert.That(@params.Parent, Is.Null);
            Assert.That(@params.ColourTolerance, Is.EqualTo(0));
        }

        [Test]
        public void FromQuery_ImageRegionWithRelativeAncestors_ShouldInitialiseProperties()
        {
            string queryString = "RegionMouseClickCentre StartX=378 StartY=390 EndX=412 EndY=448 ElementSnapshot=" + Image3.ToString() + " LocationMethod=Image RegionPosition=Fixed ImageSearchPadding=\"5, 10, 15, 20\" ColourTolerance=10" + " RelativeParent_0_StartX=185 RelativeParent_0_StartY=295 RelativeParent_0_EndX=218 RelativeParent_0_EndY=322" + " RelativeParent_0_ElementSnapshot=" + Image2.ToString() + " RelativeParent_0_LocationMethod=Image RelativeParent_0_RegionPosition=Fixed" + " RelativeParent_0_ImageSearchPadding=\"3, 6, 9, 12\"" + " RelativeParent_0_ColourTolerance=15" + " RelativeParent_0_GreyScale=True" + " RelativeParent_1_StartX=10 RelativeParent_1_StartY=10 RelativeParent_1_EndX=40 RelativeParent_1_EndY=40" + " RelativeParent_1_ElementSnapshot=" + Image1.ToString() + " RelativeParent_1_LocationMethod=Image RelativeParent_1_RegionPosition=Anywhere" + " RelativeParent_1_ColourTolerance=20" + " RelativeParent_1_GreyScale=True" + " RelativeParent_1_ImageSearchPadding=\"0, 0, 0, 0\"";












            var query = clsQuery.Parse(queryString);
            var @params = RegionLocationParamsMapper.FromQuery(query);
            Assert.That(@params, Is.Not.Null);
            // Ctor params: left, right, top, bottom, ToString format: left, top, bottom, right
            Assert.That(@params.Coordinates, Is.EqualTo(new Rectangle(378, 390, 35, 59)));
            Assert.That(@params.LocationMethod, Is.EqualTo(RegionLocationMethod.Image));
            Assert.That(@params.Position, Is.EqualTo(RegionPosition.Fixed));
            Assert.That(@params.Padding, Is.EqualTo(new Padding(5, 10, 15, 20)));
            Assert.That(@params.Image, Is.EqualTo(Image3));
            Assert.That(@params.ColourTolerance, Is.EqualTo(10));
            var ancestor1 = @params.Parent;
            Assert.That(ancestor1, Is.Not.Null);
            Assert.That(ancestor1.Coordinates, Is.EqualTo(new Rectangle(185, 295, 34, 28)));
            Assert.That(ancestor1.LocationMethod, Is.EqualTo(RegionLocationMethod.Image));
            Assert.That(ancestor1.Position, Is.EqualTo(RegionPosition.Fixed));
            Assert.That(ancestor1.Padding, Is.EqualTo(new Padding(3, 6, 9, 12)));
            Assert.That(ancestor1.Image, Is.EqualTo(Image2));
            Assert.That(ancestor1.ColourTolerance, Is.EqualTo(15));
            Assert.That(ancestor1.Greyscale, Is.EqualTo(true));
            var ancestor2 = ancestor1.Parent;
            Assert.That(ancestor2, Is.Not.Null);
            Assert.That(ancestor2.Coordinates, Is.EqualTo(new Rectangle(10, 10, 31, 31)));
            Assert.That(ancestor2.LocationMethod, Is.EqualTo(RegionLocationMethod.Image));
            Assert.That(ancestor2.Position, Is.EqualTo(RegionPosition.Anywhere));
            Assert.That(ancestor2.Padding, Is.EqualTo(new Padding(0, 0, 0, 0)));
            Assert.That(ancestor2.Image, Is.EqualTo(Image1));
            Assert.That(ancestor2.ColourTolerance, Is.EqualTo(20));
            Assert.That(ancestor2.Greyscale, Is.EqualTo(true));
        }
    }
}
