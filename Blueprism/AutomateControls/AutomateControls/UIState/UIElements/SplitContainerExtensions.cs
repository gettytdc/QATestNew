using NLog;
using System;
using System.Windows.Forms;

namespace AutomateControls.UIState.UIElements
{
    public static class SplitContainerExtensions
    {
        private static readonly IUIStateManager _uIStateManager = UIStateManager.Instance;
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static void LoadUserLayout(this SplitContainer splitContainer)
        {
            try
            {
                var layoutPreference = _uIStateManager.GetControlConfig(splitContainer.GetId());

                if (layoutPreference != null &&
                    double.TryParse(layoutPreference.Value.ToString(), out double splitterDistancePercent))
                {
                    if (splitterDistancePercent > 0 && splitterDistancePercent <= 1)
                        splitContainer.SplitterDistance = GetPositionFromPercent(splitContainer, splitterDistancePercent);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public static int GetPositionFromPercent(SplitContainer splitContainer, double positionPercent)
        {
            int calculatedSplitterDistance;
            if (splitContainer.Orientation == Orientation.Horizontal)
            {
                calculatedSplitterDistance = (int)((float)splitContainer.Height * positionPercent);
            }
            else
            {
                calculatedSplitterDistance = (int)((float)splitContainer.Width * positionPercent);
            }
            return calculatedSplitterDistance;
        }

        public static void SaveUserLayout(this SplitContainer splitContainer)
        {
            try
            {
                _uIStateManager.SetControlConfig(new UIControlConfig
                {
                    Id = splitContainer.GetId(),
                    Value = ConvertPositionToPercent(splitContainer)
                });
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public static double ConvertPositionToPercent(SplitContainer splitContainer)
        {
            double splitterDistancePercent;
            if (splitContainer.Orientation == Orientation.Horizontal)
            {
                splitterDistancePercent = (float)splitContainer.SplitterDistance / splitContainer.Height;
            }
            else
            {
                splitterDistancePercent = (float)splitContainer.SplitterDistance / splitContainer.Width;
            }
            return splitterDistancePercent;
        }

        private static string GetId(this SplitContainer splitContainer)
        {
            return $"{splitContainer.Parent?.Name}_{splitContainer.Name}";
        }
    }
}
