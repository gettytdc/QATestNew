using System.Drawing;
using System.Text.RegularExpressions;
using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandlers.UIAutomation;
using ClientComms.UnitTests.CommandHandlers.UIAutomation.Shared;
using NUnit.Framework;

namespace ClientComms.UnitTests.CommandHandlers.UIAutomation
{
    internal class GetRelativeElementBoundsHandlerTests : UIAutomationHandlerTestBase<GetRelativeElementBoundsHandler>
    {
        private clsQuery Query
        {
            get
            {
                return clsQuery.Parse("UIAGetRelativeElementBounds");
            }
        }

        [Test]
        public void Execute_ReturnsXML()
        {
            var rectangle = new Rectangle(1, 2, 20, 40);
            ElementMock.Setup(e => e.GetCurrentBoundingRelativeClientRectangle()).Returns(rectangle);
            var result = Execute(Query);


            var expectedResult = $@"<collection>
                                        <row>
                                            <field name=""Left"" type=""number"" value=""{rectangle.Left}"" />
                                            <field name=""Top"" type=""number"" value=""{rectangle.Top}"" />
                                            <field name= ""Bottom"" type = ""number"" value=""{rectangle.Bottom}"" />
                                            <field name= ""Right"" type = ""number"" value=""{rectangle.Right}"" />
                                            <field name= ""Width"" type = ""number"" value=""{rectangle.Width}"" />
                                            <field name= ""Height"" type = ""number"" value=""{rectangle.Height}"" />
                                        </row>
                                    </collection>";

            Assert.AreEqual(Regex.Replace(result.Message, @"\s+", ""), Regex.Replace(expectedResult.ToString(), @"\s+", ""));
        }
    }
}