using BluePrism.Core.Utility;
using System;
using System.Diagnostics;
using System.IO;

namespace BluePrism.Datapipeline.Logstash.Wrappers
{
    public class ProcessWrapper : IProcess
    {
        private readonly Process _process;

        public event EventHandler<StandardOutputEventArgs> OutputDataReceived;

        public event EventHandler<StandardOutputEventArgs> ErrorDataReceived;

        public event EventHandler Exited;

        public ProcessWrapper(Process process)
        {
            _process = process;
            _process.EnableRaisingEvents = true;
            _process.OutputDataReceived += _process_OutputDataReceived;
            _process.ErrorDataReceived += _process_ErrorDataReceived;
            _process.Exited += _process_Exited;
        }

        private void _process_Exited(object sender, EventArgs e)
        {
            Exited?.Invoke(this, EventArgs.Empty);
        }

        private void _process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            OutputDataReceived?.Invoke(this, new StandardOutputEventArgs(e.Data));
        }


        private void _process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            ErrorDataReceived?.Invoke(this, new StandardOutputEventArgs(e.Data));
        }


        public bool HasExited => _process.HasExited;

        public ProcessStartInfo StartInfo { get => _process.StartInfo; set => _process.StartInfo = value; }

        public void Kill()
        {
            _process.Kill();
        }

        public void Terminate()
        {
            CmdlineAppTerminate.StopProgram(_process);
        }

        public void Start()
        {
            _process.Start();
        }

        public int ExitCode { get => _process.ExitCode; }

        public DateTime StartTime { get => _process.StartTime; }

        public bool WaitForExit(TimeSpan timeout)
        {
            return _process.WaitForExit((int)timeout.TotalMilliseconds);
        }

        public void WaitForExit()
        {
           _process.WaitForExit();
        }

        public void BeginOutputReadLine()
        {
            _process.BeginOutputReadLine();
        }

        public void BeginErrorReadLine()
        {
            _process.BeginErrorReadLine();
        }


        public StreamWriter StandardInput
        {
            get => _process.StandardInput;
        }

        public StreamReader StandardError
        {
            get => _process.StandardError;
        }
    }
}
