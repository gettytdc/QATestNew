#if UNITTESTS

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using BluePrism.Core.Media.Images;
using System.Drawing;

namespace BluePrism.Core.UnitTests.Media.Images
{
    /// <summary>
    /// Tests any classes implementing the IStartPointGenerator interface
    /// </summary>
    public class StartPointGeneratorTests
    {
        /// <summary>
        /// Class used to provide some test cases for the start point generator tests
        /// </summary>
        public class StartPointsTestCase
        {
            /// <summary>
            /// Description (used in ToString) to clarify unit test description
            /// </summary>
            public string Description;

            /// <summary>
            /// The overall container that is being searched
            /// </summary>
            public Point[] Container;

            /// <summary>
            /// Where the image was located within the container when originally captured
            /// </summary>
            public Point OriginalPosition;

            /// <summary>
            /// The height of the image being searched for
            /// </summary>
            public int ImageHeight;

            /// <summary>
            /// The width of the image being searched for
            /// </summary>
            public int ImageWidth;

            /// <summary>
            /// The possible start points within the container that image could possibly 
            /// be situated, given its size.
            /// </summary>
            public Point[] PossibleStartPoints;

            public override string ToString()
            {
                return Description;
            }
        }

        /// <summary>
        /// Function to return the various test cases. Each test case has the containing rectangle,
        /// the width of the image that is being searched inside it and the top left
        /// of where the image originally was which is used as the start point in the spiral search.
        /// It also contains all of the coordinates of the start point (top left) of where the image
        /// could possibly be within that container. This is used to check the results.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<StartPointsTestCase> GetTestCases()
        {
            yield return new StartPointsTestCase
            {
                Description = "Image same size as container",
                Container = new[] { 
                                    new Point(0, 0), new Point(1, 0), new Point(2, 0),
                                    new Point(0, 1), new Point(1, 1), new Point(2, 1) 
                                },

                OriginalPosition = new Point(0, 0),
                ImageHeight = 2,
                ImageWidth = 3,
                PossibleStartPoints = new[] {
                                                new Point(0, 0) 
                                            }
            };

            yield return new StartPointsTestCase
            {
                Description = "2x4 image within container",
                Container = new[] { 
                                    new Point(0, 0), new Point(1, 0),
                                    new Point(0, 1), new Point(1, 1), 
                                    new Point(0, 2), new Point(1, 2),
                                    new Point(0, 3), new Point(1, 3),
                                    new Point(0, 4), new Point(1, 4),
                                    new Point(0, 5), new Point(1, 5),
                                    new Point(0, 6), new Point(1, 6),
                                    new Point(0, 7), new Point(1, 7),
                                    new Point(0, 8), new Point(1, 8)
                                },

                OriginalPosition = new Point(0, 4),
                ImageHeight = 4,
                ImageWidth = 2,
                PossibleStartPoints = new[] {
                                                new Point(0, 0), 
                                                new Point(0, 1), 
                                                new Point(0, 2), 
                                                new Point(0, 3), 
                                                new Point(0, 4), 
                                                new Point(0, 5) 
                                            }
            };

            yield return new StartPointsTestCase
            {
                Description = "2x2 image within container near top-left edge",
                Container = new[] { 
                                    new Point(0, 0), new Point(1, 0), new Point(2, 0),
                                    new Point(0, 1), new Point(1, 1), new Point(2, 1),
                                    new Point(0, 2), new Point(1, 2), new Point(2, 2),
                                    new Point(0, 3), new Point(1, 3), new Point(2, 3),
                                    new Point(0, 4), new Point(1, 4), new Point(2, 4)
                                },

                OriginalPosition = new Point(0, 1),
                ImageHeight = 2,
                ImageWidth = 2,
                PossibleStartPoints = new[] {
                                                new Point(0, 0), new Point(1, 0), 
                                                new Point(0, 1), new Point(1, 1),
                                                new Point(0, 2), new Point(1, 2),
                                                new Point(0, 3), new Point(1, 3)
                                            }
            };

            yield return new StartPointsTestCase
            {
                Description = "2x2 image within container near top-right edge",
                Container =  new[] { 
                                        new Point(0, 0), new Point(1, 0), new Point(2, 0), new Point(3, 0), new Point(4, 0),
                                        new Point(0, 1), new Point(1, 1), new Point(2, 1), new Point(3, 1), new Point(4, 1),
                                        new Point(0, 2), new Point(1, 2), new Point(2, 2), new Point(3, 2), new Point(4, 2)
                                },

                OriginalPosition = new Point(3, 0),
                ImageHeight = 1,
                ImageWidth = 1,
                PossibleStartPoints = new[] { 
                                                new Point(0, 0), new Point(1, 0), new Point(2, 0), new Point(3, 0), new Point(4, 0),
                                                new Point(0, 1), new Point(1, 1), new Point(2, 1), new Point(3, 1), new Point(4, 1),
                                                new Point(0, 2), new Point(1, 2), new Point(2, 2), new Point(3, 2), new Point(4, 2)
                                            }
            };

            yield return new StartPointsTestCase
            {
                Description = "2x4 image within container near top-left edge",
                Container = new[] { 
                                    new Point(0, 0), new Point(1, 0), new Point(2, 0), new Point(3, 0),
                                    new Point(0, 1), new Point(1, 1), new Point(2, 1), new Point(3, 1),
                                    new Point(0, 2), new Point(1, 2), new Point(2, 2), new Point(3, 2),
                                    new Point(0, 3), new Point(1, 3), new Point(2, 3), new Point(3, 3),
                                    new Point(0, 4), new Point(1, 4), new Point(2, 4), new Point(3, 4)
                                },
                OriginalPosition = new Point(1, 1),
                ImageHeight = 4,
                ImageWidth = 2,
                PossibleStartPoints = new[] { 
                                                new Point(0, 0), new Point(1, 0), new Point(2, 0), 
                                                new Point(0, 1), new Point(1, 1), new Point(2, 1) 
                                            }
            };



        }


        [TestCaseSource("GetTestCases")]
        public void TestThatDefaultGeneratorShouldReturnTheCorrectPoints(StartPointsTestCase container)
        {
            var generator = new DefaultStartPointGenerator();
            var containerWidth = container.Container.Last().X + 1;
            var containerHeight = container.Container.Last().Y + 1;

            var generatedPoints = generator.GetStartPoints(new Rectangle(0, 0, containerWidth, containerHeight), 
                container.ImageWidth, container.ImageHeight, container.OriginalPosition);

            Assert.That(new HashSet<Point>(container.PossibleStartPoints).SetEquals(generatedPoints));
            Assert.That(container.PossibleStartPoints.Count(), Is.EqualTo(generatedPoints.Count()));
        }

        [TestCaseSource("GetTestCases")]
        public void TestThatSpiralGeneratorShouldReturnTheCorrectPoints(StartPointsTestCase container)
        {
            var generator = new SpiralStartPointGenerator();
            var containerWidth = container.Container.Last().X + 1;
            var containerHeight = container.Container.Last().Y + 1;

            var generatedPoints = generator.GetStartPoints(new Rectangle(0, 0, containerWidth, containerHeight),
                container.ImageWidth, container.ImageHeight, container.OriginalPosition);

            Assert.That(new HashSet<Point>(container.PossibleStartPoints).SetEquals(generatedPoints));
            Assert.That(container.PossibleStartPoints.Count(), Is.EqualTo(generatedPoints.Count()));
        }


    }
}

#endif