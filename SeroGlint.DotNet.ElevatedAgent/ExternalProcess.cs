using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SeroGlint.DotNet.Common.Extensions;
using SeroGlint.DotNet.Ipc.Interfaces;

namespace SeroGlint.DotNet.ElevatedAgent
{
    internal class ExternalProcess(ILogger logger, INamedPipeClient client)
    {
        internal async Task ExecuteRequestProcess(EaConfigurationResponse? parsedConfiguration)
        {
            if (client == null)
            {
                throw new InvalidOperationException("NamedPipeClient is not initialized.");
            }

            await client.SendAsync(new EaMessage()
                .WithOperationType("Event")
                .WithPipeName(client.PipeName)
                .WithMessage("Attempting to execute application with given configuration.")
                .BuildPipeEnvelope());

            logger.LogInformation("Executing application with the provided configuration...");

            await ExecuteApplicationLaunch(parsedConfiguration);
        }

        private async Task ExecuteApplicationLaunch(EaConfigurationResponse? parsedConfiguration)
        {
            InputValidator.ValidateConfiguration(parsedConfiguration, client);

            var processInfo = new ProcessStartInfo
            {
                FileName = parsedConfiguration!.ExecutablePath,
                Arguments = parsedConfiguration.ProgramArguments,
                UseShellExecute = true,
                Verb = parsedConfiguration.RunAsAdministrator ? "runas" : string.Empty
            };

            try
            {
                logger.LogInformation("Preparing to launch external process");
                await client.SendAsync(new EaMessage()
                    .WithOperationType("Log")
                    .WithPipeName(client.PipeName)
                    .WithMessage("Launching external process")
                    .BuildPipeEnvelope());

                var process = Process.Start(processInfo);

                if (process == null)
                {
                    logger.LogError("Failed to start the external process. Process is null.");
                    await client.SendAsync(new EaMessage()
                        .WithOperationType("OperationFailed")
                        .WithPipeName(client.PipeName)
                        .WithMessage("Failed to start the external process. Process is null.")
                        .BuildPipeEnvelope());
                }

                process!.EnableRaisingEvents = true;
                process.Exited += async (sender, e) => await ExternalProcessExited(sender, e);

                logger.LogInformation("External process launched successfully.");
                await client.SendAsync(new EaMessage()
                    .WithOperationType("Log")
                    .WithPipeName(client.PipeName)
                    .WithMessage("External process launched")
                    .BuildPipeEnvelope());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to execute the application with the provided configuration.");
                await client.SendAsync(new EaMessage()
                    .WithOperationType("OperationFailed")
                    .WithPipeName(client.PipeName)
                    .WithMessage($"Failed to execute the application: {ex.Message}")
                    .BuildPipeEnvelope());
            }
        }

        private async Task ExternalProcessExited(object? sender, EventArgs eventArgs)
        {
            if (sender is not Process process)
            {
                logger.LogError("External process exited, but the process object is null.");
                await client.SendAsync(new EaMessage()
                    .WithOperationType("OperationFailed")
                    .WithPipeName(client.PipeName)
                    .WithMessage("External process exited, but the process object is null.")
                    .BuildPipeEnvelope());
                return;
            }

            logger.LogInformation("External process with ID {processId} exited.\n\nProcess handle: {processHandle}\n\nEvent arguments: {eventArgsJson}", process.Id, process.Handle, eventArgs.ToJson());
            await client.SendAsync(new EaMessage()
                .WithOperationType("Complete")
                .WithPipeName(client.PipeName)
                .WithMessage("External process exited.")
                .BuildPipeEnvelope());
        }
    }
}
