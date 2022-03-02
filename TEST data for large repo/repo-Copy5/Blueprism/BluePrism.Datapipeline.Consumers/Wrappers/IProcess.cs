using System;
using System.Diagnostics;
using System.IO;

namespace BluePrism.Datapipeline.Logstash.Wrappers
{
    /// <summary>
    /// An interface abstracting the System.Diagnostics.Process class.
    /// </summary>
    public interface IProcess
    {
        /// <summary>
        /// Forcefully exits the process.
        /// </summary>
        void Kill();

        /// <summary>
        /// Sends a terminate signal to the process.
        /// </summary>
        void Terminate();

        /// <summary>
        /// Returns true id the process has exited.
        /// </summary>
        bool HasExited { get; }

        event EventHandler Exited;

        /// <summary>
        /// Specifies a set of values used for starting a process.
        /// </summary>
        ProcessStartInfo StartInfo { get; set; }

        /// <summary>
        /// Starts the process.
        /// </summary>
        void Start();

        /// <summary>
        /// The exit code of the process if it has exited.
        /// </summary>
        int ExitCode { get; }

        /// <summary>
        /// Waits for the specified timeout for the process to exit.
        /// This method does not terminate the process.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns>true if the process exits before the specified timeout, otherwise false.</returns>
        bool WaitForExit(TimeSpan timeout);

        /// <summary>
        /// Waits indefinitely for the process to exit.
        /// This method does not terminate the process.
        /// </summary>
        void WaitForExit();

        /// <summary>
        /// The time the process was started.
        /// </summary>
        DateTime StartTime { get; }

        event EventHandler<StandardOutputEventArgs> OutputDataReceived;

        event EventHandler<StandardOutputEventArgs> ErrorDataReceived;

        void BeginOutputReadLine();

        void BeginErrorReadLine();

        StreamWriter StandardInput { get; }

        StreamReader StandardError { get; }
    }
}
