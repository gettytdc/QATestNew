using System;
using System.Drawing;
using System.Windows.Forms;
using AutomateControls;

namespace AutomateControlTester.Configurators
{
    public partial class HighlightWindowConfigurator : BaseConfigurator
    {
        public HighlightWindowConfigurator()
        {
            InitializeComponent();
            btnShow.Click += HandleShowClick;
            btnShowBlocked.Click += HandleShowClick;

        }

        public Rectangle HighlightRect
        {
            get
            {
                return new Rectangle(
                    (int)numX.Value, (int)numY.Value,
                    (int)numWidth.Value, (int)numHeight.Value);
            }
            set
            {
                numX.Value = value.X;
                numY.Value = value.Y;
                numWidth.Value = value.Width;
                numHeight.Value = value.Height;
            }
        }

        public TimeSpan Duration
        {
            get { return TimeSpan.FromSeconds((double)numPeriod.Value); }
            set { numPeriod.Value = (decimal)value.TotalSeconds; }
        }

        public override string ConfigName
        {
            get { return "HighlightWindow"; }
        }

        private void HandleShowClick(object sender, EventArgs e)
        {
            TopLevelControl.Cursor = Cursors.WaitCursor;
            if (sender == btnShow)
                HighlighterWindow.ShowForAsync(
                    TopLevelControl, HighlightRect, Duration);
            else if (sender == btnShowBlocked)
                HighlighterWindow.ShowFor(
                    TopLevelControl, HighlightRect, Duration);

            TopLevelControl.Cursor = null;
        }
    }
}
