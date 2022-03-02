using System.Windows.Forms;

namespace BluePrism.Core.Utility
{
    public class ClipboardWrapper : IClipboard
    {
        public IDataObject GetDataObject()
        {
            return Clipboard.GetDataObject();
        }
    }
}
