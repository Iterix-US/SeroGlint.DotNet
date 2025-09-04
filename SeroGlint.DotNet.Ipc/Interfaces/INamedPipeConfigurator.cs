using System.Threading;
using Microsoft.Extensions.Logging;
using SeroGlint.DotNet.Security.Interfaces;

namespace SeroGlint.DotNet.Ipc.Interfaces
{
    /// <summary>
    /// Interface for configuring named pipe servers, providing properties and methods for setup.
    /// </summary>
    public interface INamedPipeConfiguration
    {
        /// <summary>
        /// Logger for logging named pipe operations and events.
        /// </summary>
        ILogger Logger { get; set; }

        /// <summary>
        /// Unique identifier for the named pipe configuration.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Indicates whether to use encryption for messages sent through the pipe.
        /// </summary>
        bool UseEncryption { get; set; }

        /// <summary>
        /// The name of the server that hosts the named pipe.
        /// </summary>
        string ServerName { get; }

        /// <summary>
        /// The name of the named pipe to be used for communication.
        /// </summary>
        string PipeName { get; }

        /// <summary>
        /// The encryption service used for securing messages.
        /// </summary>
        IEncryptionService EncryptionService { get; }

        /// <summary>
        /// Cancellation token source for managing cancellation of named pipe operations.
        /// </summary>
        CancellationTokenSource CancellationTokenSource { get; }
    }
}
