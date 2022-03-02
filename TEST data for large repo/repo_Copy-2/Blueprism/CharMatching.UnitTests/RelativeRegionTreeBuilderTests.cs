#if UNITTESTS

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Moq;
using BluePrism.CharMatching.UI;
using System.Drawing;

namespace BluePrism.CharMatching.UnitTests
{
    public class RelativeRegionTreeBuilderTests
    {
        private static Mock<ISpyRegionContainer> containerMock;
        private static SpyRegion regionA;
        private static SpyRegion regionB;
        private static SpyRegion regionC;
        private static SpyRegion regionD;
        private static SpyRegion regionE;
        private static SpyRegion regionF;
        private static SpyRegion regionG;
        private static SpyRegion regionH;
        private static SpyRegion regionI;

        private static Line2D[] tree1ExpectedLines;
        private static Line2D[] tree2ExpectedLines;
        private static Line2D[] tree3ExpectedLines;
        
        static RelativeRegionTreeBuilderTests()
        {
            // Setup a spy region container with a number of relative regions.
            // These regions can be split up into three relative region trees:
            //
            //   Tree 1          Tree 2         Tree 3
            //
            //     A               G              I
            //    / \             / 
            //   B   C           H 
            //        \
            //         D
            //        / \
            //       E   F
            //
            // Due to the one to many relationship, each region can only appear in one tree

            //Create the container and all spy regions
            containerMock = new Mock<ISpyRegionContainer>();
            regionA = new SpyRegion(containerMock.Object, new Rectangle(5, 10, 20, 20), "Region A");
            regionB = new SpyRegion(containerMock.Object, new Rectangle(12, 40, 50, 30), "Region B");
            regionC = new SpyRegion(containerMock.Object, new Rectangle(0, 40, 120, 10), "Region C");
            regionD = new SpyRegion(containerMock.Object, new Rectangle(100, 500, 5, 40), "Region D");
            regionE = new SpyRegion(containerMock.Object, new Rectangle(200, 20, 20, 60), "Region E");
            regionF = new SpyRegion(containerMock.Object, new Rectangle(65, 340, 80, 10), "Region F");
            regionG = new SpyRegion(containerMock.Object, new Rectangle(10, 400, 120, 15), "Region G");
            regionH = new SpyRegion(containerMock.Object, new Rectangle(400, 100, 10, 10), "Region H");
            regionI = new SpyRegion(containerMock.Object, new Rectangle(15, 200, 8, 48), "Region I");


            //Set up the spy region relative relationships as described in diagram above
            regionB.RegionPosition = RegionPosition.Relative;
            regionB.RelativeParent = regionA;

            regionC.RegionPosition = RegionPosition.Relative;
            regionC.RelativeParent = regionA;

            regionD.RegionPosition = RegionPosition.Relative;
            regionD.RelativeParent = regionC;

            regionE.RegionPosition = RegionPosition.Relative;
            regionE.RelativeParent = regionD;

            regionF.RegionPosition = RegionPosition.Relative;
            regionF.RelativeParent = regionD;

            regionH.RegionPosition = RegionPosition.Relative;
            regionH.RelativeParent = regionG;


            // Create an array of the lines that are expected in each tree
            tree1ExpectedLines = new Line2D[] 
                { 
                    GenerateLine(regionB, regionA), 
                    GenerateLine(regionC, regionA), 
                    GenerateLine(regionD, regionC), 
                    GenerateLine(regionE, regionD),
                    GenerateLine(regionF, regionD)
                };

            tree2ExpectedLines = new Line2D[] { GenerateLine(regionH, regionG) };
            tree3ExpectedLines = new Line2D[] {};


            var all = new List<SpyRegion> { 
                regionA, regionB, regionC, 
                regionD, regionE, regionF,
                regionG, regionH, regionI};

            containerMock.SetupGet(x => x.SpyRegions).Returns(all);
        }

        public static IEnumerable<object[]> ExpectedTreeRootTestCase()
        {
            yield return new object[] { regionA, regionA};
            yield return new object[] { regionB, regionA};
            yield return new object[] { regionC, regionA};
            yield return new object[] { regionD, regionA};
            yield return new object[] { regionE, regionA};
            yield return new object[] { regionF, regionA};
            yield return new object[] { regionG, regionG};
            yield return new object[] { regionH, regionG};
            yield return new object[] { regionI, regionI};
        }

