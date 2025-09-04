using System.Collections.Generic;
using SeroGlint.DotNet.Notifications.Interfaces;

namespace SeroGlint.DotNet.Notifications.Toasts
{
    public class ButtonToast : ToastBase<ButtonToast>, IButtonToast
    {
        public IList<IToastButton> Buttons { get; }
    }
}
