using BluePrism.DatabaseInstaller.Data;
using BluePrism.Utilities.Testing;
using FluentAssertions;
using NUnit.Framework;

using System;
using System.Text;

namespace BluePrism.DatabaseInstaller.UnitTests
{
    [TestFixture]
    public class DatabaseScriptGeneratorTests : UnitTestBase<DatabaseScriptGenerator>
    {
        public override void Setup()
        {
            base.Setup();

            GetMock<IDatabaseScriptLoader>()
                .Setup(l => l.GetResetInstallMarkerSql())
                .Returns("Install SQL");
        }

        [Test]
        public void GenerateInstallationScript_ValidQuery_RunsScriptsSequentially()
        {
            var script12Content = "  This is '12' "
                + Environment.NewLine
                + Environment.NewLine
                + "  /* A comment */ "
                + "A bit more SQL"
                + " /* A"
                + " Multi Line"
                + " comment */";

            GetMock<IDatabaseScriptLoader>()
                .Setup(q => q.GetUpgradeScript(12))
                .Returns(DatabaseInstallerScript.Parse("script12",script12Content));


            GetMock<IDatabaseScriptLoader>()
                .Setup(q => q.GetUpgradeScript(13))
                .Returns(DatabaseInstallerScript.Parse("script13", "This is '13'"));


            GetMock<IDatabaseScriptLoader>()
                .Setup(q => q.GetUpgradeScript(14))
                .Returns(DatabaseInstallerScript.Parse("script14", "This is '14'"));
                       

            var expected = new StringBuilder();
            expected.Append("if not exists (select 1 from BPADBVersion where dbversion='12') begin ");
            expected.Append($"exec('{script12Content.Replace("'", "''")}')");
            expected.AppendLine(" end ");
            expected.AppendLine("GO");
            expected.Append("if not exists (select 1 from BPADBVersion where dbversion='13') begin ");
            expected.Append("exec('This is ''13''')");
            expected.AppendLine(" end ");
            expected.AppendLine("GO");
            expected.Append("if not exists (select 1 from BPADBVersion where dbversion='14') begin ");
            expected.Append("exec('This is ''14''')");
            expected.AppendLine(" end ");
            expected.AppendLine("GO");
            expected.AppendLine("Install SQL");

            ClassUnderTest.GenerateInstallationScript(11, 14, false)
                .Should().Be(expected.ToString());
        }

        [Test]
        public void GenerateInstallationScript_Minify_MinifiesSqlAndExecutesInOneStep()
        {
            var script12Content = "  This is '12' " + Environment.NewLine
                + Environment.NewLine
                + "  /* A comment */ " + Environment.NewLine
                + "A bit more SQL" + Environment.NewLine
                + "/* A" + Environment.NewLine
                + " Multi Line" + Environment.NewLine
                + " comment */";

            GetMock<IDatabaseScriptLoader>()
            .Setup(q => q.GetUpgradeScript(12))
            .Returns(DatabaseInstallerScript.Parse("script12", script12Content));


            var builder = new StringBuilder();
            builder.Append("if not exists (select 1 from BPADBVersion where dbversion='12') begin ");
            builder.Append("exec('This is ''12'' A bit more SQL ')");
            builder.AppendLine(" end ");
            builder.AppendLine("Install SQL");

            ClassUnderTest.GenerateInstallationScript(11, 12, true)
                .Should().Be(builder.ToString());
        }

        [Test]
        public void GenerateInstallationScript_FromIs0_StartsAtCreate10()
        {
            GetMock<IDatabaseScriptLoader>()
                .Setup(q => q.GetCreateScript())
                .Returns(DatabaseInstallerScript.Parse("create", "This is the create script"));

            GetMock<IDatabaseScriptLoader>()
               .Setup(q => q.GetUpgradeScript(11))
               .Returns(DatabaseInstallerScript.Parse("script11", "This is '11'"));

            GetMock<IDatabaseScriptLoader>()
               .Setup(q => q.GetUpgradeScript(12))
               .Returns(DatabaseInstallerScript.Parse("script12", "This is '12'"));

            var expected = "This is the create script" + Environment.NewLine
                + "GO" + Environment.NewLine
                + "if not exists (select 1 from BPADBVersion where dbversion='11') begin exec('This is ''11''')"
                + " end " + Environment.NewLine
                + "GO" + Environment.NewLine
                + "if not exists (select 1 from BPADBVersion where dbversion='12') begin exec('This is ''12''')"
                + " end " + Environment.NewLine
                + "GO" + Environment.NewLine
                + "Install SQL" + Environment.NewLine;

            ClassUnderTest.GenerateInstallationScript(0, 12, false)
               .Should().Be(expected);
        }

