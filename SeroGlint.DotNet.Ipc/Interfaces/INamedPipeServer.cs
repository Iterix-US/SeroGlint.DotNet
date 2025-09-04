using System;
using System.Threading.Tasks;
using SeroGlint.DotNet.Ipc.Delegates;
using SeroGlint.DotNet.Ipc.Objects;

namespace SeroGlint.DotNet.Ipc.Interfaces
{
    /// <summary>
    /// Interface for a named pipe server that listens for messages and processes them asynchronously.
    /// </summary>
    public interface INamedPipeServer : IDisposable
    {
        /// <summary>
        /// Unique identifier for the named pipe server instance.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Indicates whether the named pipe server is currently listening for messages.
        /// </summary>
        bool IsListening { get; }

        /// <summary>
        /// Configuration settings for the named pipe server.
        /// </summary>
        PipeServerConfiguration Configuration { get; }

        /// <summary>
        /// Event that is triggered when a message is received on the named pipe.
        /// </summary>
        event PipeMessageReceivedHandler MessageReceived;

        /// <summary>
        /// Event that is triggered when a response is requested from the named pipe server.
        /// </summary>
        /// <returns></returns>
        Task StartAsync();

        /// <summary>
        /// Requests, via CancellationTokenSource, that the named pipe server stops listening for messages and cleans up resources.
        /// </summary>
        /// <returns></returns>
        void Stop();
    }
}
