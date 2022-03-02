#if UNITTESTS

using System.Reflection;
using BluePrism.UnitTesting;
using Mono.Cecil;
using NUnit.Framework;

namespace BluePrism.BPServer.UnitTests { 
    [TestFixture, Category("Static Analysis")]
    public class StaticAnalysisTests
    {
        /// <summary>
        /// Ensures that any setting of the 'AutoScaleMode' property within this assembly
        /// sets it to AutoScaleMode.None. Any other value will cause it to fail.
        /// </summary>
        [Test, Category("AutoScaleMode")]
        public void EnsureAutoScaleModeSetToNone_BPServer()
        {
            StaticAnalysis.EnsureAutoScaleModeSetToNone(
                AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location)
            );
        }

        [Test, Category("StyledTextBox")]
        public void EnsureStyledTextBoxUsed_BPServer()
        {
            StaticAnalysis.EnsureStyledTexboxUsed(
                AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location)
            );
        }

        [Test, Category("StyledRadioButton")]
        public void EnsureStyledRadioButtonUsed_BPServer()
        {
            StaticAnalysis.EnsureStyledRadioButtonUsed(
                AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location)
            );
        }

        [Test, Category("StyledButton")]
        public void EnsureStyledButtonUsed_BPServer()
        {
            StaticAnalysis.EnsureStyledButtonUsed(
                AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location)
            );
        }

        [Test, Category("StyledDataGridViewButtonColumn")]
        public void EnsureStyledDataGridViewButtonColumnUsed_BPServer()
        {
            StaticAnalysis.EnsureStyledDataGridViewButtonColumnUsed(
                AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location)
            );
        }

        [Test, Category("StyledDataGridViewButtonCell")]
        public void EnsureStyledDataGridViewButtonCellUsed_BPServer()
        {
            StaticAnalysis.EnsureStyledDataGridViewButtonCellUsed(
                AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location)
            );
        }

        [Test, Category("StyledNumericUpDown")]
        public void EnsureStyledNumericUpDownsUsed_BPServer()
        {
            StaticAnalysis.EnsureStyledNumericUpDownUsed(
                   AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location)
               );
        }
    }
}

#endif
