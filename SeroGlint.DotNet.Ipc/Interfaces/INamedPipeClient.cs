using System.Threading.Tasks;

namespace SeroGlint.DotNet.Ipc.Interfaces
{
    /// <summary>
    /// Interface for a named pipe client that sends messages to a named pipe server.
    /// </summary>
    public interface INamedPipeClient
    {
        /// <summary>
        /// Gets the name of the named pipe used for communication with the server.
        /// </summary>
        string PipeName { get; }
        /// <summary>
        /// Connects to the named pipe server asynchronously.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Sends a message to the named pipe server using the specified pipe envelope.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pipeEnvelope"></param>
        /// <returns></returns>
        Task<string> SendAsync<T>(IPipeEnvelope<T> pipeEnvelope);

        /// <summary>
        /// Disconnects the named pipe client from the server and cleans up resources.
        /// </summary>
        void Disconnect();
    }
}
