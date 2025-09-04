using System;
using Microsoft.Extensions.Logging;
using SeroGlint.DotNet.Common;
using SeroGlint.DotNet.Common.Extensions;
using SeroGlint.DotNet.Notifications.Events;
using SeroGlint.DotNet.Notifications.Interfaces;

namespace SeroGlint.DotNet.Notifications.Toasts
{
    /// <summary>
    /// Bread.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ToastBase<T> : IToastBase
    {
        public virtual ILogger Logger { get; private set; }
        public virtual string Id { get; private set; } = Guid.NewGuid().ToString();
        public virtual string GroupName { get; private set; } = "SeroGlint";
        public virtual string Title { get; private set; } = string.Empty;
        public virtual TimeSpan? Timeout { get; private set; } = TimeSpan.FromSeconds(5);
        public virtual event ToastInteractionTriggered ToastClicked;

        public ToastBase<T> WithLogger(ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            return this;
        }

        public ToastBase<T> WithId(string id)
        {
            Id = id;
            if (!id.IsNullOrWhitespace())
            {
                return this;
            }

            Id = Guid.NewGuid().ToString();
            Logger.LogWarning("No id provided. Assigning a random id: {0}", Id);
            return this;
        }

        public ToastBase<T> WithGroupName(string groupName)
        {
            GroupName = groupName;
            if (!groupName.IsNullOrWhitespace())
            {
                return this;
            }

            GroupName = groupName ?? "SeroGlint";
            Logger.LogWarning("No group name provided. Assigning default group name: {0}", GroupName);
            return this;
        }

        public ToastBase<T> WithTitle(string title)
        {
            Title = title;
            if (!title.IsNullOrWhitespace())
            {
                return this;
            }

            Title = "SeroGlint";
            Logger.LogWarning("No title provided. Assigning default title: {0}", Title);
            return this;
        }

        public ToastBase<T> WithTimeout(TimeSpan? timeout)
        {
            Timeout = timeout;
            if (timeout != null)
            {
                return this;
            }

            Timeout = TimeSpan.FromSeconds(5);
            Logger.LogWarning("No timeout provided. Assigning default timeout: {0} seconds", Timeout.Value.TotalSeconds);
            return this;
        }

        public virtual void Click()
        {
            var toastInteractionTriggeredEventArgs = new ToastInteractionTriggeredEventArgs
            {
                ToastId = Id,
                InteractionType = ToastInteractionType.Click,
                InteractionData = this.ToJson()
            };

            Logger.LogInformation("Stock toast clicked (not a button)- triggering default click logic.");
            ToastClicked?.Invoke(this, toastInteractionTriggeredEventArgs);
        }
    }
}
