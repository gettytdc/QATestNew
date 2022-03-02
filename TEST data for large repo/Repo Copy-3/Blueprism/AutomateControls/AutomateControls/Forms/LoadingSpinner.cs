using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutomateControls.Forms
{
    /// <summary>
    /// An obstructive form containing a gif that can be used to represent 'work being done'.
    /// </summary>
    public partial class LoadingSpinner : Form
    {
        protected Action Action;

        /// <param name="action">Action that will be invoked when this form is Shown.</param>
        public LoadingSpinner(Action action)
        {
            InitializeComponent();

            Action = action ?? throw new ArgumentNullException($"Action {nameof(action)} cannot be null");
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            await Task.Run(Action);
            Close();
        }
    }
}
