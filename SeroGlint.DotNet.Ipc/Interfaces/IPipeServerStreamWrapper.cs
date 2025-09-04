using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace SeroGlint.DotNet.Ipc.Interfaces
{
    public interface IPipeServerStreamWrapper : IDisposable
    {
        /// <summary>
        /// Indicates that the server is connected to a client
        /// </summary>
        bool IsConnected { get; }
        /// <summary>
        /// Indicates whether the server is currently listening for connections.
        /// </summary>
        bool IsListening { get; }
        /// <summary>
        /// The underlying NamedPipeServerStream instance that this wrapper is managing.
        /// </summary>
        NamedPipeServerStream ServerStream { get; }
        /// <summary>
        /// Waits for a client to connect to the named pipe server asynchronously.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task WaitForConnectionAsync(CancellationToken token);
        /// <summary>
        /// Writes a byte array to the named pipe server asynchronously.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken token);
        /// <summary>
        /// Flushes the named pipe server stream asynchronously, ensuring that all data is sent to the client.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task FlushAsync(CancellationToken token);
        /// <summary>
        /// Reads a specified number of bytes from the named pipe server stream asynchronously.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="i"></param>
        /// <param name="bufferLength"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<int> ReadAsync(byte[] buffer, int i, int bufferLength, CancellationToken token);
    }
}
