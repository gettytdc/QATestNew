#if UNITTESTS

using System;

namespace BluePrism.Core.TestSupport
{
    /// <summary>
    /// Results of executing command via <see cref="CommandLineHelper"/>
    /// </summary>
    public class CommandLineResult
    {
        public CommandLineResult(int exitCode, string output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));
            ExitCode = exitCode;
            Output = output;
        }

        /// <summary>
        /// The exit code from the process
        /// </summary>
        public int ExitCode { get; private set; }

        /// <summary>
        /// The output from the processes output and error streams
        /// </summary>
        public string Output { get; private set; }

        /// <summary>
        /// Indicates whether the command executed successfully based on the
        /// error code. 
        /// </summary>
        public bool Success { get { return ExitCode == 0; } }
    }
}

#endif