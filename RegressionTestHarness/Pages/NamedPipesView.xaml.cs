using Microsoft.Extensions.Logging;
using System.Windows;
using RegressionTestHarness.Objects;
using RegressionTestHarness.Utilities;
using SeroGlint.DotNet.Common.Extensions;
using SeroGlint.DotNet.Ipc;
using SeroGlint.DotNet.Ipc.EventArguments;
using SeroGlint.DotNet.Ipc.Objects;
using SeroGlint.DotNet.Security;

namespace RegressionTestHarness.Pages
{
    /// <summary>
    /// Interaction logic for NamedPipesView.xaml
    /// </summary>
    public partial class NamedPipesView
    {
        private static readonly string EncryptionKey = AesEncryptionService.GenerateKey();

        private ILogger? _serverLogger;
        private ILogger? _clientLogger;
        private NamedPipeServer? _namedPipeServer;
        private AesEncryptionService? _encryptionService;
        private PipeServerConfiguration? _serverConfiguration;
        private PipeClientConfiguration? _clientConfiguration;
        private NamedPipeClient _namedPipeClient;

        public NamedPipesView()
        {
            InitializeComponent();
            InitializeLogger();
            InitializeServerConfiguration();
            InitializeServer();
            InitializeClient();
        }

        private void InitializeClient()
        {
            if (_clientConfiguration == null)
            {
                _clientConfiguration = new PipeClientConfiguration();
                _clientConfiguration.Initialize(
                    ".",
                    "TestPipe",
                    _clientLogger,
                    _encryptionService);
            }

            _namedPipeClient = new NamedPipeClient(_clientConfiguration, _clientLogger);
        }

        private void InitializeServerConfiguration()
        {
            _encryptionService = new AesEncryptionService(EncryptionKey, _serverLogger);

            _serverConfiguration = new PipeServerConfiguration();
            _serverConfiguration.Initialize(
                ".",
                "TestPipe",
                _serverLogger,
                _encryptionService);
        }

        private void InitializeLogger()
        {
            _serverLogger =
                ListBoxLogger.RouteLoggerToListBox(Dispatcher, LstServerLog, "NamedPipeServerTestHarness", "Server");
            _serverLogger?.LogInformation("Server logger initialized in test harness.");

            _clientLogger =
                ListBoxLogger.RouteLoggerToListBox(Dispatcher, LstClientLog, "NamedPipeClientTestHarness", "Client");
            _clientLogger?.LogInformation("Client logger initialized in test harness.");
        }

        private void InitializeServer()
        {
            _namedPipeServer = new NamedPipeServer(_serverConfiguration);
            _namedPipeServer.MessageReceived += NamedPipeServerMessageReceived;
            _namedPipeServer.ResponseRequested += NamedPipeServerResponseRequested;
            _namedPipeServer.ServerStateChanged += NamedPipeServerStateChanged;
        }

        private async void btnStartServer_Click(object sender, RoutedEventArgs e)
        {
            if (_namedPipeServer == null)
            {
                InitializeServer();
            }

            await _namedPipeServer!.StartAsync();
        }

        private void btnStopServer_Click(object sender, RoutedEventArgs e)
        {
            _namedPipeServer?.Stop();
            _serverConfiguration?.Reset();
        }

        // TODO: Add a client disconnect button/logic to break the connection with the server.

        private async void btnSendTestMessage_Click(object sender, RoutedEventArgs e)
        {
            var envelope = new PipeEnvelope<TestObject>(_clientLogger)
            {
                Payload = new TestObject()
            };
            
            var response = await _namedPipeClient.SendAsync(envelope);

            _clientConfiguration?.Logger.LogInformation($"FROM PIPE CLIENT- Message received: {response}");
        }

        private void btnClearServerLog_Click(object sender, RoutedEventArgs e)
        {
            LstServerLog.Items.Clear();
        }

        private void btnClearClientLog_Click(object sender, RoutedEventArgs e)
        {
            LstClientLog.Items.Clear();
        }

        private void NamedPipeServerMessageReceived(object sender, PipeMessageReceivedEventArgs args)
        {
            Dispatcher.Invoke(() =>
            {
                LstServerLog.Items.Add($"UI Server Log Capture- Received message: {args.Json}");
            });

            var parsedObject = args.Json.FromJsonToType<PipeEnvelope<TestObject>>();
            var logMessage = $"\n\tUI Server Log Capture- Parsed object:\n" +
                             $"\tType- {parsedObject?.TypeName ?? "null"}\n" +
                             $"\tName- {parsedObject?.Payload?.Name}\n" +
                             $"\tSource- {parsedObject?.Payload?.Source}\n" +
                             $"\tPayload Id- {parsedObject?.Payload?.Id}";

            _serverLogger?.LogInformation(logMessage);
        }

        private Task NamedPipeServerResponseRequested(object sender, PipeResponseRequestedEventArgs args)
        {
            var logMessage = $"\n\tUI Server Log Capture- Response requested\n" +
                             $"\tMessage Id: {args.ResponseObject.MessageId}\n" +
                             $"\t{args.ResponseObject.ToJson()}";

            _serverLogger?.LogInformation(logMessage);

            return Task.CompletedTask;
        }

        private Task NamedPipeServerStateChanged(object sender, PipeServerStateChangedEventArgs args)
        {
            var logMessage = args.GetLogMessage() ?? "Failure to change server state";
            _serverLogger?.LogInformation(logMessage);

                txtServerStatus.Text =
                    args.ContextLabel.EqualsIgnoreCase("started") ?
                        $"Server running ({_namedPipeServer?.Id})" :
                        "No server running";

            return Task.CompletedTask;
        }
    }
}
