using System.Diagnostics;

namespace SeroGlint.DotNet.Common.Abstractions
{
    public interface IProcessWrapper
    {
        ProcessStartInfo StartInfo { get; set; }
        int ExitCode { get; }
        bool HasExited { get; }
        bool Start();
        Process Start(ProcessStartInfo startInfo);
        void WaitForExit();
        void Kill();
        event DataReceivedEventHandler OutputDataReceived;
        event DataReceivedEventHandler ErrorDataReceived;
        void BeginOutputReadLine();
        void BeginErrorReadLine();
        void Dispose();
    }
}