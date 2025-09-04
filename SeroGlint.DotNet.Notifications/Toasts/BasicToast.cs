using System;
using Microsoft.Extensions.Logging;
using SeroGlint.DotNet.Common.Extensions;
using SeroGlint.DotNet.Notifications.Interfaces;

namespace SeroGlint.DotNet.Notifications.Toasts
{
    public class BasicToast : ToastBase<BasicToast>, IBasicToast
    {
        public string Body { get; private set; } = "";
        public string IconPath { get; private set; } = "";

        public new BasicToast WithLogger(ILogger logger)
        {
            base.WithLogger(logger);
            return this;
        }

        public new BasicToast WithTitle(string title)
        {
            base.WithTitle(title);
            return this;
        }

        public new BasicToast WithId(string id)
        {
            base.WithId(id);
            return this;
        }

        public new BasicToast WithGroupName(string groupName)
        {
            base.WithGroupName(groupName);
            return this;
        }

        public new BasicToast WithTimeout(TimeSpan? timeout)
        {
            base.WithTimeout(timeout);
            return this;
        }

        public BasicToast WithBody(string body)
        {
            if (body.IsNullOrWhitespace())
            {
                Logger.LogWarning("No toast body text provided.");
                return this;
            }

            Logger.LogInformation("Setting toast body text: {0}", body);
            Body = body;
            return this;
        }

        public BasicToast WithIconPath(string iconPath)
        {
            if (iconPath.IsNullOrWhitespace())
            {
                Logger.LogWarning("No toast icon path provided.");
                return this;
            }

            Logger.LogInformation("Setting toast icon path: {0}", iconPath);
            IconPath = iconPath;
            return this;
        }
    }
}
