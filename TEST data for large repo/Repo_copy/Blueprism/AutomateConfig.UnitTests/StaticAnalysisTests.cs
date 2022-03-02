#if UNITTESTS
using System.Reflection;
using BluePrism.UnitTesting;
using Mono.Cecil;
using NUnit.Framework;

namespace AutomateConfig.UnitTests
{

    [TestFixture]
    [Category("Static Analysis")]
    public class StaticAnalysisTests
    {

        /// <summary>
        /// Ensures that any setting of the 'AutoScaleMode' property within this assembly
        /// sets it to AutoScaleMode.None. Any other value will cause it to fail.
        /// </summary>

        [Test]
        [Category("AutoScaleMode")]
        public void EnsureAutoScaleModeSetToNone_AutomateConfig()
        {
            StaticAnalysis.EnsureAutoScaleModeSetToNone(AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location));
        }
    }

}
#endif