#if UNITTESTS

using System.Reflection;
using BluePrism.UnitTesting;
using Mono.Cecil;
using NUnit.Framework;

namespace BluePrism.CharMatching.UnitTests
{
    [TestFixture, Category("Static Analysis")]
    public class StaticAnalysisTests
    {
        /// <summary>
        /// Ensures that any setting of the 'AutoScaleMode' property within this assembly
        /// sets it to AutoScaleMode.None. Any other value will cause it to fail.
        /// </summary>
        [Test, Category("AutoScaleMode")]
        public void EnsureAutoScaleModeSetToNone_CharMatching()
        {
            StaticAnalysis.EnsureAutoScaleModeSetToNone(
                AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location)
            );
        }

        [Test, Category("StyledTextBox")]
        public void EnsureStyledTextBoxUsed_CharMatching()
        {
            StaticAnalysis.EnsureStyledTexboxUsed(
                AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location)
            );
        }

        [Test, Category("StyledRadioButton")]
        public void EnsureStyledRadioButtonUsed_CharMatching()
        {
            StaticAnalysis.EnsureStyledRadioButtonUsed(
                AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location)
            );
        }

        [Test, Category("StyledButton")]
        public void EnsureStyledButtonUsed_CharMatching()
        {
            StaticAnalysis.EnsureStyledButtonUsed(
                AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location)
            );
        }

        [Test, Category("StyledDataGridViewButtonColumn")]
        public void EnsureStyledDataGridViewButtonColumnUsed_CharMatching()
        {
            StaticAnalysis.EnsureStyledDataGridViewButtonColumnUsed(
                AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location)
            );
        }

        [Test, Category("StyledDataGridViewButtonCell")]
        public void EnsureStyledDataGridViewButtonCellUsed_CharMatching()
        {
            StaticAnalysis.EnsureStyledDataGridViewButtonCellUsed(
                AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location)
            );
        }

        [Test, Category("StyledNumericUpDown")]
        public void EnsureStyledNumericUpDownsUsed_CharMatching()
        {
            StaticAnalysis.EnsureStyledNumericUpDownUsed(
                   AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location)
               );
        }
    }
}
#endif
