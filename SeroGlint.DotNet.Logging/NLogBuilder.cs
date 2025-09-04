using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Targets;
using SeroGlint.DotNet.Common;
using SeroGlint.DotNet.Common.Abstractions;
using SeroGlint.DotNet.Common.Extensions;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace SeroGlint.DotNet.Logging
{
    // Tested via <see cref="LoggerFactoryBuilder"/>, so the derived logger class doesn't need testing directly.
    [ExcludeFromCodeCoverage]
    public class NLogBuilder : LoggerBuilderBase<NLogBuilder, ILogger, LoggingConfiguration>
    {
        public NLogBuilder(IDirectoryManagement fileManager)
        {
            FileManager = fileManager ?? throw new NullReferenceException("Directory manager cannot be null");
        }

        public override LoggingConfiguration Configuration { get; internal set; } = new LoggingConfiguration();

        public override NLogBuilder OutputToFile(bool createLogPath = false, string logPath = null, string logName = null, string logExtension = null)
        {
            EnableFileOutput = true;
            CreateLogPath = createLogPath;

            LogFilePath = string.IsNullOrWhiteSpace(logPath) ? AppContext.BaseDirectory : logPath;
            LogFileName = string.IsNullOrWhiteSpace(logName) ? "Log" : logName;
            LogFileExtension = string.IsNullOrWhiteSpace(logExtension?.TrimStart('.')) ? "log" : logExtension.TrimStart('.');

            return this;
        }

        public override NLogBuilder OutputToConsole()
        {
            EnableConsoleOutput = true;
            return this;
        }

        public override NLogBuilder WithMinimumLevel(LoggingLevel logLevel)
        {
            LogLevel = logLevel;
            return this;
        }

        public override NLogBuilder WithRollingInterval(LogRollInterval rollingInterval)
        {
            RollingInterval = rollingInterval;
            return this;
        }

        public override NLogBuilder WithFileSizeLimit(long fileSizeLimit)
        {
            FileSizeLimit = fileSizeLimit;
            return this;
        }

        public override NLogBuilder WithFileRetentionLimit(int fileCount)
        {
            RetainedFileCountLimit = fileCount;
            return this;
        }

        public override NLogBuilder WithOutputTemplate(string outputTemplate)
        {
            OutputTemplate = outputTemplate;
            return this;
        }

        public override NLogBuilder WithConfiguration(LoggingConfiguration configuration)
        {
            OverrideDefaultConfiguration = true;
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            return this;
        }

        public override NLogBuilder WithEnhancement(Func<LoggingConfiguration, LoggingConfiguration> enhancement)
        {
            Configuration = enhancement?.Invoke(Configuration)
                            ?? throw new ArgumentNullException(nameof(enhancement));
            return this;
        }

        public override NLogBuilder WithCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Category cannot be null or whitespace.", nameof(category));

            Category = category;
            return this;
        }

        public override void AddAsService(IServiceCollection serviceCollection, ServiceLifetime lifetime)
        {
            if (!IsBuilt)
            {
                Build();
            }

            serviceCollection.AddLogging(builder => builder.ClearProviders().AddNLog());

            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    serviceCollection.AddSingleton(_ => Instance);
                    break;
                case ServiceLifetime.Scoped:
                    serviceCollection.AddScoped(_ => Instance);
                    break;
                case ServiceLifetime.Transient:
                    serviceCollection.AddTransient(_ => Instance);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lifetime));
            }
        }

        public override NLogBuilder FromConfiguration(IConfiguration configuration)
        {
            Configuration = new NLogLoggingConfiguration(configuration.GetSection("NLog"));
            OverrideDefaultConfiguration = true;
            return this;
        }

        public override void Reset()
        {
            Configuration = new LoggingConfiguration();
            Instance = null;
            IsBuilt = false;
            OverrideDefaultConfiguration = false;
        }

        public override LoggingConfiguration BuildLoggerConfiguration()
        {
            if (OverrideDefaultConfiguration)
                return Configuration;

            if (EnableConsoleOutput)
            {
                var consoleTarget = new ColoredConsoleTarget("console")
                {
                    Layout = OutputTemplate
                };
                Configuration.AddTarget(consoleTarget);
                Configuration.AddRule(LogLevel.ToNLogLevel(), NLog.LogLevel.Fatal, consoleTarget);
            }

            if (EnableFileOutput)
            {
                var fileTarget = new FileTarget("file")
                {
                    FileName = Path.Combine(LogFilePath, $"{LogFileName}.{LogFileExtension}"),
                    ArchiveEvery = RollingInterval.ToNLogArchivePeriod(),
                    ArchiveNumbering = ArchiveNumberingMode.Rolling,
                    ArchiveFileName = Path.Combine(LogFilePath, $"{LogFileName}.{{#}}.{LogFileExtension}"),
                    MaxArchiveFiles = RetainedFileCountLimit,
                    ArchiveAboveSize = FileSizeLimit,
                    Layout = OutputTemplate,
                    ConcurrentWrites = true,
                    KeepFileOpen = false
                };
                Configuration.AddTarget(fileTarget);
                Configuration.AddRule(LogLevel.ToNLogLevel(), NLog.LogLevel.Fatal, fileTarget);
            }

            LogManager.Configuration = Configuration;
            return Configuration;
        }

        public override ILogger Build()
        {
            if (Instance != null)
                return Instance;

            if (IsBuilt)
                throw new InvalidOperationException("Logger has already been built.");

            if (CreateLogPath && !Directory.Exists(LogFilePath))
            {
                Directory.CreateDirectory(LogFilePath);
            }

            BuildLoggerConfiguration();

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.ClearProviders();
                builder.AddNLog();
            });

            var effectiveCategory = Category ?? Assembly.GetExecutingAssembly().GetName().Name ?? "AppLogger";
            var logger = loggerFactory.CreateLogger(effectiveCategory);

            Instance = logger;
            IsBuilt = true;

            if (EnableConsoleOutput || EnableFileOutput)
            {
                Instance.LogInformation($"Logger location: {Path.Combine(LogFilePath, $"{LogFileName}.{LogFileExtension}")}");
            }

            return logger;
        }
    }
}
