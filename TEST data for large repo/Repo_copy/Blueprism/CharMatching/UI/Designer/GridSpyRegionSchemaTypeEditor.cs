using System;
using System.Drawing.Design;
using System.ComponentModel;
using System.Windows.Forms.Design;

namespace BluePrism.CharMatching.UI.Designer
{
    /// <summary>
    /// UI Type editor for handling a grid spy region schema
    /// </summary>
    public class GridSpyRegionSchemaTypeEditor : UITypeEditor   
    {
        /// <summary>
        /// Gets the edit style using the given context
        /// </summary>
        /// <param name="context">The context in which the edit style is required.
        /// </param>
        /// <returns>The type of edit style used in this editor - in this case,
        /// <see cref="UITypeEditorEditStyle.DropDown"/></returns>
        public override UITypeEditorEditStyle GetEditStyle(
            ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        /// <summary>
        /// Edits the value given using the specified context and provider.
        /// </summary>
        /// <param name="context">The context in which this value is being edited.
        /// </param>
        /// <param name="prov">The service provider for the editing</param>
        /// <param name="value">The value being edited</param>
        /// <returns>The value after editing</returns>
        public override object EditValue(
            ITypeDescriptorContext context, IServiceProvider prov, object value)
        {
            GridSpyRegionSchema schema = value as GridSpyRegionSchema;
            if (schema == null)
                return null;

            IWindowsFormsEditorService sv = null;
            if (prov == null)
                return value;

            sv = prov.GetService(typeof(IWindowsFormsEditorService))
                as IWindowsFormsEditorService;

            if (sv == null)
                return value;

            GridSpyRegionSchemaEditorDropDown selectionControl =
                new GridSpyRegionSchemaEditorDropDown(schema);
            sv.DropDownControl(selectionControl);

            return value;
        }
    }
}
