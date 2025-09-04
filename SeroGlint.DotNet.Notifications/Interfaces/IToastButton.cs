using System;
using Microsoft.Extensions.Logging;
using SeroGlint.DotNet.Notifications.Events;

namespace SeroGlint.DotNet.Notifications.Interfaces
{
    public interface IToastButton
    {
        /// <summary>
        /// Gets the logger instance for logging events related to this button.
        /// </summary>
        ILogger Logger { get; }
        /// <summary>
        /// Gets the text displayed on the button.
        /// </summary>
        string Text { get; }
        /// <summary>
        /// Gets the URI associated with the button, if any.
        /// </summary>
        Uri Uri { get; }
        /// <summary>
        /// Event triggered when the button is clicked.
        /// </summary>
        event ToastInteractionTriggered ToastButtonClicked;
        /// <summary>
        /// Contributes this button to a toast container.
        /// </summary>
        /// <param name="toastContainer"></param>
        void Contribute(IButtonToast toastContainer);
    }
}
