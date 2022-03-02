using System.Windows.Forms;

namespace BluePrism.Core.Utility
{
    public interface IClipboard
    {
        IDataObject GetDataObject();
    }
}