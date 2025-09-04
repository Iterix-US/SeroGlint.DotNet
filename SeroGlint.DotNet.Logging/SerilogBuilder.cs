using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using SeroGlint.DotNet.Common;
using SeroGlint.DotNet.Common.Abstractions;
using SeroGlint.DotNet.Common.Extensions;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace SeroGlint.DotNet.Logging
{
    // Tested via <see cref="LoggerFactoryBuilder"/>, so the derived logger class doesn't need testing directly.
    [ExcludeFromCodeCoverage]
    public class SerilogBuilder : LoggerBuilderBase<SerilogBuilder, ILogger, LoggerConfiguration>
    {
        private readonly StringBuilder _logFallback = new StringBuilder();
        private readonly ILogger _logger;
        public override LoggerConfiguration Configuration { get; internal set; } = new LoggerConfiguration();

        /// <summary>
        /// Initializes a new instance of the <see cref="SerilogBuilder"/> class.
        /// </summary>
        /// <param name="fileManager">The wrapper for directory and file management; If none is passed in, a new one will be instantiated.</param>
        /// <param name="logger">If there's another logger being used, we can inject it to capture what's happening in this log builder</param>
        public SerilogBuilder(IDirectoryManagement fileManager = null, ILogger logger = null)
        {
            _logger = logger;
            FileManager = fileManager ?? throw new NullReferenceException("Directory manager cannot be null");
        }

        public override SerilogBuilder OutputToFile(bool createLogPath = false, string logPath = null, string logName = null, string logExtension = null)
        {
            EnableFileOutput = true;
            CreateLogPath = createLogPath;

            LogFilePath = string.IsNullOrWhiteSpace(logPath) ? AppContext.BaseDirectory : logPath;
            LogFileName = string.IsNullOrWhiteSpace(logName) ? "Log" : logName;
            LogFileExtension = string.IsNullOrWhiteSpace(logExtension?.Trim('.')) ? "log" : logExtension.Trim('.');

            AppendLog("Configuring logger to output to file:" +
                                   $"Log file path: {LogFilePath}" +
                                   $"Log file name: {LogFileName}" +
                                   $"Log file extension: {LogFileExtension}", LoggingLevel.Information);

            return this;
        }

        public override SerilogBuilder OutputToConsole()
        {
            AppendLog("Configuring logger to output to console.", LoggingLevel.Information);
            EnableConsoleOutput = true;
            return this;
        }

        public override SerilogBuilder WithMinimumLevel(LoggingLevel logLevel)
        {
            AppendLog($"Configuring logger to output at log level: {logLevel.GetDescription()}", LoggingLevel.Information);
            LogLevel = logLevel;
            return this;
        }

        public override SerilogBuilder WithRollingInterval(LogRollInterval rollingInterval)
        {
            AppendLog($"Configuring logger to roll files at interval: {rollingInterval.GetDescription()}", LoggingLevel.Information);
            RollingInterval = rollingInterval;
            return this;
        }

        public override SerilogBuilder WithFileSizeLimit(long fileSizeLimit)
        {
            AppendLog($"Configuring logger with file size limit: {fileSizeLimit} bytes", LoggingLevel.Information);
            FileSizeLimit = fileSizeLimit;
            return this;
        }

        public override SerilogBuilder WithFileRetentionLimit(int fileCount)
        {
            AppendLog($"Configuring logger to retain a maximum of {fileCount} log files.", LoggingLevel.Information);
            RetainedFileCountLimit = fileCount;
            return this;
        }

        public override SerilogBuilder WithOutputTemplate(string outputTemplate)
        {
            AppendLog($"Configuring logger to use output template: {outputTemplate}", LoggingLevel.Information);
            OutputTemplate = outputTemplate;
            return this;
        }

        public override SerilogBuilder WithConfiguration(LoggerConfiguration configuration)
        {
            AppendLog("Injecting custom configuration override into logger", LoggingLevel.Information);
            OverrideDefaultConfiguration = true;
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration), "Logger configuration cannot be null.");
            return this;
        }

        public override SerilogBuilder WithEnhancement(Func<LoggerConfiguration, LoggerConfiguration> enhancement)
        {
            if (enhancement == null)
            {
                AppendLog("Enhancement function cannot be null.", LoggingLevel.Error);
                throw new ArgumentNullException(nameof(enhancement), "Enhancement function cannot be null");
            }

            AppendLog("Applying enhancement to logger configuration.", LoggingLevel.Information);
            Configuration = enhancement(Configuration);
            return this;
        }

        public override SerilogBuilder WithCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                AppendLog("Category cannot be null or whitespace.", LoggingLevel.Error);
                throw new ArgumentException("Category cannot be null or whitespace.", nameof(category));
            }

            AppendLog($"Configuring logger with category: {category}", LoggingLevel.Information);
            Category = category;

            return this;
        }

        public override void AddAsService(IServiceCollection serviceCollection, ServiceLifetime lifetime)
        {
            if (serviceCollection == null)
            {
                AppendLog("Service collection cannot be null.", LoggingLevel.Error);
                throw new ArgumentNullException(nameof(serviceCollection), "Service collection cannot be null.");
            }

            if (!IsBuilt)
            {
                AppendLog("Logger has not been built yet. Building now.", LoggingLevel.Information);
                Build();
            }

            AppendLog($"Adding Serilog logger as a service with lifetime: {lifetime.GetDescription()}", LoggingLevel.Information);
            serviceCollection.AddLogging(logging => logging.AddSerilog());

            switch (lifetime)
            {
                case ServiceLifetime.Scoped:
                    AppendLog("Registering logger as a scoped service.", LoggingLevel.Information);
                    serviceCollection.AddScoped(_ => Instance);
                    break;
                case ServiceLifetime.Singleton:
                    AppendLog("Registering logger as a singleton service.", LoggingLevel.Information);
                    serviceCollection.AddSingleton(_ => Instance);
                    break;
                case ServiceLifetime.Transient:
                    AppendLog("Registering logger as a transient service.", LoggingLevel.Information);
                    serviceCollection.AddTransient(_ => Instance);
                    break;
                default:
                    AppendLog($"Unrecognized lifetime selection: {lifetime.GetDescription()}", LoggingLevel.Error);
                    throw new ArgumentOutOfRangeException(nameof(lifetime), "Unrecognized lifetime selection.");
            }
        }

        public override SerilogBuilder FromConfiguration(IConfiguration configuration)
        {
            if (configuration == null)
            {
                AppendLog("Configuration cannot be null.", LoggingLevel.Error);
                throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null.");
            }

            AppendLog("Loading logger configuration from provided IConfiguration instance.", LoggingLevel.Information);
            Configuration.ReadFrom.Configuration(configuration);
            OverrideDefaultConfiguration = true;
            return this;
        }

        public override void Reset()
        {
            AppendLog("Resetting logger configuration to default values.", LoggingLevel.Information);
            Configuration = new LoggerConfiguration();
            Instance = null;
            IsBuilt = false;
            OverrideDefaultConfiguration = false;
        }

        public override LoggerConfiguration BuildLoggerConfiguration()
        {
            if (OverrideDefaultConfiguration)
            {
                AppendLog("Using custom logger configuration provided by user.", LoggingLevel.Information);
                return Configuration;
            }

            if (EnableFileOutput)
            {
                AppendLog("Hydrating logger file output configuration", LoggingLevel.Information);

                Configuration.WriteTo.File(
                    LogFileLocation,
                    rollingInterval: RollingInterval.ToSerilogRollingInterval(),
                    fileSizeLimitBytes: FileSizeLimit,
                    retainedFileCountLimit: RetainedFileCountLimit,
                    rollOnFileSizeLimit: true,
                    outputTemplate: OutputTemplate);
            }

            if (EnableConsoleOutput)
            {
                AppendLog("Hydrating logger console output configuration", LoggingLevel.Information);

                Configuration.WriteTo.Console(
                    outputTemplate: OutputTemplate,
                    theme: Serilog.Sinks.SystemConsole.Themes.SystemConsoleTheme.Literate
                );
            }

            AppendLog($"Setting logger minimum level to: {LogLevel.GetDescription()}", LoggingLevel.Information);
            Configuration.MinimumLevel.Is(LogLevel.ToSerilogLevel());
            return Configuration;
        }

        public override ILogger Build()
        {
            if (Instance != null)
            {
                AppendLog("Returning existing logger instance.", LoggingLevel.Information);
                return Instance;
            }

            if (IsBuilt)
            {
                AppendLog("Logger has already been built. Create a new instance of the logger to build anew.", LoggingLevel.Error);
                throw new InvalidOperationException("Logger has already been built. Create a new instance of the logger to build anew.");
            }

            if (CreateLogPath && !FileManager.DirectoryExists(LogFilePath))
            {
                AppendLog($"Creating log directory at: {LogFilePath}", LoggingLevel.Information);
                FileManager.CreateDirectory(LogFilePath);
            }

            BuildLoggerConfiguration();

            AppendLog("Creating logger factory with Serilog configuration.", LoggingLevel.Information);
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                Log.Logger = Configuration.CreateLogger();
                builder.AddSerilog();
            });

            var effectiveCategory = Category ?? Assembly.GetExecutingAssembly().GetName().Name ?? "AppLogger";
            var logger = loggerFactory.CreateLogger(effectiveCategory);
            AppendLog($"Logger created with category: {effectiveCategory}", LoggingLevel.Information);

            const string loggerInitializedMessage = "Logger initialized with Serilog";
            logger.LogInformation(loggerInitializedMessage);
            logger.LogDebug(loggerInitializedMessage);

            IsBuilt = true;
            Instance = logger;

            if (EnableFileOutput || EnableConsoleOutput)
            {
                Instance.LogInformation($"Logger location: {LogFileLocation}");
            }

            return logger;
        }

        /// <summary>
        /// Because the logger may not be instantiated yet, we also add the log message to a string builder so we can add it once the logger is built.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="level"></param>
        private void AppendLog(string message, LoggingLevel level)
        {
            switch (level)
            {
                case LoggingLevel.Information:
                    _logger?.LogInformation(message);
                    break;
                case LoggingLevel.Warning:
                    _logger?.LogWarning(message);
                    break;
                case LoggingLevel.Error:
                    _logger?.LogError(message);
                    break;
                case LoggingLevel.Debug:
                    _logger?.LogDebug(message);
                    break;
                case LoggingLevel.Fatal:
                    _logger?.LogCritical(message);
                    break;
                case LoggingLevel.Verbose:
                default:
                    _logger?.LogInformation(message);
                    break;
            }

            _logFallback.AppendLine(message);
        }
    }
}
