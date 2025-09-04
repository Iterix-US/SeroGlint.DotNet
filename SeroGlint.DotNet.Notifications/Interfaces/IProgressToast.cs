using Microsoft.Extensions.Logging;

namespace SeroGlint.DotNet.Notifications.Interfaces
{
    public interface IProgressToast : IToast
    {
        /// <summary>
        /// Gets the logger instance for logging events related to this progress toast.
        /// </summary>
        ILogger Logger { get; }
        /// <summary>
        /// Gets the minimum value for the progress bar control on the toast
        /// </summary>
        decimal MinValue { get; }
        /// <summary>
        /// Gets the maximum value for the progress bar control on the toast
        /// </summary>
        decimal MaxValue { get; }
        /// <summary>
        /// Gets or sets the current value for the progress bar control on the toast
        /// </summary>
        decimal CurrentValue { get; set; }

        /// <summary>
        /// Updates the progress toast with new information.
        /// </summary>
        void Update();
    }
}
