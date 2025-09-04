using Microsoft.Extensions.Logging;

namespace SeroGlint.DotNet.Notifications.Interfaces
{
    public interface INotificationPlatform
    {
        /// <summary>
        /// Gets the logger instance for logging events related to the notification platform.
        /// </summary>
        ILogger Logger { get; }
        /// <summary>
        /// Gets the unique identifier for the notification platform instance.
        /// </summary>
        string OperatingSystem { get; }
        /// <summary>
        /// Gets the name of the software that is using this notification platform.
        /// </summary>
        string SoftwareName { get; }
        /// <summary>
        /// Pushes a toast notification to the platform for display.
        /// </summary>
        /// <param name="payload"></param>
        void PopToast(IToast payload);
    }
}