        [Test]
        public void GenerateInstallationScript_ToIs0_RunsToRequiredVersion()
        {
            GetMock<IDatabaseScriptLoader>()
                .Setup(q => q.GetLatestUpgradeVersion())
                .Returns(13);

            GetMock<IDatabaseScriptLoader>()
               .Setup(q => q.GetUpgradeScript(12))
               .Returns(DatabaseInstallerScript.Parse("script12", "This is '12'"));


            GetMock<IDatabaseScriptLoader>()
                .Setup(q => q.GetUpgradeScript(13))
                .Returns(DatabaseInstallerScript.Parse("script13", "This is '13'"));

            var builder = new StringBuilder();
            builder.Append("if not exists (select 1 from BPADBVersion where dbversion='12') begin ");
            builder.Append("exec('This is ''12''')");
            builder.AppendLine(" end ");
            builder.AppendLine("GO");
            builder.Append("if not exists (select 1 from BPADBVersion where dbversion='13') begin ");
            builder.Append("exec('This is ''13''')");
            builder.AppendLine(" end ");
            builder.AppendLine("GO");
            builder.AppendLine("Install SQL");

            ClassUnderTest.GenerateInstallationScript(11, 0, false)
               .Should().Be(builder.ToString());
        }

        [Test]
        public void GenerateInstallationScript_ToIsLessThanFrom_RunsNoScripts()
        {
            GetMock<IDatabaseScriptLoader>()
              .Setup(q => q.GetUpgradeScript(11))
              .Returns(DatabaseInstallerScript.Parse("script11", "This is '11'"));


            GetMock<IDatabaseScriptLoader>()
                .Setup(q => q.GetUpgradeScript(12))
                .Returns(DatabaseInstallerScript.Parse("script12", "This is '12'"));
                     

            var builder = new StringBuilder();
            builder.AppendLine("Install SQL");

            ClassUnderTest.GenerateInstallationScript(12, 11, false)
               .Should().Be(builder.ToString());
        }

        [Test]
        public void GenerateInstallationScript_FromIsLessThan10_StartsAt11()
        {

            GetMock<IDatabaseScriptLoader>()
              .Setup(q => q.GetUpgradeScript(11))
              .Returns(DatabaseInstallerScript.Parse("script11", "This is '11'"));


            GetMock<IDatabaseScriptLoader>()
                .Setup(q => q.GetUpgradeScript(12))
                .Returns(DatabaseInstallerScript.Parse("script12", "This is '12'"));
            
            var builder = new StringBuilder();
            builder.Append("if not exists (select 1 from BPADBVersion where dbversion='11') begin ");
            builder.Append("exec('This is ''11''')");
            builder.AppendLine(" end ");
            builder.AppendLine("GO");
            builder.Append("if not exists (select 1 from BPADBVersion where dbversion='12') begin ");
            builder.Append("exec('This is ''12''')");
            builder.AppendLine(" end ");
            builder.AppendLine("GO");
            builder.AppendLine("Install SQL");

            ClassUnderTest.GenerateInstallationScript(2, 12, false)
               .Should().Be(builder.ToString());
        }

        [Test]
        public void GenerateInstallationScript_InvalidAction_Throws()
        {
            Action generate = () => ClassUnderTest.GenerateInstallationScript(DatabaseAction.None);
            generate.ShouldThrow<Exception>();
        }

        [Test]
        public void GenerateInstallationScript_ValidUpgrade_RunsScriptsSequentially()
        {
            GetMock<IDatabaseScriptLoader>()
                .Setup(l => l.GetLatestUpgradeVersion())
                .Returns(13);

            GetMock<IDatabaseScriptLoader>()
                .Setup(q => q.GetUpgradeScript(11))
                .Returns(DatabaseInstallerScript.Parse("script11", "This is '11'"));

            GetMock<IDatabaseScriptLoader>()
                .Setup(q => q.GetUpgradeScript(12))
                .Returns(DatabaseInstallerScript.Parse("script12", "This is '12'"));

            GetMock<IDatabaseScriptLoader>()
                .Setup(q => q.GetUpgradeScript(13))
                .Returns(DatabaseInstallerScript.Parse("script13", "This is '13'"));

            var expected = new StringBuilder();
            expected.Append("if not exists (select 1 from BPADBVersion where dbversion='11') ");
            expected.AppendLine($"begin exec('This is ''11'' ') end ");
            expected.Append("if not exists (select 1 from BPADBVersion where dbversion='12') ");
            expected.AppendLine($"begin exec('This is ''12'' ') end ");
            expected.Append("if not exists (select 1 from BPADBVersion where dbversion='13') ");
            expected.AppendLine($"begin exec('This is ''13'' ') end ");
            expected.AppendLine("Install SQL");

            ClassUnderTest.GenerateInstallationScript(DatabaseAction.Upgrade)
                .Should().Be(expected.ToString());
        }

