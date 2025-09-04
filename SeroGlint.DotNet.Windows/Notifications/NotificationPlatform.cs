using System;
using System.Xml;
using Windows.UI.Notifications;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Uwp.Notifications;
using SeroGlint.DotNet.Notifications.Interfaces;
using SeroGlint.DotNet.Windows.Extensions;

namespace SeroGlint.DotNet.Windows.Notifications
{
    public class NotificationPlatform : INotificationPlatform
    {
        private readonly ToastContentBuilder _contentBuilder = new();
        private ToastNotifier _notifier;
        public ILogger Logger { get; }
        public string OperatingSystem => "Windows";
        public string SoftwareName { get; }
        public bool AppIsRegistered { get; private set; }

        public NotificationPlatform(ILogger logger, string softwareName)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            SoftwareName = softwareName;

            ConfirmAppRegistration();
        }

        private void ConfirmAppRegistration()
        {
            var registrar = new NotificationRegistrar();
            AppIsRegistered = registrar.VerifyAppUserModelIdRegistered(Logger, SoftwareName);

            if (!AppIsRegistered)
            {
                Logger.LogWarning("Application is not registered for notifications. Attempting to register.");
                registrar.RegisterAppUserModelId(Logger, SoftwareName);
            }
            else
            {
                Logger.LogInformation("Application is already registered for notifications.");
            }
        }

        public void PopToast(IToast payload)
        {
            CreateNotifier();

            Logger.LogInformation("Popping toast notification (id : {0}) on Windows.", payload.Id);
            _notifier.Show(payload.ToToast());
        }

        private void CreateNotifier()
        {
            _notifier = ToastNotificationManager.CreateToastNotifier(SoftwareName);
            var toastXml = new XmlDocument();
            toastXml.LoadXml(_contentBuilder!.GetToastContent().GetContent());
        }
    }
}
