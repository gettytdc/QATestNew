using BluePrism.UnitTesting;
using Mono.Cecil;
using NUnit.Framework;
using System.Reflection;

namespace AutomateUI.UnitTests
{
    [TestFixture, Category("Static Analysis")]
    public class StaticAnalysisTests
    {
        [Test, Category("AutoScaleMode")]
        public void EnsureAutoScaleModeSetToNone_AutomateUI()
        {
            StaticAnalysis.EnsureAutoScaleModeSetToNone(
                AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location)
            );
        }

        [Test, Category("StyledTextBox")]
        public void EnsureStyledTextBoxUsed_AutomateUI()
        {
            StaticAnalysis.EnsureStyledTexboxUsed(
                AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location)
            );
        }

        [Test, Category("StyledRadioButton")]
        public void EnsureStyledRadioButtonUsed_AutomateUI()
        {
            StaticAnalysis.EnsureStyledRadioButtonUsed(
                AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location)
            );
        }

        [Test, Category("StyledButton")]
        public void EnsureStyledButtonUsed_AutomateUI()
        {
            StaticAnalysis.EnsureStyledButtonUsed(
                AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location)
            );
        }

        [Test, Category("StyledDataGridButtonColumn")]
        public void EnsureStyledDataGridButtonColumnUsed_AutomateUI()
        {
            StaticAnalysis.EnsureStyledDataGridViewButtonColumnUsed(
                AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location)
            );
        }

        [Test, Category("StyledDataGridButtonCell")]
        public void EnsureStyledDataGridButtonCellUsed_AutomateUI()
        {
            StaticAnalysis.EnsureStyledDataGridViewButtonCellUsed(
                AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location)
            );
        }

        [Test, Category("StyledNumericUpDown")]
        public void EnsureStyledNumericUpDownsUsed_AutomateUI()
        {
            StaticAnalysis.EnsureStyledNumericUpDownUsed(
                   AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location)
               );
        }
    }
}