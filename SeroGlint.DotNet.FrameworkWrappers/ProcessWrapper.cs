using System.Diagnostics;
using SeroGlint.DotNet.Common.Abstractions;

namespace SeroGlint.DotNet.Common.Wrappers
{
    public class ProcessWrapper : IProcessWrapper
    {
        private readonly Process _process = new Process();

        public ProcessStartInfo StartInfo
        {
            get => _process.StartInfo;
            set => _process.StartInfo = value;
        }

        public bool Start()
        {
            return _process.Start();
        }

        public Process Start(ProcessStartInfo startInfo)
        {
            return Process.Start(startInfo);
        }

        public void WaitForExit()
        {
            _process.WaitForExit();
        }

        public int ExitCode => _process.ExitCode;

        public bool HasExited => _process.HasExited;

        public void Kill()
        {
            _process.Kill();
        }

        public event DataReceivedEventHandler OutputDataReceived
        {
            add => _process.OutputDataReceived += value;
            remove => _process.OutputDataReceived -= value;
        }

        public event DataReceivedEventHandler ErrorDataReceived
        {
            add => _process.ErrorDataReceived += value;
            remove => _process.ErrorDataReceived -= value;
        }

        public void BeginOutputReadLine()
        {
            _process.BeginOutputReadLine();
        }

        public void BeginErrorReadLine()
        {
            _process.BeginErrorReadLine();
        }

        public void Dispose()
        {
            _process.Dispose();
        }
    }
}