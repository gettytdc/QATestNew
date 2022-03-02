using System;
using BluePrism.AutomateAppCore.BackgroundJobs.Monitoring;

namespace AutomateAppCore.UnitTests.BackgroundJobs.Monitoring
{

    /// <summary>
    /// Test implementation
    /// </summary>
    public class TestUpdateTrigger : IUpdateTrigger
    {
        public event IUpdateTrigger.UpdateEventHandler Update;

        /// <summary>
        /// Triggers update
        /// </summary>
        public void OnUpdate()
        {
            Update?.Invoke(this, EventArgs.Empty);
        }
    }
}
