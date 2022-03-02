#if UNITTESTS

using System.ComponentModel;
using System.Drawing;
using BluePrism.CharMatching.UI;
using BluePrism.Core.Utility;

using Moq;
using NUnit.Framework;


namespace BluePrism.CharMatching.UnitTests.UI
{
    public class SpyRegionTypeDescriptorTests
    {
        private static class PropertyNames
        {
            public static readonly string LocationMethod = 
                MemberAccessHelper.GetMemberName((SpyRegion x) => x.LocationMethod);

            public static readonly string Rectangle =
                MemberAccessHelper.GetMemberName((SpyRegion x) => x.Rectangle);

            public static readonly string RetainImage =
                MemberAccessHelper.GetMemberName((SpyRegion x) => x.RetainImage);

            public static readonly string RegionPosition =
                MemberAccessHelper.GetMemberName((SpyRegion x) => x.RegionPosition);

            public static readonly string ImageSearchPadding =
                MemberAccessHelper.GetMemberName((SpyRegion x) => x.ImageSearchPadding);

            public static readonly string ImageSearchAllPadding =
                MemberAccessHelper.GetMemberName((SpyRegion x) => x.ImageSearchAllPadding);

            public static readonly string ImageSearchLeftPadding =
                MemberAccessHelper.GetMemberName((SpyRegion x) => x.ImageSearchLeftPadding);

            public static readonly string ImageSearchRightPadding =
                MemberAccessHelper.GetMemberName((SpyRegion x) => x.ImageSearchRightPadding);

            public static readonly string ImageSearchTopPadding =
                MemberAccessHelper.GetMemberName((SpyRegion x) => x.ImageSearchTopPadding);

            public static readonly string ImageSearchBottomPadding =
                MemberAccessHelper.GetMemberName((SpyRegion x) => x.ImageSearchBottomPadding);

            public static readonly string RelativeParent =
                MemberAccessHelper.GetMemberName((SpyRegion x) => x.RelativeParent);

            public static readonly string ColourTolerance =
                MemberAccessHelper.GetMemberName((SpyRegion x) => x.ColourTolerance);

            public static readonly string Greyscale =
                MemberAccessHelper.GetMemberName((SpyRegion x) => x.Greyscale);
        }

        private const string TestRegionName = "Region 1";
        
        [Test]
        public void GetTypeDescriptor_ShouldCreateDescriptorForRegion()
        {
            var region = CreateRegion();
            var descriptor = CreateDescriptorUsingProvider(region);

            Assert.That(descriptor, Is.Not.Null);
            var properties = descriptor.GetProperties();
            Assert.That(properties[PropertyNames.LocationMethod], Is.Not.Null);
            Assert.That(properties[PropertyNames.Rectangle], Is.Not.Null);
        }

        [Test] 
        public void GetProperties_ImageInFixedPosition_ShouldEnableValidProperties()
        {
            var region = CreateRegion();
            var descriptor = CreateDescriptorUsingProvider(region);

            region.LocationMethod = RegionLocationMethod.Image;
            region.RegionPosition = RegionPosition.Fixed;
            var properties = descriptor.GetProperties();

            AssertValidProperty(properties, PropertyNames.RetainImage, true, true);
            AssertValidProperty(properties, PropertyNames.RegionPosition, true, false);
            AssertValidProperty(properties, PropertyNames.ImageSearchPadding, false, false);

            AssertValidProperty(properties, PropertyNames.ImageSearchAllPadding, true, false);
            AssertValidProperty(properties, PropertyNames.ImageSearchLeftPadding, true, false);
            AssertValidProperty(properties, PropertyNames.ImageSearchRightPadding, true, false);
            AssertValidProperty(properties, PropertyNames.ImageSearchTopPadding, true, false);
            AssertValidProperty(properties, PropertyNames.ImageSearchBottomPadding, true, false);

            AssertValidProperty(properties, PropertyNames.RelativeParent, false, false);
            AssertValidProperty(properties, PropertyNames.ColourTolerance, true, false);
            AssertValidProperty(properties, PropertyNames.Greyscale, true, false);
        }

