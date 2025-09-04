using System;
using SeroGlint.DotNet.Notifications.Events;

namespace SeroGlint.DotNet.Notifications.Interfaces
{
    public interface IToastBase : IToast
    {
        /// <summary>
        /// Gets the group name for this toast, used for grouping similar toasts together.
        /// </summary>
        string GroupName { get; }
        /// <summary>
        /// Gets the title of the toast, which is displayed prominently.
        /// </summary>
        string Title { get; }
        /// <summary>
        /// Gets the timeout duration for the toast, after which it will automatically close.
        /// </summary>
        TimeSpan? Timeout { get; }

        /// <summary>
        /// Event triggered when the toast is clicked or interacted with.
        /// </summary>
        event ToastInteractionTriggered ToastClicked;

        /// <summary>
        /// Triggers the toast click event, notifying listeners of the interaction.
        /// </summary>
        void Click();
    }
}
