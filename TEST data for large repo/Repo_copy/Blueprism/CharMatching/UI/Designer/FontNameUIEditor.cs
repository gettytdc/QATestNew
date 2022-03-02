using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.Design;

namespace BluePrism.CharMatching.UI.Designer
{
    internal class FontNameUIEditor : UITypeEditor
    {
        private IWindowsFormsEditorService _service;

        private bool _cancelled;
        private string _chosenValue;

        public override UITypeEditorEditStyle GetEditStyle(
            ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context,
                    IServiceProvider prov, object value)
        {
            string fontName = value as string;
            SpyRegion reg = (SpyRegion)context.Instance;
            ISpyRegionContainer cont = reg.Container;

            // get the editor service
            if (prov == null)
                return value;

            _service = prov.GetService(typeof(IWindowsFormsEditorService))
                as IWindowsFormsEditorService;

            if (_service == null)
                return value;

            FontSelectionDropDown dd = new FontSelectionDropDown(reg, fontName);
            dd.FontSelectionAccepted += HandleSelectionAccepted;
            dd.FontSelectionCancelled += HandleSelectionCancelled;

            _cancelled = false;
            _service.DropDownControl(dd);
            return (_cancelled ? value : _chosenValue);
        }

        void HandleSelectionCancelled(object sender, EventArgs e)
        {
            _chosenValue = null;
            _cancelled = true;
            _service.CloseDropDown();
        }

        void HandleSelectionAccepted(object sender, EventArgs e)
        {
            _chosenValue = (sender as FontSelectionDropDown).SelectedFont;
            _service.CloseDropDown();
        }
    }

}