        [Test]
        public void GetProperties_ImageAnywhere_ShouldEnableValidProperties()
        {
            var region = CreateRegion();
            var descriptor = CreateDescriptorUsingProvider(region);

            region.LocationMethod = RegionLocationMethod.Image;
            region.RegionPosition = RegionPosition.Anywhere;
            var properties = descriptor.GetProperties();

            AssertValidProperty(properties, PropertyNames.RetainImage, true, true);
            AssertValidProperty(properties, PropertyNames.RegionPosition, true, false);
            AssertValidProperty(properties, PropertyNames.ImageSearchPadding, false, false);

            AssertValidProperty(properties, PropertyNames.ImageSearchAllPadding, false, false);
            AssertValidProperty(properties, PropertyNames.ImageSearchLeftPadding, false, false);
            AssertValidProperty(properties, PropertyNames.ImageSearchRightPadding, false, false);
            AssertValidProperty(properties, PropertyNames.ImageSearchTopPadding, false, false);
            AssertValidProperty(properties, PropertyNames.ImageSearchBottomPadding, false, false);

            AssertValidProperty(properties, PropertyNames.RelativeParent, false, false);
            AssertValidProperty(properties, PropertyNames.ColourTolerance, true, false);
            AssertValidProperty(properties, PropertyNames.Greyscale, true, false);
        }

        [Test]
        public void GetProperties_ImageRelativeToParent_ShouldEnableValidProperties()
        {
            var region = CreateRegion();
            var descriptor = CreateDescriptorUsingProvider(region);

            region.LocationMethod = RegionLocationMethod.Image;
            region.RegionPosition = RegionPosition.Relative;
            var properties = descriptor.GetProperties();

            AssertValidProperty(properties, PropertyNames.RetainImage, true, true);
            AssertValidProperty(properties, PropertyNames.RegionPosition, true, false);
            AssertValidProperty(properties, PropertyNames.ImageSearchPadding, false, false);

            AssertValidProperty(properties, PropertyNames.ImageSearchAllPadding, true, false);
            AssertValidProperty(properties, PropertyNames.ImageSearchLeftPadding, true, false);
            AssertValidProperty(properties, PropertyNames.ImageSearchRightPadding, true, false);
            AssertValidProperty(properties, PropertyNames.ImageSearchTopPadding, true, false);
            AssertValidProperty(properties, PropertyNames.ImageSearchBottomPadding, true, false);

            AssertValidProperty(properties, PropertyNames.RelativeParent, true, false);
            AssertValidProperty(properties, PropertyNames.ColourTolerance, true, false);
            AssertValidProperty(properties, PropertyNames.Greyscale, true, false);
        }

        [Test]
        public void GetProperties_CoordinatesFixed_ShouldEnableValidProperties()
        {
            var region = CreateRegion();
            var descriptor = CreateDescriptorUsingProvider(region);

            region.LocationMethod = RegionLocationMethod.Coordinates;
            var properties = descriptor.GetProperties();

            AssertValidProperty(properties, PropertyNames.RetainImage, true, false);
            AssertValidProperty(properties, PropertyNames.RegionPosition, true, false);
            AssertValidProperty(properties, PropertyNames.ImageSearchPadding, false, false);

            AssertValidProperty(properties, PropertyNames.ImageSearchAllPadding, false, false);
            AssertValidProperty(properties, PropertyNames.ImageSearchLeftPadding, false, false);
            AssertValidProperty(properties, PropertyNames.ImageSearchRightPadding, false, false);
            AssertValidProperty(properties, PropertyNames.ImageSearchTopPadding, false, false);
            AssertValidProperty(properties, PropertyNames.ImageSearchBottomPadding, false, false);

            AssertValidProperty(properties, PropertyNames.RelativeParent, false, false);
            AssertValidProperty(properties, PropertyNames.ColourTolerance, false, false);
            AssertValidProperty(properties, PropertyNames.Greyscale, false, false);
        }

        [Test]
        public void GetProperties_CoordinatesRelativeToParent_ShouldEnableValidProperties()
        {
            var region = CreateRegion();
            var descriptor = CreateDescriptorUsingProvider(region);

            region.LocationMethod = RegionLocationMethod.Coordinates;
            region.RegionPosition = RegionPosition.Relative;
            var properties = descriptor.GetProperties();

            AssertValidProperty(properties, PropertyNames.RetainImage, true, false);
            AssertValidProperty(properties, PropertyNames.RegionPosition, true, false);
            AssertValidProperty(properties, PropertyNames.ImageSearchPadding, false, false);

            AssertValidProperty(properties, PropertyNames.ImageSearchAllPadding, false, false);
            AssertValidProperty(properties, PropertyNames.ImageSearchLeftPadding, false, false);
            AssertValidProperty(properties, PropertyNames.ImageSearchRightPadding, false, false);
            AssertValidProperty(properties, PropertyNames.ImageSearchTopPadding, false, false);
            AssertValidProperty(properties, PropertyNames.ImageSearchBottomPadding, false, false);

            AssertValidProperty(properties, PropertyNames.RelativeParent, true, false);
            AssertValidProperty(properties, PropertyNames.ColourTolerance, false, false);
            AssertValidProperty(properties, PropertyNames.Greyscale, false, false);
        }

