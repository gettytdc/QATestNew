using System;
using System.Drawing;
using System.Windows.Forms;

namespace AutomateControls
{
    public class ColorProgressBar : ProgressBar
    {
        private const int _marqueeLength = 127;
        private const int _padding = 2;
        private int _currentPos = -_marqueeLength;
        private Timer _timer;

        public ColorProgressBar()
        {
            SetStyle(ControlStyles.UserPaint, true);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (Style != ProgressBarStyle.Marquee) return;

            _currentPos += Step;
            if (_currentPos > Width)
                _currentPos = -_marqueeLength;

            Invalidate();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            _timer = new Timer();
            _timer.Interval = MarqueeAnimationSpeed;
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            _timer.Stop();
            _timer.Dispose();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (ProgressBarRenderer.IsSupported)
                ProgressBarRenderer.DrawHorizontalBar(e.Graphics, e.ClipRectangle);

            switch (Style)
            {
                case ProgressBarStyle.Continuous:
                    OnPaintContinuous(e);
                    break;
                case ProgressBarStyle.Marquee:
                    OnPaintMarquee(e);
                    break;
            }
        }

        private void OnPaintContinuous(PaintEventArgs e)
        {
            Rectangle rec = e.ClipRectangle;
            rec.Width = (int)(rec.Width * ((double)Value / Maximum)) - (_padding + _padding);
            rec.Height = rec.Height - (_padding + _padding);

            using (var foreColorBrush = new SolidBrush(ForeColor))
                e.Graphics.FillRectangle(foreColorBrush, _padding, _padding, rec.Width, rec.Height);
        }


        private void OnPaintMarquee(PaintEventArgs e)
        {
            Rectangle rec = e.ClipRectangle;
            rec.Height = rec.Height - (_padding + _padding);
            using (var foreColorBrush = new SolidBrush(ForeColor))
                e.Graphics.FillRectangle(foreColorBrush, _currentPos + _padding, _padding, _marqueeLength, rec.Height);
        }
    }
}