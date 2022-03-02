#if UNITTESTS

using System;
using System.Diagnostics;
using System.Text;

namespace BluePrism.Core.TestSupport
{
    /// <summary>
    /// Utility for executing simple commands on the command line. Intended for use within
    /// tests that require interaction with external programs.
    /// </summary>
    public class CommandLineHelper
    {
        /// <summary>
        /// Executes a process with the specified arguments and returns the result
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="arguments"></param>
        /// <param name="throwOnError"></param>
        /// <returns></returns>
        public static CommandLineResult ExecCommand(string fileName, string arguments, bool throwOnError)
        {
            var startInfo = new ProcessStartInfo(fileName)
            {
                Arguments = arguments,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };

            CommandLineResult commandResult;
            using (var process = new Process { StartInfo = startInfo })
            {
                var output = new StringBuilder();
                process.OutputDataReceived += (sender, e) => { output.AppendLine(e.Data); };
                process.ErrorDataReceived += (sender, e) => { output.AppendLine(e.Data); };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                commandResult = new CommandLineResult(process.ExitCode, output.ToString());
            }

            if (throwOnError && !commandResult.Success)
                throw new InvalidOperationException(string.Format("{0}: {1}", commandResult.ExitCode, commandResult.Output));
            return commandResult;
        }
    }
}

#endif