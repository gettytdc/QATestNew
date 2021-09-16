using System.Text;

namespace AutomateC.UnitTests.AuthenticationServerUserMapping
{
    public class TestCsvBuilder
    {
        private readonly StringBuilder _csv;

        public TestCsvBuilder()
        {
            _csv = new StringBuilder();            
        }

        public TestCsvBuilder AddImportHeader()
        {
            _csv.AppendLine("BPE Username,BPC Username,BPC User Id,Auth Type,First Name,Last Name,Email");
            return this;
        }

        public TestCsvBuilder AddLine(string bpeUserName, string bpcUserId, string firstName, string lastName, string email)
        {
            _csv.AppendLine($"{bpeUserName},{bpcUserId},{firstName},{lastName},{email}");
            return this;
        }

        public TestCsvBuilder AddOutputLine(string bpeUserName, string bpcUserId, string firstName, string lastName, string email, string errorMessage)
        {
            _csv.AppendLine($"{bpeUserName},{bpcUserId},{firstName},{lastName},{email},{errorMessage}");
            return this;
        }

        public string Build() => _csv.ToString();

    }
}
