using System;
using Microsoft.Extensions.Logging;
using SeroGlint.DotNet.Common;
using SeroGlint.DotNet.Common.Extensions;
using SeroGlint.DotNet.Notifications.Events;
using SeroGlint.DotNet.Notifications.Interfaces;

namespace SeroGlint.DotNet.Notifications.ToastControls
{
    public abstract class ToastButton : IToastButton
    {
        private string _toastId;

        public ILogger Logger { get; }
        public string Text { get; }
        public Uri Uri { get; }
        public event ToastInteractionTriggered ToastButtonClicked;

        protected ToastButton(ILogger logger, string buttonText, Uri uri = null)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Text = buttonText;
            Uri = uri;
        }

        public virtual void Contribute(IButtonToast toastContainer)
        {
            
        }

        public virtual void Click()
        {
            ToastButtonClicked?.Invoke(this, new ToastInteractionTriggeredEventArgs
            {
                ToastId = _toastId,
                InteractionType = ToastInteractionType.ButtonClick,
                InteractionData = this.ToJson()
            });
        }
    }
}
