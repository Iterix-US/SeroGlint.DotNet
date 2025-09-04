using SeroGlint.DotNet.Ipc.EventArguments;

namespace SeroGlint.DotNet.Ipc.Delegates
{
    /// <summary>
    /// Delegate for handling messages received on a named pipe server.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public delegate void PipeMessageReceivedHandler(object sender, PipeMessageReceivedEventArgs args);
}
