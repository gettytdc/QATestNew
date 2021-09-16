#if UNITTESTS

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Threading;
using BluePrism.CharMatching.UI;
using Moq;
using NUnit.Framework;

namespace BluePrism.CharMatching.UnitTests.UI
{
    public class RelativeSpyRegionTypeConverterTests
    {
        private SpyRegion region1;
        private SpyRegion region2;
        private SpyRegion region3;
        private static readonly CultureInfo TestCulture = Thread.CurrentThread.CurrentCulture;

        [SetUp]
        public void Setup()
        {
            var containerMock = new Mock<ISpyRegionContainer>();
            region1 = new SpyRegion(containerMock.Object, new Rectangle(0, 0, 20, 20), "Region 1");
            region2 = new SpyRegion(containerMock.Object, new Rectangle(0, 20, 20, 40), "Region 2");
            region3 = new SpyRegion(containerMock.Object, new Rectangle(0, 40, 20, 60), "Region 3");
            var all = new List<SpyRegion> {region1, region2, region3};
            containerMock.SetupGet(x => x.SpyRegions).Returns(all);
        }

        [Test]
        public void ShouldIncludeEmptyOptionAsStandardValue()
        {
            var converter = new RelativeSpyRegionTypeConverter();
            var context = new TestTypeDescriptorContext { Instance = region1 };

            var standardValues = converter.GetStandardValues(context);

            Assert.That(standardValues[0], Is.EqualTo("<No Region Selected>"));
        }

        [Test]
        public void ShouldGetOtherRegionNamesAsStandardValues()
        {
            var converter = new RelativeSpyRegionTypeConverter();
            var context = new TestTypeDescriptorContext {Instance = region1};

            var standardValues = converter.GetStandardValues(context);

            Assert.That(standardValues, Is.EquivalentTo(new[] { "<No Region Selected>", "Region 2", "Region 3" }));
        }

        [Test]
        public void ShouldConvertFromEmptyStringValueToNull()
        {
            var converter = new RelativeSpyRegionTypeConverter();
            var context = new TestTypeDescriptorContext { Instance = region1 };

            var result = converter.ConvertFrom(context, TestCulture, "<No Region Selected>");

            Assert.That(result, Is.Null);
        }

        [Test]
        public void ShouldConvertFromStringToRegionWithMatchingName()
        {
            var converter = new RelativeSpyRegionTypeConverter();
            var context = new TestTypeDescriptorContext { Instance = region1 };

            var result = converter.ConvertFrom(context, TestCulture, region2.Name);

            Assert.That(result, Is.EqualTo(region2));
        }

        [Test]
        public void ShouldNotAllowCircularPaths()
        {
            var converter = new RelativeSpyRegionTypeConverter();
            var context = new TestTypeDescriptorContext { Instance = region1 };
            region3.RelativeParent = region2;
            region2.RelativeParent = region1;
            var result = converter.IsValid(context, region1);
            Assert.That(result, Is.False);

            result = converter.IsValid(context, region2);
            Assert.That(result, Is.False);

            result = converter.IsValid(context, region3);
            Assert.That(result, Is.False);

            region3.RelativeParent = null;
            result = converter.IsValid(context, region3);
            Assert.That(result, Is.True);

            region3.RelativeParent = region1;
            result = converter.IsValid(context,region3);
            Assert.That(result, Is.False);
        }

        [Test]
        public void ShouldNotIncludeCircularPathsInStandardValues()
        {
            var converter = new RelativeSpyRegionTypeConverter();
            var context = new TestTypeDescriptorContext { Instance = region1 };
            region3.RelativeParent = region2;
            region2.RelativeParent = region1;
            // there are now no regions which would not create a circular path from region1
            string[] expectedResult = new string[1] { "<No Region Selected>" };
            RelativeSpyRegionTypeConverter.StandardValuesCollection result = converter.GetStandardValues(context);
            Assert.That(result, Is.EqualTo(expectedResult)); 
        }

            /// <summary>
        /// Test ITypeDescriptorContext implementation - just implements the members used
        /// by RelativeSpyRegionTypeConverter.
        /// </summary>
        private class TestTypeDescriptorContext : ITypeDescriptorContext
        {
            public object GetService(Type serviceType)
            {
                throw new NotImplementedException();
            }

            public bool OnComponentChanging()
            {
                throw new NotImplementedException();
            }

            public void OnComponentChanged()
            {
                throw new NotImplementedException();
            }

            public IContainer Container { get; set; }
            public object Instance { get; set; }
            public PropertyDescriptor PropertyDescriptor { get; private set; }
        }
    }
}

#endif