        public static IEnumerable<object[]> ExpectedChildrenTestCase()
        {
            yield return new object[] { regionA, new SpyRegion[] { regionB, regionC } };
            yield return new object[] { regionB, new SpyRegion[]{} };
            yield return new object[] { regionC, new SpyRegion[]{ regionD} };
            yield return new object[] { regionD, new SpyRegion[]{ regionE, regionF } };
            yield return new object[] { regionE, new SpyRegion[]{} };
            yield return new object[] { regionF, new SpyRegion[]{} };
            yield return new object[] { regionG, new SpyRegion[] { regionH } };
            yield return new object[] { regionH, new SpyRegion[]{} };
            yield return new object[] { regionI, new SpyRegion[]{} };
        }


        public static IEnumerable<object[]> ExpectedLinesTestCase()
        {
            yield return new object[] { regionA, tree1ExpectedLines };
            yield return new object[] { regionB, tree1ExpectedLines };
            yield return new object[] { regionC, tree1ExpectedLines };
            yield return new object[] { regionD, tree1ExpectedLines };
            yield return new object[] { regionE, tree1ExpectedLines };
            yield return new object[] { regionF, tree1ExpectedLines };
            yield return new object[] { regionG, tree2ExpectedLines };
            yield return new object[] { regionH, tree2ExpectedLines };
            yield return new object[] { regionI, tree3ExpectedLines };
        }

        
        [TestCaseSource("ExpectedTreeRootTestCase")]
        public void ShouldReturnTheExpectedTreeRoot(SpyRegion region, SpyRegion expectedRoot)
        {
            var treeBuilder = new RelativeRegionTreeBuilder(region, region.Container.SpyRegions);
            Assert.That(treeBuilder.Build().Region, Iz.EqualTo(expectedRoot));
        }

        [TestCaseSource("ExpectedChildrenTestCase")]
        public void ShouldReturnTheExpectedChildren(SpyRegion region, params SpyRegion[] expectedChildren)
        {
            var treeBuilder = new RelativeRegionTreeBuilder(region, region.Container.SpyRegions);
            var treeRoot = treeBuilder.Build();
            var originalRegion = FindRegion(treeRoot, region);
            var returnedChildren = originalRegion.Children.Select(x => x.Region).ToArray();

            Assert.That(returnedChildren, Is.EquivalentTo(expectedChildren));
        }
        
        [TestCaseSource("ExpectedLinesTestCase")]
        public void ShouldReturnTheExpectedLines(SpyRegion region, params Line2D[] expectedLines)
        {
            var treeBuilder = new RelativeRegionTreeBuilder(region, region.Container.SpyRegions);
            var treeRoot = treeBuilder.Build();
            var lines = treeRoot.GetLines().ToArray();

            Assert.That(lines, Is.EquivalentTo(expectedLines));
        }
        

        #region Helper Methods

        /// <summary>
        /// After a tree is built, finds and returns a region within that tree
        /// </summary>
        /// <param name="searchStart">The place to start searching the tree</param>
        /// <param name="regionToFind">The region to find</param>
        /// <returns>A region within a tree</returns>
        private RelativeRegionComposite FindRegion(RelativeRegionComposite searchStart, SpyRegion regionToFind)
        {
            if (searchStart.Region == regionToFind) return searchStart;

            foreach (RelativeRegionComposite child in searchStart.Children)
            {
                var region = FindRegion(child, regionToFind);
                if (region != null) return region;
            }

            return null;
        
        }                       
        /// <summary>
        /// Generates a line object between the top left hand corner of two regions
        /// </summary>
        /// <param name="StartRegion">The region where the lines starts</param>
        /// <param name="EndRegion">The region where the lines ends</param>
        /// <returns></returns>
        private static Line2D GenerateLine(SpyRegion StartRegion, SpyRegion EndRegion)
        {
            return new Line2D(StartRegion.Rectangle.Location, EndRegion.Rectangle.Location);
        }

        #endregion

    }
}

#endif