using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BluePrism.AutomateProcessCore;
using BluePrism.CharMatching.UI;
using Moq;
using NUnit.Framework;

namespace AutomateUI.UnitTests
{
    [TestFixture]
    public class RegionConvertTests
    {
        private SpyRegion _region1;
        private SpyRegion _region2;
        private SpyRegion _region3;
        private SpyRegion _region4;
        private List<clsApplicationMember> _children;
        private HashSet<SpyRegion> _processed;
        private Mock<ISpyRegionContainer> _mapperMock;
        private Bitmap _screenShot;
        private clsRegionContainer _appRegContainer;

        [SetUp]
        public void SetUp()
        {
            _children = new List<clsApplicationMember>();
            _processed = new HashSet<SpyRegion>();
            _appRegContainer = new clsRegionContainer("testAppRegions");
            _mapperMock = new Mock<ISpyRegionContainer>();
            _region1 = new SpyRegion(_mapperMock.Object, new Rectangle(0, 0, 20, 20), "Region 1");
            _region2 = new SpyRegion(_mapperMock.Object, new Rectangle(0, 20, 20, 40), "Region 2");
            _region3 = new SpyRegion(_mapperMock.Object, new Rectangle(0, 40, 20, 60), "Region 3");
            _region4 = new SpyRegion(_mapperMock.Object, new Rectangle(10, 20, 20, 60), "Region 4");
            var all = new List<SpyRegion>(new SpyRegion[] { _region1, _region2, _region3, _region4 });
            _screenShot = new Bitmap(5, 5);
            _mapperMock.SetupGet(x => x.SpyRegions).Returns(all);
        }

        [TearDown]
        public void CleanUp()
        {
            _screenShot.Dispose();
        }

        [Test]
        public void ShouldConvertSingleSpyRegionToAppRegionAndBack()
        {
            // Convert one region to an app region and then back to check that the 
            // properties are retained
            _region1.ColourTolerance = 25;
            _region1.LocationMethod = RegionLocationMethod.Coordinates;
            _region1.ImageSearchPadding = new Padding(10, 10, 5, 6);
            _region1.RetainImage = true;
            var appReg = clsRegionConvert.ConvertToAppRegion(_region1, _appRegContainer, _screenShot, _children, _processed);

            var result = clsRegionConvert.ToSpyRegion(_mapperMock.Object, appReg);
            Assert.That(result.Name, Is.EqualTo(_region1.Name));
            Assert.That(result.Id, Is.EqualTo(appReg.ID));
            Assert.That(result.Rectangle, Is.EqualTo(_region1.Rectangle));
            Assert.That(result.ColourTolerance, Is.EqualTo(25));
            Assert.That(result.LocationMethod, Is.EqualTo(_region1.LocationMethod));
            Assert.That(result.ImageSearchPadding, Is.EqualTo(_region1.ImageSearchPadding));
            Assert.That(result.RetainImage, Is.EqualTo(_region1.RetainImage));
        }

        [Test]
        public void ShouldConvertCollectionOfAppRegionsToSpyRegions()
        {
            var appReg1 = clsRegionConvert.ConvertToAppRegion(_region1, _appRegContainer, _screenShot, _children, _processed);
            var appReg2 = clsRegionConvert.ConvertToAppRegion(_region2, _appRegContainer, _screenShot, _children, _processed);

            var appReg3 = clsRegionConvert.ConvertToAppRegion(_region3, _appRegContainer, _screenShot, _children, _processed);

            var regions = new HashSet<clsApplicationRegion>(new[] { appReg1, appReg2, appReg3 });
            var result = clsRegionConvert.ToSpyRegions(_mapperMock.Object, regions);

            // Make sure the collection of SpyRegions returned is as expected
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result.FirstOrDefault(x => x.Id.Equals(appReg1.ID)), Is.Not.Null);
            Assert.That(result.FirstOrDefault(x => x.Id.Equals(appReg2.ID)), Is.Not.Null);
            Assert.That(result.FirstOrDefault(x => x.Id.Equals(appReg3.ID)), Is.Not.Null);
        }

        /// <summary>
        /// On converting SpyRegions to AppRegions and vice versa, to set the relative
        /// parent of a region it is necessary to convert that parent region first before
        /// it can be set as a parent region (otherwise the ParentRegionId will be
        /// Guid.Empty.
        /// This test checks that the conversion is performed correctly in both directions.
        /// </summary>
        [Test]
        public void ShouldSetRelativeParentsRecursively()
        {
            // set up relative region relationships which will fail the test 
            // if the parents are not set recursively

            // region1 
            // /\
            // /  _region2  
            // region4    \
            // region3

            _region3.RelativeParent = _region2;
            _region2.RelativeParent = _region1;
            _region4.RelativeParent = _region1;

            // Convert the spy regions to app regions (as occurs in frmIntegrationAssistant)
            foreach (var reg in _mapperMock.Object.SpyRegions)
            {
                if (!_processed.Contains(reg))
                    clsRegionConvert.ConvertToAppRegion(reg, _appRegContainer, _screenShot, _children, _processed);
            }

            // ...and back to spyregions (as occurs when opening back up the mapper)
            clsRegionConvert.ToSpyRegions(_mapperMock.Object, _appRegContainer.Regions);

            // check that the relative parents have been set correctly on the 3 regions
            foreach (var reg in new[] { _region2, _region4, _region3 })
                Assert.That(reg.RelativeParent.Id, Is.Not.EqualTo(Guid.Empty));

            // Check that the correct Guid has been set
            Assert.That(_region2.RelativeParent.Id, Is.EqualTo(_region1.Id));
            Assert.That(_region3.RelativeParent.Id, Is.EqualTo(_region2.Id));
            Assert.That(_region4.RelativeParent.Id, Is.EqualTo(_region1.Id));
        }

        [Test]
        public void ConvertToAppRegion_AmountOfRegionsToProcess_IsEqualToChildrenCount()
        {
            _region3.RelativeParent = _region2;
            _region2.RelativeParent = _region1;
            _region4.RelativeParent = _region1;
            foreach (var reg in _mapperMock.Object.SpyRegions)
            {
                if (!_processed.Contains(reg))
                {
                    clsRegionConvert.ConvertToAppRegion(reg, _appRegContainer, _screenShot, _children, _processed);
                }
            }

            Assert.IsTrue(_children.Count == _mapperMock.Object.SpyRegions.Count);
        }
    }
}
