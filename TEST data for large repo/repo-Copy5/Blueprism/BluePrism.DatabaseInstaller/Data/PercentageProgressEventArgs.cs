using System;

namespace BluePrism.DatabaseInstaller
{
    public delegate void PercentageProgressEventHandler(object sender, PercentageProgressEventArgs e);

    public class PercentageProgressEventArgs : EventArgs
    {
        public int PercentProgress { get; }
        public string Message { get; }

        public PercentageProgressEventArgs(int percentageProgress, string message)
        {
            PercentProgress = percentageProgress;
            Message = message;
        }
    }
}