        [Test]
        public void GenerateInstallationScript_CreateWithExclusions_SkipsInstalledScripts()
        {
            GetMock<IDatabaseScriptLoader>()
                .Setup(q => q.GetCreateScript())
                .Returns(DatabaseInstallerScript.Parse("create", "This is the create script"));

            GetMock<IDatabaseScriptLoader>()
                .Setup(l => l.GetLatestUpgradeVersion())
                .Returns(12);

            GetMock<IDatabaseScriptLoader>()
                .Setup(q => q.GetUpgradeScript(11))
                .Returns(DatabaseInstallerScript.Parse("script11", "This is '11'"));

            GetMock<IDatabaseScriptLoader>()
                .Setup(q => q.GetUpgradeScript(12))
                .Returns(DatabaseInstallerScript.Parse("script12", "This is '12'"));

            var expected = "This is the create script " + Environment.NewLine
                + "GO" + Environment.NewLine
                + "if not exists (select 1 from BPADBVersion where dbversion='12') begin exec('This is ''12'' ')"
                + " end " + Environment.NewLine
                + "Install SQL" + Environment.NewLine;

            ClassUnderTest.GenerateInstallationScript(DatabaseAction.Create, new int[] { 11 })
               .Should().Be(expected);
        }

        [Test]
        public void GenerateInstallationScript_Create_StartsAtCreate()
        {
            GetMock<IDatabaseScriptLoader>()
                .Setup(q => q.GetCreateScript())
                .Returns(DatabaseInstallerScript.Parse("create", "This is the create script"));

            GetMock<IDatabaseScriptLoader>()
                .Setup(l => l.GetLatestUpgradeVersion())
                .Returns(12);

            GetMock<IDatabaseScriptLoader>()
                .Setup(q => q.GetUpgradeScript(11))
                .Returns(DatabaseInstallerScript.Parse("script11", "This is '11'"));

            GetMock<IDatabaseScriptLoader>()
                .Setup(q => q.GetUpgradeScript(12))
                .Returns(DatabaseInstallerScript.Parse("script12", "This is '12'"));

            GetMock<IDatabaseScriptLoader>()
                .Setup(q => q.GetResetInstallMarkerSql())
                .Returns("Install SQL");

            var expected = "This is the create script " + Environment.NewLine
                + "GO" + Environment.NewLine
                + "if not exists (select 1 from BPADBVersion where dbversion='11') begin exec('This is ''11'' ')"
                + " end " + Environment.NewLine
                + "if not exists (select 1 from BPADBVersion where dbversion='12') begin exec('This is ''12'' ')"
                + " end " + Environment.NewLine
                + "Install SQL" + Environment.NewLine;

            ClassUnderTest.GenerateInstallationScript(DatabaseAction.Create)
               .Should().Be(expected);
        }

        [Test]
        public void GenerateInstallationScript_Upgrade_StartsAt11()
        {
            GetMock<IDatabaseScriptLoader>()
                .Setup(q => q.GetCreateScript())
                .Returns(DatabaseInstallerScript.Parse("create", "This is the create script"));

            GetMock<IDatabaseScriptLoader>()
                .Setup(l => l.GetLatestUpgradeVersion())
                .Returns(12);

            GetMock<IDatabaseScriptLoader>()
                .Setup(q => q.GetUpgradeScript(11))
                .Returns(DatabaseInstallerScript.Parse("script11", "This is '11'"));

            GetMock<IDatabaseScriptLoader>()
                .Setup(q => q.GetUpgradeScript(12))
                .Returns(DatabaseInstallerScript.Parse("script12", "This is '12'"));

            var expected = "if not exists (select 1 from BPADBVersion where dbversion='11') begin exec('This is ''11'' ')"
                + " end " + Environment.NewLine
                + "if not exists (select 1 from BPADBVersion where dbversion='12') begin exec('This is ''12'' ')"
                + " end " + Environment.NewLine
                + "Install SQL" + Environment.NewLine;

            ClassUnderTest.GenerateInstallationScript(DatabaseAction.Upgrade)
               .Should().Be(expected);
        }
    }
}
