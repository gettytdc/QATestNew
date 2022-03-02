using System;
using BluePrism.BPCoreLib;
using System.ComponentModel;

namespace AutomateControls
{
    /// <summary>
    /// Wraps a background worker into a progress monitor which passes on any
    /// progress monitor messages directly to the worker.
    /// </summary>
    public class BackgroundWorkerProgressMonitor: clsProgressMonitor
    {
        // The wrapped background worker
        private BackgroundWorker _worker;

        /// <summary>
        /// Creates a new progress monitor which passes on progress messages to the
        /// specified background worker.
        /// </summary>
        /// <param name="worker">The worker to which progress messages should be
        /// forwarded.</param>
        /// <exception cref="ArgumentNullException">If the given background worker
        /// is null</exception>
        public BackgroundWorkerProgressMonitor(BackgroundWorker worker)
        {
            if (worker == null)
                throw new ArgumentNullException(nameof(worker));
            _worker = worker;
        }

        /// <summary>
        /// Raises the progress changed event and ensures that the background worker
        /// wrapped in this object is updated with the progress reported. This also
        /// checks the background worker to see if a cancel request has been made of
        /// it and forwards that request onto the progress monitor if it has.
        /// </summary>
        /// <param name="value">The value to which the progress should be set. To
        /// work with the background worker this should be between 0 and 100
        /// inclusive. Values outside this range may trigger an exception</param>
        /// <param name="data">The data to pass onto the worker as part of the
        /// progress update.</param>
        /// <exception cref="InvalidOperationException">If the wrapped background
        /// worker's <see cref="BackgroundWorker.WorkerReportsProgress"/> property
        /// is set to false</exception>
        protected override void OnProgressChanged(int value, object data)
        {
            base.OnProgressChanged(value, data);
            if (_worker.CancellationPending)
                RequestCancel();
            else
                _worker.ReportProgress(value, data);
        }

        /// <summary>
        /// Requests a cancellation on this progress monitor and forwards the request
        /// onto the wrapped background worker.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the wrapped background
        /// worker's <see cref="BackgroundWorker.WorkerSupportsCancellation"/>
        /// property is set to false</exception>
        public override void RequestCancel()
        {
            base.RequestCancel();
            if (!_worker.CancellationPending)
                _worker.CancelAsync();
        }
    }
}
