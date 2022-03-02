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
    public class RegionPositionTypeConverterTests
    {
        private SpyRegion _region;
        private static readonly CultureInfo TestCulture = Thread.CurrentThread.CurrentCulture;

        [SetUp]
        public void Setup()
        {
            var containerMock = new Mock<ISpyRegionContainer>();
            _region = new SpyRegion(containerMock.Object, new Rectangle(0, 0, 20, 20), "Test Region");
            var all = new List<SpyRegion> { _region };
            containerMock.SetupGet(x => x.SpyRegions).Returns(all);
        }

    
        [Test]
        public void ShouldGetAllPositionsWhenRegionLocationMethodIsImage()
        {
            var converter = new RegionPositionTypeConverter();
            _region.LocationMethod = RegionLocationMethod.Image;
            var context = new TestTypeDescriptorContext {Instance = _region};
            var standardValues = converter.GetStandardValues(context);
            var expectedValues = new[] {
                    RegionPosition.Anywhere, 
                    RegionPosition.Fixed, 
                    RegionPosition.Relative} ;
                        
            Assert.That(standardValues, Is.EquivalentTo(expectedValues));
        }

        [Test]
        public void ShouldNotReturnAnywhereWhenRegionLocationMethodIsCoordinates()
        {
            var converter = new RegionPositionTypeConverter();
            _region.LocationMethod = RegionLocationMethod.Coordinates;
            var context = new TestTypeDescriptorContext { Instance = _region };
            var standardValues = converter.GetStandardValues(context);
            var expectedValues = new[] { 
                    RegionPosition.Fixed, 
                    RegionPosition.Relative};

            Assert.That(standardValues, Is.EquivalentTo(expectedValues));
        }

        [Test]
        public void ShouldConvertFromStringToPositionWithMatchingName()
        {
            var converter = new RegionPositionTypeConverter();
                    
            var context = new TestTypeDescriptorContext { Instance = _region };
            var result = converter.ConvertFrom(context, TestCulture, "Fixed");

            var expectedResult = RegionPosition.Fixed;

            Assert.That(result, Is.EqualTo(expectedResult));
        }

        /// <summary>
        /// Test ITypeDescriptorContext implementation - just implements the members used
        /// by RegionPositionTypeConverter.
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