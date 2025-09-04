using Microsoft.Extensions.Logging;
using SeroGlint.DotNet.Common.Extensions;
using SeroGlint.DotNet.Ipc;
using SeroGlint.DotNet.Ipc.Interfaces;
using SeroGlint.DotNet.Ipc.Objects;
using SeroGlint.DotNet.Logging;
using SeroGlint.DotNet.Security;

namespace SeroGlint.DotNet.ElevatedAgent
{
    internal static class Program
    {
        private static string _namedPipeName;
        private static int _retryCount;
        private static TimeSpan _retryDelay;
        private static ILogger? _logger;
        private static INamedPipeClient _namedPipeClient = new NamedPipeClient();
        private static ExternalProcess? _applicationExecutor;
        private static InputValidator? _inputValidator;

        private static async Task Main(string[] args)
        {
            _logger = new LoggerFactoryBuilder()
                .EnableConsoleOutput()
                .EnableFileOutput(createLogPath: true, logPath: "Logs", logName: "ElevatedAgent")
                .BuildSerilog();

            _inputValidator = new InputValidator(_logger, args);
            if (!_inputValidator.ValidateParameters(out _namedPipeName, out _retryCount, out _retryDelay))
            {
                return;
            }

            InitializeNamedPipeClient();
            var applicationRunConfiguration = await TryConnectNamedPipe();
            var parsedConfiguration = await ParseRunParameters(applicationRunConfiguration);

            _applicationExecutor = new ExternalProcess(_logger, _namedPipeClient);
            await _applicationExecutor.ExecuteRequestProcess(parsedConfiguration);
        }

        private static async Task<string> TryConnectNamedPipe()
        {
            var configurationResponse = string.Empty;
            var message = new EaMessage()
                .WithOperationType("GetConfig")
                .WithPipeName(_namedPipeName)
                .WithRetryCount(_retryCount)
                .WithRetryDelay(_retryDelay)
                .WithResponseFormatType(typeof(EaConfigurationResponse))
                .WithMessage("Request for external application launch configuration")
                .BuildPipeEnvelope();

            for (var i = 0; i < _retryCount; i++)
            {
                _logger?.LogInformation("Attempting to connect to the named pipe server...");
                _logger?.LogInformation("Named Pipe Name: {name}", _namedPipeName);
                _logger?.LogInformation("Connection Attempt: {count} of {retryCount}", i + 1, _retryCount);

                configurationResponse = await _namedPipeClient.SendAsync(message)! ?? "";

                if (!_namedPipeClient.IsConnected)
                {
                    Thread.Sleep(_retryDelay);
                    continue;
                }

                _logger?.LogInformation("Successfully connected to the named pipe server.");
            }

            return configurationResponse;
        }

        private static void InitializeNamedPipeClient()
        {
            var namedPipeConfiguration = new PipeClientConfiguration();
            namedPipeConfiguration.Initialize(
                serverName: ".",
                pipeName: _namedPipeName,
                encryptionService: new AesEncryptionService(AesEncryptionService.GenerateKey(), _logger),
                cancellationTokenSource: new CancellationTokenSource(),
                logger: _logger
            );

            _namedPipeClient = new NamedPipeClient(namedPipeConfiguration, _logger);
        }

        private static async Task<EaConfigurationResponse?> ParseRunParameters(string configurationResponse)
        {
            if (configurationResponse.IsNullOrWhitespace())
            {
                _logger?.LogError("No configuration response received from the named pipe server.");
                await _namedPipeClient.SendAsync(
                    new EaMessage()
                        .WithOperationType("OperationFailed")
                        .WithPipeName(_namedPipeName)
                        .WithMessage("No configuration response received from the named pipe server.")
                        .BuildPipeEnvelope());
                return null;
            }

            EaConfigurationResponse? response;
            try
            {
                response = configurationResponse.FromJsonToType<EaConfigurationResponse>();
                
                if (response == null)
                {
                    _logger?.LogError("Failed to parse the configuration response from the named pipe server.");
                    return null;
                }

                _logger?.LogInformation("Configuration response received from the named pipe server");
            }
            catch (Exception ex)
            {
                await _namedPipeClient.SendAsync(new EaMessage()
                    .WithOperationType("OperationFailed")
                    .WithPipeName(_namedPipeName)
                    .WithMessage($"Failed to parse the configuration response: {ex.Message}")
                    .BuildPipeEnvelope());
                _logger?.LogError(ex, "Failed to parse the configuration response from the named pipe server.");

                return null;
            }

            return response;
        }
    }
}
