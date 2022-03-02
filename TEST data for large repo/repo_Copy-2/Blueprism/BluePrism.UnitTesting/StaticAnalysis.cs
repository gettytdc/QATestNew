using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NUnit.Framework;

namespace BluePrism.UnitTesting
{
    /// <summary>
    /// Provides methods which can be used to perform some form of static analysis on
    /// the code within an assembly.
    /// </summary>
    /// <remarks>Any calls to this class will probably require a reference to the
    /// 'Mono.Cecil' NuGet package configured in the solution.</remarks>
    public class StaticAnalysis
    {
        /// <summary>
        /// Ensures that all calls setting the 'AutoScaleMode' property of a control
        /// or form within an assembly use the <see cref="AutoScaleMode.None"/> value
        /// rather than any other value.
        /// </summary>
        /// <param name="assembly">The assembly definition to search and check for
        /// the correct value being set in an AutoScaleMode property call.</param>
        /// <remarks>Note that, as written, this will only check inside methods named
        /// <c>InitializeComponent</c> - ie. the auto-generated method created by
        /// Visual Studio for user controls and forms.</remarks>
        public static void EnsureAutoScaleModeSetToNone(AssemblyDefinition assembly)
        {
            // All the references to `.AutoScaleMode = x` where `x` is *not* zero
            // called within 'InitializeComponent' methods in the assembly.
            var refs = assembly.
                Modules.
                SelectMany(m => m.Types).
                SelectMany(t => t.Methods).
                Where(m => m.Name == "InitializeComponent").
                Where(m => m.Body.Instructions.Any(i =>
                {
                    if (i.OpCode != OpCodes.Call) return false;

                    var method = i.Operand as MethodReference;
                    if (method?.Name != "set_AutoScaleMode") return false;

                // If we reach '.AutoScaleMode = x', check the previous
                // IL code and make sure it is setting the value to 0
                return (i.Previous.OpCode != OpCodes.Ldc_I4_0);
                })).
                Select(m => m.DeclaringType.Name).
                ToList();

            Assert.That(refs, Is.Empty);
        }
        private static void CheckControlExistsOnForm(AssemblyDefinition assembly, string controlName)
        {
            var refs = assembly.
                Modules.
                SelectMany(m => m.Types).
                SelectMany(t => t.Methods).
                Where(m => m.Name == "InitializeComponent").
                Where(m => m.Body.Instructions.Any(i =>
                {
                    if (i.OpCode != OpCodes.Newobj) return false;

                    var method = i.Operand as MethodReference;
                    if (method?.DeclaringType.FullName != controlName) return false;

                    return true;
                }));

            Assert.That(refs, Is.Empty);
        }

        /// <summary>
        /// Ensures that there are no instances of a System.Windows.Forms.TextBox
        /// being initialised within the InitializeComponent method of a Designer.
        /// </summary>
        /// <remarks>If this unit test fails then a System.Windows.Forms.TextBox
        /// has been added to a form/control. Please replace this with our custom
        /// AutomateControls.Textboxes.StyledTextBox to maintain all accessibility 
        /// functionality</remarks>
        public static void EnsureStyledTexboxUsed(AssemblyDefinition assembly)
            => CheckControlExistsOnForm(assembly, "System.Windows.Forms.TextBox");

        /// <summary>
        /// Ensures that there are no instances of a System.Windows.Forms.RadioButton
        /// being initialised within the InitializeComponent method of a Designer.
        /// </summary>
        /// <remarks>If this unit test fails then a System.Windows.Forms.RadioButton
        /// has been added to a form/control. Please replace this with our custom
        /// AutomateControls.StyledRadioButton to maintain all accessibility 
        /// functionality</remarks>
        public static void EnsureStyledRadioButtonUsed(AssemblyDefinition assembly)
            => CheckControlExistsOnForm(assembly, "System.Windows.Forms.RadioButton");

        /// <summary>
        /// Ensures that there are no instances of a System.Windows.Forms.DataGridViewButtonColumn
        /// being initialised within the InitializeComponent method of a Designer.
        /// </summary>
        /// <remarks>If this unit test fails then a System.Windows.Forms.DataGridViewButtonColumn
        /// has been added to a form/control. Please replace this with  
        /// AutomateControls.Buttons.StyledDataGridViewButtonColumn to maintain all 
        /// accessibility functionality</remarks>
        public static void EnsureStyledDataGridViewButtonColumnUsed(AssemblyDefinition assembly) =>
            CheckControlExistsOnForm(assembly, "System.Windows.Forms.DataGridViewButtonColumn");

        /// <summary>
        /// Ensures that there are no instances of a System.Windows.Forms.DataGridViewButtonCell
        /// being initialised within the InitializeComponent method of a Designer.
        /// </summary>
        /// <remarks>If this unit test fails then a System.Windows.Forms.DataGridViewButtonCell
        /// has been added to a form/control. Please replace this with  
        /// AutomateControls.Buttons.StyledDataGridViewButtonCell to maintain all 
        /// accessibility functionality</remarks>
        public static void EnsureStyledDataGridViewButtonCellUsed(AssemblyDefinition assembly) =>
            CheckControlExistsOnForm(assembly, "System.Windows.Forms.DataGridViewButtonCell");

        /// <summary>
        /// Ensures that there are no instances of a System.Windows.Forms.Button
        /// being initialised within the InitializeComponent method of a Designer.
        /// </summary>
        /// <remarks>If this unit test fails then a System.Windows.Forms.Button
        /// has been added to a form/control. Please replace this with either 
        /// AutomateControls.Buttons.StandardStyledButton or
        /// AutomateControls.Buttons.FlatStyleStyledButton to maintain all 
        /// accessibility functionality</remarks>
        public static void EnsureStyledButtonUsed(AssemblyDefinition assembly) =>
            CheckControlExistsOnForm(assembly, "System.Windows.Forms.Button");

        public static void EnsureStyledNumericUpDownUsed(AssemblyDefinition assembly) =>
            CheckControlExistsOnForm(assembly, "System.Windows.Forms.NumericUpDown");

       
    }
}