        [Test]
        public void GetProperties_ListRegionFixed_ShouldEnableValidProperties()
        {
            var region = CreateListRegion();
            var descriptor = CreateDescriptorUsingProvider(region);

            var properties = descriptor.GetProperties();

            AssertValidPropertiesForListOrGrid(properties);
            AssertValidProperty(properties, PropertyNames.RelativeParent, false, false);
        }

        [Test]
        public void GetProperties_ListRegionRelativeToParent_ShouldEnableValidProperties()
        {
            var region = CreateListRegion();
            region.RegionPosition = RegionPosition.Relative;
            var descriptor = CreateDescriptorUsingProvider(region);

            var properties = descriptor.GetProperties();

            AssertValidPropertiesForListOrGrid(properties);
            AssertValidProperty(properties, PropertyNames.RelativeParent, true, false);
        }
        
        [TestCase(-10, 0)]
        [TestCase(5, 5)]
        [TestCase(300, 255)]
        public void GetProperties_ImageWithColourTolerance_ShouldConstrainValue(int original, int expected)
        {
            var region = CreateRegion();
            var descriptor = CreateDescriptorUsingProvider(region);

            var properties = descriptor.GetProperties();

            region.LocationMethod = RegionLocationMethod.Image;
            properties[PropertyNames.ColourTolerance].SetValue(descriptor, original);

            Assert.That(properties[PropertyNames.ColourTolerance].GetValue(descriptor), Is.EqualTo(expected));
        }
       
        private void AssertValidPropertiesForListOrGrid(PropertyDescriptorCollection properties)
        {
            AssertValidProperty(properties, PropertyNames.LocationMethod, false, false);
            AssertValidProperty(properties, PropertyNames.RegionPosition, true, false);
            AssertValidProperty(properties, PropertyNames.ImageSearchPadding, false, false);

            AssertValidProperty(properties, PropertyNames.ImageSearchAllPadding, false, false);
            AssertValidProperty(properties, PropertyNames.ImageSearchLeftPadding, false, false);
            AssertValidProperty(properties, PropertyNames.ImageSearchRightPadding, false, false);
            AssertValidProperty(properties, PropertyNames.ImageSearchTopPadding, false, false);
            AssertValidProperty(properties, PropertyNames.ImageSearchBottomPadding, false, false);

            AssertValidProperty(properties, PropertyNames.ColourTolerance, false, false);
            AssertValidProperty(properties, PropertyNames.Greyscale, false, false);
        }

        private void AssertValidProperty(PropertyDescriptorCollection properties, string name, bool browsable, bool readOnly)
        {
            var property = properties[name];
            Assert.That(property, Is.Not.Null);
            Assert.That(property.IsBrowsable, Is.EqualTo(browsable), "Unexpected IsBrowsable value for property " + name);
            Assert.That(property.IsReadOnly, Is.EqualTo(readOnly), "Unexpected IsReadOnly value for property " + name);
            var browsableAttribute = (BrowsableAttribute) property.Attributes[typeof(BrowsableAttribute)];
            Assert.That(browsableAttribute.Browsable, Is.EqualTo(browsable), "Unexpected Browsable value on BrowsableAttribute for property " + name);
        }

        private static SpyRegion CreateRegion()
        {
            var container = Mock.Of<ISpyRegionContainer>();
            var region = new SpyRegion(container, new Rectangle(0, 0, 20, 20), TestRegionName);
            return region;
        }
        
        private static SpyRegion CreateListRegion()
        {
            var container = Mock.Of<ISpyRegionContainer>();
            var region = new ListSpyRegion(container, new Rectangle(0, 0, 20, 20), TestRegionName);
            return region;
        }

        private static ICustomTypeDescriptor CreateDescriptorUsingProvider(SpyRegion region)
        {
            var provider = new SpyRegionTypeDescriptorProvider();
            var descriptor = provider.GetTypeDescriptor(region);
            return descriptor;
        }
    }
}

#endif