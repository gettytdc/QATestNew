#if DEBUG

using NUnit.Framework;
using System;
using System.Linq;
using System.Xml;

namespace LoginAgentService.UnitTests
{
    /// <summary>
    /// Tests the configuration class of the login agent service.
    /// </summary>
    [TestFixture]
    public class ConfigTests
    {
        /// <summary>
        /// Tests the arg parsing for the resource startup arguments defined in the
        /// config file, ensuring that multiple values associated with an argument
        /// and argument values with spaces are handled correctly.
        /// </summary>
        [Test]
        public void TestCommandLineArgs()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(
@"<configuration>
  <workingdirectory path=""C:\Program Files\Blue Prism Limited\Automate"" />
  <startuparguments>
    <argument name=""empty"" />
    <argument name=""single""><value>1</value></argument>
    <argument name=""withspace""><value>Has a space</value></argument>
    <argument name=""double""><value>Two</value><value>Values</value></argument>
    <argument name=""treble""><value>Trebles</value><value>All</value><value>Round!</value></argument>
    <argument name=""doublewithspace""><value>A Double</value><value>With Spaces</value></argument>
  </startuparguments>
</configuration>");

            Config cfg = new Config();
            cfg.LoadXML(doc);

            var names = cfg.StartupArguments.Select((a) => a.Name).ToList();

            Assert.That(names, Contains.Item("empty"));
            Assert.That(names, Contains.Item("single"));
            Assert.That(names, Contains.Item("withspace"));
            Assert.That(names, Contains.Item("double"));
            Assert.That(names, Contains.Item("treble"));
            Assert.That(names, Contains.Item("doublewithspace"));

            Assert.That(
                cfg.StartupArguments.First((a) => a.Name == "empty").Values,
                Is.Empty);

            Assert.That(
                cfg.StartupArguments.First((a) => a.Name == "single").Values,
                Has.Count.EqualTo(1));

            Assert.That(
                cfg.StartupArguments.First((a) => a.Name == "withspace").Values,
                Has.Count.EqualTo(1));

            Assert.That(
                cfg.StartupArguments.First((a) => a.Name == "double").Values,
                Has.Count.EqualTo(2));

            Assert.That(
                cfg.StartupArguments.First((a) => a.Name == "treble").Values,
                Has.Count.EqualTo(3));

            Assert.That(
                cfg.StartupArguments.First((a) => a.Name == "doublewithspace").Values,
                Has.Count.EqualTo(2));

            Assert.That(
                cfg.ExpandedCommandLineArguments,
                Is.EqualTo(new String[]{
                    "/empty",
                    "/single", "1",
                    "/withspace", "Has a space",
                    "/double", "Two", "Values",
                    "/treble", "Trebles", "All", "Round!",
                    "/doublewithspace", "A Double", "With Spaces"
                })
            );

            Assert.That(
                cfg.EscapedCommandLineArguments,
                Is.EqualTo(
                    "/empty" +
                    " /single 1" +
                    " /withspace \"Has a space\"" +
                    " /double Two Values" +
                    " /treble Trebles All Round!" +
                    " /doublewithspace \"A Double\" \"With Spaces\""
                )
            );

        }
    }
}

#endif