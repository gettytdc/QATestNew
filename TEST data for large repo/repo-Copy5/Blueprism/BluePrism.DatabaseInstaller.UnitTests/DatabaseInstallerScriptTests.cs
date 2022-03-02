#if UNITTESTS

using FluentAssertions;
using NUnit.Framework;

namespace BluePrism.DatabaseInstaller.UnitTests
{
    [TestFixture]
    public class DatabaseInstallerScriptTests
    {
        [Test]
        public void Parse_EmptyScript_ReturnsNothing()
        {
            DatabaseInstallerScript.Parse("name", string.Empty).Should().BeNull();
        }

        [Test]
        public void Parse_NullScript_ReturnsNothing()
        {
            DatabaseInstallerScript.Parse("name", null).Should().BeNull();
        }

        [Test]
        public void Parse_ScriptWithContent_ParsedCorrectly()
        {
            var script = "Some Code \r\nGO some more code \r\nGO \nGO final bit of code";

            var installerScript = DatabaseInstallerScript.Parse("name", script);

            installerScript.Name.Should().Be("name");
            installerScript.SqlStatements.Should().HaveCount(7);
            installerScript.SqlStatements[0].Should().Be("Some Code ");
            installerScript.SqlStatements[1].Should().Be("\r\n");
            installerScript.SqlStatements[2].Should().Be(" some more code ");
            installerScript.SqlStatements[3].Should().Be("\r\n");
            installerScript.SqlStatements[4].Should().Be(" ");
            installerScript.SqlStatements[5].Should().Be("\n");
            installerScript.SqlStatements[6].Should().Be(" final bit of code");
        }
    }
}

#endif