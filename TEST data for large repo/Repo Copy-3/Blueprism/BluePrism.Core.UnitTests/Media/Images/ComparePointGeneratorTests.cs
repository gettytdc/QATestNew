#if UNITTESTS

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using BluePrism.Core.Media.Images;
using System.Drawing;

namespace BluePrism.Core.UnitTests.Media.Images
{
    /// <summary>
    /// Tests any classes implementing the IComparePointGenerator interface
    /// </summary>
    public class ComparePointGeneratorTests
    {

        public static IEnumerable<int[]> GetRectangleDimensionTestCases()
        {
            yield return new[] {1, 1};
            yield return new[] {1, 2};
            yield return new[] {2, 1};
            yield return new[] {2, 3};
            yield return new[] {6, 7};
            yield return new[] {150, 800};
            yield return new[] {1000, 250};
        }

        public static IEnumerable<Point[]> GetValidRectangleTestCases()
        {
            yield return new[] { 
                                    new Point(0, 0), new Point(1, 0), new Point(2, 0),
                                    new Point(0, 1), new Point(1, 1), new Point(2, 1) 
                                };

            yield return new[] { 
                                    new Point(0, 0), new Point(1, 0),
                                    new Point(0, 1), new Point(1, 1), 
                                    new Point(0, 2), new Point(1, 2),
                                    new Point(0, 3), new Point(1, 3),
                                    new Point(0, 4), new Point(1, 4),
                                    new Point(0, 5), new Point(1, 5),
                                    new Point(0, 6), new Point(1, 6),
                                    new Point(0, 7), new Point(1, 7),
                                    new Point(0, 8), new Point(1, 8)
                                };

            yield return new[] { 
                                    new Point(0, 0), new Point(1, 0), new Point(2, 0),
                                    new Point(0, 1), new Point(1, 1), new Point(2, 1),
                                    new Point(0, 2), new Point(1, 2), new Point(2, 2),
                                    new Point(0, 3), new Point(1, 3), new Point(2, 3),
                                    new Point(0, 4), new Point(1, 4), new Point(2, 4)
                                };

            yield return new[] { 
                                    new Point(0, 0), new Point(1, 0), new Point(2, 0), new Point(3, 0), new Point(4, 0),
                                    new Point(0, 1), new Point(1, 1), new Point(2, 1), new Point(3, 1), new Point(4, 1),
                                    new Point(0, 2), new Point(1, 2), new Point(2, 2), new Point(3, 2), new Point(4, 2)
                                };

            yield return new[] { 
                                    new Point(0, 0), new Point(1, 0), new Point(2, 0), new Point(3, 0),
                                    new Point(0, 1), new Point(1, 1), new Point(2, 1), new Point(3, 1),
                                    new Point(0, 2), new Point(1, 2), new Point(2, 2), new Point(3, 2),
                                    new Point(0, 3), new Point(1, 3), new Point(2, 3), new Point(3, 3),
                                    new Point(0, 4), new Point(1, 4), new Point(2, 4), new Point(3, 4)
                                };
        }



        [TestCaseSource("GetRectangleDimensionTestCases")]
        public void TestThatDefaultGeneratorShouldReturnTheCorrectNumberOfPoints(int width, int height)
        {
            var generator = new DefaultComparePointGenerator();
            var numOfPointsGenerated = generator.GetComparePoints(width, height).Count();

            Assert.That(numOfPointsGenerated, Is.EqualTo(width * height));

        }

        [TestCaseSource("GetRectangleDimensionTestCases")]
        public void TestThatSpiralGeneratorShouldReturnTheCorrectNumberOfPoints(int width, int height)
        {
            var generator = new SpiralComparePointGenerator();
            var numOfPointsGenerated = generator.GetComparePoints(width, height).Count();

            Assert.That(numOfPointsGenerated, Is.EqualTo(width * height));

        }
        

        [TestCaseSource("GetValidRectangleTestCases")]
        public void TestThatDefaultGeneratorShouldReturnTheCorrectPoints(IEnumerable<Point> expectedPoints)
        {
            var generator = new DefaultComparePointGenerator();
            var rectWidth = expectedPoints.Last().X + 1;
            var rectHeight = expectedPoints.Last().Y + 1;

            var generatedPoints = generator.GetComparePoints(rectWidth, rectHeight);

            Assert.That(new HashSet<Point>(expectedPoints).SetEquals(generatedPoints));
            
        }

        [TestCaseSource("GetValidRectangleTestCases")]
        public void TestThatSpiralGeneratorShouldReturnTheCorrectPoints(IEnumerable<Point> expectedPoints)
        {
            var generator = new SpiralComparePointGenerator();
            var rectWidth = expectedPoints.Last().X + 1;
            var rectHeight = expectedPoints.Last().Y + 1;

            var generatedPoints = generator.GetComparePoints(rectWidth, rectHeight);

            Assert.That(new HashSet<Point>(expectedPoints).SetEquals(generatedPoints));
            
        }

    }
}

#endif