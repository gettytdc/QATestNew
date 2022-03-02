using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BluePrism.Core.Utility
{
    public interface IChangeTracker
    {
        /// <summary>
        /// Control to record
        /// </summary>
        /// <param name="control">Control to record</param>
        void RecordValue(Control control);
        
        /// <summary>
        /// Has the control changed
        /// </summary>
        /// <returns>true if control changed.</returns>
        bool HasChanged();

        void Reset();
        
    }
}
