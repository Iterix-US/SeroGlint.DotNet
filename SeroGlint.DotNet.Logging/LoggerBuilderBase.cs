using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SeroGlint.DotNet.Common;
using SeroGlint.DotNet.Common.Abstractions;
using SeroGlint.DotNet.Logging.LoggerInterfaces;

namespace SeroGlint.DotNet.Logging
{
    /// <summary>
    /// Properties of the logger builders are documented, however it's best to leverage the methods exposed here.
    /// To begin working with any logger in this library, simply write the name of your logger builder variable,
    /// then access the functions that start as ".With" to further tailor the logging experience.
    /// 
    /// Defaults:
    ///     Output to console: false,
    ///     Output to file: false,
    ///     Log path: AppContext.BaseDirectory,
    ///     Log file name: AppLog,
    ///     Log file extension: .log,
    ///     Log file location: AppContext.BaseDirectory/AppLog.log,
    ///     Create log path: false,
    ///     Category: Assembly.GetExecutingAssembly().GetName().Name ?? "AppLogger",
    ///     Override default configuration: false,
    ///     Log file count limit: 31,
    ///     Log file size limit: 10 MB,
    ///     Log rolling interval: Monthly,
    ///     Log level: Debug,
    ///     Log output template: {Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception},
    ///     Configuration: Initially null, hydrated by property definitions at the time of the <see cref="Build"/> process
    /// </summary>
    public abstract class LoggerBuilderBase<TLoggerBuilder, TLogger, TConfiguration> : ILoggerBuilder<TLoggerBuilder, TLogger, TConfiguration> 
        where TLoggerBuilder : class 
        where TLogger : class 
        where TConfiguration : class
    {
        public virtual TLogger Instance { get; internal set; }

        /// <summary>
        /// Gets the current logger configuration, which includes settings such as output paths, log levels, and other configurations.
        /// </summary>
        public abstract TConfiguration Configuration { get; internal set; }

        /// <summary>
        /// The file manager used for handling file operations. This is useful for abstracting file system interactions, such as checking if a file exists or creating directories.
        /// </summary>
        public IDirectoryManagement FileManager { get; internal set; }

        /// <summary>
        /// Indicates whether the logger has been built. This is used to prevent multiple builds of the same logger configuration.
        /// </summary>
        public bool IsBuilt { get; internal set; }

        /// <summary>
        /// The path where log files will be stored. If not specified, defaults to the application's base directory.
        /// </summary>
        protected string LogFilePath { get; set; } = AppContext.BaseDirectory;

        /// <summary>
        /// The name of the log file. If not specified, defaults to "AppLog".
        /// </summary>
        protected string LogFileName { get; set; } = "AppLog";

        /// <summary>
        /// The file extension for the log file. If not specified, defaults to ".log".
        /// </summary>
        protected string LogFileExtension { get; set; } = "log";

        /// <summary>
        /// Constructs the location of the log file based on the specified log path, name, and extension.
        /// </summary>
        protected string LogFileLocation =>
            string.IsNullOrWhiteSpace(LogFilePath) ? 
                Path.Combine(AppContext.BaseDirectory, $"{LogFileName}.{LogFileExtension.TrimStart('.')}") :
                Path.Combine(LogFilePath, $"{LogFileName}.{LogFileExtension.TrimStart('.')}");

        /// <summary>
        /// The minimum logging level for the logger. This determines which log entries are captured based on their severity.
        /// </summary>
        protected LoggingLevel LogLevel { get; set; } = LoggingLevel.Debug;

        /// <summary>
        /// The interval at which log files are rolled over. This can be set to daily, weekly, monthly, etc.
        /// </summary>
        protected LogRollInterval RollingInterval { get; set; } = LogRollInterval.Monthly;

        /// <summary>
        /// The output template for log entries. This defines how log messages are formatted when written to the output (console or file).
        /// Defaulted to "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}".
        /// </summary>
        protected string OutputTemplate { get; set; } = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

        /// <summary>
        /// The category for the logger, which is typically used to group log entries by application or module.
        /// </summary>
        protected string Category { get; set; } = Assembly.GetExecutingAssembly().GetName().Name ?? "AppLogger";

        /// <summary>
        /// Indicates whether console output is enabled for the logger. If true, log entries will be written to the console.
        /// </summary>
        protected bool EnableConsoleOutput { get; set; }

        /// <summary>
        /// Indicates whether file output is enabled for the logger. If true, log entries will be written to a file.
        /// </summary>
        protected bool EnableFileOutput { get; set; }

        /// <summary>
        /// If true, the logger will create the log directory if it does not exist. This is useful for ensuring that log files can be written without errors due to missing directories.
        /// </summary>
        protected bool CreateLogPath { get; set; }

        /// <summary>
        /// If true, the logger will override the default configuration settings. This allows for custom configurations to be applied without using the defaults.
        /// </summary>
        protected bool OverrideDefaultConfiguration { get; set; }

        /// <summary>
        /// The maximum number of log files to retain. When this limit is reached, the oldest log file will be deleted to make room for new logs.
        /// </summary>
        protected int RetainedFileCountLimit { get; set; } = 31;

        /// <summary>
        /// The maximum size of a log file in bytes. When a log file exceeds this size, it will be rolled over to a new file.
        /// </summary>
        protected long FileSizeLimit { get; set; } = 10485760; // 10 MB

        public abstract TLoggerBuilder OutputToFile(bool createLogPath = false, string logPath = null, string logName = null, string logExtension = null);
        public abstract TLoggerBuilder OutputToConsole();
        public abstract TLoggerBuilder WithMinimumLevel(LoggingLevel logLevel);
        public abstract TLoggerBuilder WithRollingInterval(LogRollInterval rollingInterval);
        public abstract TLoggerBuilder WithFileSizeLimit(long fileSizeLimit);
        public abstract TLoggerBuilder WithFileRetentionLimit(int fileCount);
        public abstract TLoggerBuilder WithOutputTemplate(string outputTemplate);
        public abstract TLoggerBuilder WithConfiguration(TConfiguration configuration);
        public abstract TLoggerBuilder WithEnhancement(Func<TConfiguration, TConfiguration> enhancement);
        public abstract TLoggerBuilder WithCategory(string category);
        public abstract void AddAsService(IServiceCollection serviceCollection, ServiceLifetime lifetime);
        public abstract TLoggerBuilder FromConfiguration(IConfiguration configuration);
        public abstract void Reset();

        public abstract TConfiguration BuildLoggerConfiguration();
        public abstract TLogger Build();
    }
}
