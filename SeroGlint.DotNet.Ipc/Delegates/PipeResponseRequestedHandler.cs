using System.Threading.Tasks;
using SeroGlint.DotNet.Ipc.EventArguments;

namespace SeroGlint.DotNet.Ipc.Delegates
{
    /// <summary>
    /// Delegate for handling pipe response requests.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public delegate Task PipeResponseRequestedHandler(object sender, PipeResponseRequestedEventArgs args);
}
