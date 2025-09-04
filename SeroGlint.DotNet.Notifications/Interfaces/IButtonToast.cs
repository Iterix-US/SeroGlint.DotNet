using System.Collections.Generic;

namespace SeroGlint.DotNet.Notifications.Interfaces
{
    public interface IButtonToast : IToast
    {
        /// <summary>
        /// Gets the collection of buttons associated with this toast notification.
        /// </summary>
        IList<IToastButton> Buttons { get; }
    }
}
