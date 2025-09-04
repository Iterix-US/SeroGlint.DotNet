using Microsoft.Extensions.Logging;

namespace SeroGlint.DotNet.Notifications.Interfaces
{
    public interface IToast
    {
        /// <summary>
        /// Gets the logger instance for logging events related to this button toast.
        /// </summary>
        ILogger Logger { get; }
        /// <summary>
        /// Gets the unique identifier for this toast.
        /// </summary>
        string Id { get; }
    }
}
