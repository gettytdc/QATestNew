using FluentAssertions;
using NUnit.Framework;
namespace AutomateUI.UnitTests
{

    [TestFixture]
    public partial class ConfirmDatabaseConversionTests
    {
        [Test]
        public void Constructor_SetsConfirmationCode()
        {
            var frm1 = new frmConfirmDatabaseConversion();
            var confirmationCode = frm1.ConfirmationCode;
            confirmationCode.Should().BeGreaterOrEqualTo(1000);
            confirmationCode.Should().BeLessOrEqualTo(9999);
            Assert.That(confirmationCode.ToString() == frm1.lblRandomNumber.Text.ToString());
        }
    }
}

