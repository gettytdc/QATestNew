using System.Reflection;
using Mono.Cecil;
using NUnit.Framework;
using BluePrism.UnitTesting;

namespace AutomateControls.UnitTests
{
    [TestFixture, Category("Static Analysis")]
    public class StaticAnalysisTests
    {
        /// <summary>
        /// Ensures that any setting of the 'AutoScaleMode' property within this assembly
        /// sets it to AutoScaleMode.None. Any other value will cause it to fail.
        /// </summary>
        [Test, Category("AutoScaleMode")]
        public void EnsureAutoScaleModeSetToNone_AutomateControls()
        {
            StaticAnalysis.EnsureAutoScaleModeSetToNone(
                AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location)
            );
        }

        [Test, Category("StyledTextBox")]
        public void EnsureStyledTextBoxUsed_AutomateControls()
        {
            StaticAnalysis.EnsureStyledTexboxUsed(
                AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location)
            );
        }

        [Test, Category("StyledRadioButton")]
        public void EnsureStyledRadioButtonUsed_AutomateControls()
        {
            StaticAnalysis.EnsureStyledRadioButtonUsed(
                AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location)
            );
        }

        [Test, Category("StyledButton")]
        public void EnsureStyledButtonUsed_AutomateControls()
        {
            StaticAnalysis.EnsureStyledButtonUsed(
                AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location)
            );
        }

        [Test, Category("StyledDataGridButtonColumn")]
        public void EnsureStyledDataGridButtonColumnUsed_AutomateControls()
        {
            StaticAnalysis.EnsureStyledDataGridViewButtonColumnUsed(
                AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location)
            );
        }

        [Test, Category("StyledDataGridButtonCell")]
        public void EnsureStyledDataGridButtonCellUsed_AutomateControls()
        {
            StaticAnalysis.EnsureStyledDataGridViewButtonCellUsed(
                AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location)
            );
        }

        [Test, Category("StyledNumericUpDown")]
        public void EnsureStyledNumericUpDownUsed_AutomateControls()
        {
            StaticAnalysis.EnsureStyledNumericUpDownUsed(
                AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location)
            );
        }
    }
}