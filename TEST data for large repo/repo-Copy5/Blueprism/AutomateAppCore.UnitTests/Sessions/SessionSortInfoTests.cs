#if UNITTESTS
using BluePrism.AutomateAppCore.clsServerPartialClasses.Sessions;
using FluentAssertions;
using NUnit.Framework;
using System.Xml;

namespace AutomateAppCore.UnitTests.Sessions
{
    public class SessionSortInfoTests
    {
        [Test]
        public void GetDefaultSortInfo_ReturnsExpectedResult()
        {
            var result = SessionSortInfo.GetDefaultSortInfo();
            var expectedResult = new SessionSortInfo(SessionManagementColumn.StartTime, 
                SessionSortInfo.SortDirection.Descending);
            result.ShouldBeEquivalentTo(expectedResult);
        }

        [Test]
        public void XmlElementRoundTrip_ReturnsExpectedResult()
        {
            var sortInfo = new SessionSortInfo(SessionManagementColumn.LastStage, SessionSortInfo.SortDirection.Ascending);
            var roundTrip = SessionSortInfo.FromXmlElement(sortInfo.ToXmlElement(new XmlDocument()));
            roundTrip.ShouldBeEquivalentTo(sortInfo);
        }

        [Test]
        public void FromXmlElement_MissingColumnAttribute_ShouldApplyDefault()
        {
            var xmlDocument = new XmlDocument();
            var xmlElement = xmlDocument.CreateElement("sortinfo");
            xmlElement.SetAttribute("direction", SessionSortInfo.SortDirection.Ascending.ToString());
            var result = SessionSortInfo.FromXmlElement(xmlElement);
            result.ShouldBeEquivalentTo(new SessionSortInfo(SessionManagementColumn.StartTime, 
                SessionSortInfo.SortDirection.Ascending));
        }

        [Test]
        public void FromXmlElement_MissingDirectionAttribute_ShouldApplyDefault()
        {
            var xmlDocument = new XmlDocument();
            var xmlElement = xmlDocument.CreateElement("sortinfo");
            xmlElement.SetAttribute("column", SessionManagementColumn.EndTime.ToString());
            var result = SessionSortInfo.FromXmlElement(xmlElement);
            result.ShouldBeEquivalentTo(new SessionSortInfo(SessionManagementColumn.EndTime, 
                SessionSortInfo.SortDirection.Descending));
        }
    }
}
#endif