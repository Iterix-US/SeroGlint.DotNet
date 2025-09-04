using SeroGlint.DotNet.Common;

namespace SeroGlint.DotNet.Notifications.Events
{
    public class ToastInteractionTriggeredEventArgs
    {
        /// <summary>
        /// Represents the type of interaction that triggered the toast event.
        /// </summary>
        public string ToastId { get; set; }
        /// <summary>
        /// Represents the type of interaction that triggered the toast event.
        /// </summary>
        public ToastInteractionType InteractionType { get; set; }
        /// <summary>
        /// Represents additional data related to the interaction, such as button clicks or other user actions.
        /// </summary>
        public string InteractionData { get; set; }
    }
}
