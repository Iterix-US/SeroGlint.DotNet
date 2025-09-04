using System;
using System.Diagnostics;
using System.Security.Principal;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using SeroGlint.DotNet.Common.Abstractions;
using SeroGlint.DotNet.Windows.Registry.Interfaces;
using SeroGlint.DotNet.Windows.Registry;
using SeroGlint.DotNet.Windows.Abstractions;
using SeroGlint.DotNet.Windows.Wrappers;

namespace SeroGlint.DotNet.Windows.Notifications
{
    public class NotificationRegistrar
    {
        private readonly string _exePath;
        private readonly IRegistryKey _baseKey;
        private readonly IDirectoryManagement _directoryManager;
        private readonly IProcessWrapper _processWrapper;
        private readonly IEnvironmentWrapper _environmentWrapper;
        private readonly IWindowsPrincipalWrapper _windowsPrincipalWrapper;

        public NotificationRegistrar(RegistrarWrapperPackage registrarWrapperPackage = null)
        {
            _exePath = Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty;
            if (registrarWrapperPackage == null)
            {
                _baseKey = new RegistryKeyWrapper(Microsoft.Win32.Registry.CurrentUser);
                _windowsPrincipalWrapper = new WindowsPrincipalWrapper(new WindowsIdentityWrapper());
                return;
            }

            registrarWrapperPackage.ContributeWrappers(
                ref _baseKey,
                ref _directoryManager,
                ref _processWrapper,
                ref _environmentWrapper,
                ref _windowsPrincipalWrapper);
        }

        public bool VerifyAppUserModelIdRegistered(ILogger logger, string appName)
        {
            logger.LogInformation("Verifying app registration");
            if (IsAppUserModelIdRegistered(logger, appName))
            {
                return true;
            }

            logger.LogInformation("Verifying elevated privileges");
            if (IsRunningAsAdmin())
            {
                return false;
            }

            logger.LogInformation("Permissions not elevated. Requesting elevation.");
            RestartAsAdmin(logger);
            _environmentWrapper.Exit(0);

            return false;
        }

        public void RegisterAppUserModelId(ILogger logger, string appName)
        {
            var executableName = _directoryManager.GetFileName(_exePath);
            try
            {
                logger.LogInformation("Registering AppUserModelID");

                // Register the aumid
                using (var key = _baseKey.CreateSubKey($@"Software\Classes\AppUserModelId\{appName}"))
                {
                    key.SetValue("DisplayName", appName, RegistryValueKind.String);
                    key.SetValue("AppUserModelID", appName, RegistryValueKind.String);
                }
                logger.LogInformation("AppUserModelID registered successfully.");

                // Register the notification exe
                using (var key = _baseKey.CreateSubKey($@"Software\Classes\Applications\{executableName}"))
                {
                    key.SetValue("AppUserModelID", $"{appName}", RegistryValueKind.String);
                }

                logger.LogInformation("AppUserModelID registered successfully.");
            }
            catch (Exception ex)
            {
                logger.LogTrace(exception: ex, "Failed to register AppUserModelID.");
            }
        }

        private bool IsAppUserModelIdRegistered(ILogger logger, string appName)
        {
            logger.LogInformation("Looking for registrar key");
            using var key = _baseKey.OpenSubKey($@"Software\Classes\AppUserModelId\{appName}");
            return key != null;
        }

        private bool IsRunningAsAdmin()
        {
            return _windowsPrincipalWrapper.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void RestartAsAdmin(ILogger logger)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = _exePath,
                UseShellExecute = true,
                Verb = "runas"
            };

            try
            {
                _processWrapper.Start(startInfo);
            }
            catch (Exception ex)
            {
                logger.LogTrace(exception: ex, message: "Failed to request privilege elevation.");
            }
        }
    }
}
