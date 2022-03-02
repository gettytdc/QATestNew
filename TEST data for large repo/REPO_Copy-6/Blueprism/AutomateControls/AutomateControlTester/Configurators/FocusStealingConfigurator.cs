using System;
using System.Runtime.InteropServices;

namespace AutomateControlTester.Configurators
{
    public partial class FocusStealingConfigurator : BaseConfigurator
    {
        [DllImport("user32")]
        static extern bool SetForegroundWindow(IntPtr hwnd);

        public FocusStealingConfigurator()
        {
            InitializeComponent();
        }

        public override string ConfigName
        {
            get { return "Focus Stealer"; }
        }

        private void btnSteal_Click(object sender, EventArgs e)
        {
            timerStealer.Enabled = false;
            timerStealer.Interval = (int)(1000 * spinnerStealSecs.Value);
            timerStealer.Enabled = true;
            txtEntry.Focus();
        }

        private void timerStealer_Tick(object sender, EventArgs e)
        {
            SetForegroundWindow(this.Handle);
            timerStealer.Enabled = false;
        }

    }
}
