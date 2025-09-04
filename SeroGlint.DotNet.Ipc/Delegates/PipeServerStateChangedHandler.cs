using System.Threading.Tasks;
using SeroGlint.DotNet.Ipc.EventArguments;

namespace SeroGlint.DotNet.Ipc.Delegates
{
    public delegate Task PipeServerStateChangedHandler(object sender, PipeServerStateChangedEventArgs args);
}
