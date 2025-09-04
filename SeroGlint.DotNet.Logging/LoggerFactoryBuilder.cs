using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SeroGlint.DotNet.Common;
using SeroGlint.DotNet.Common.Abstractions;

namespace SeroGlint.DotNet.Logging
{
    /// <summary>
    /// A builder class for configuring and creating logger instances with various options such as console output, file output, log levels, and more.
    /// </summary>
    public class LoggerFactoryBuilder
    {
        private bool _enableConsoleOutput;
        private bool _enableFileOutput;
        private bool _createLogPath;
        private string _logPath = AppContext.BaseDirectory;
        private string _logName = "AppLog";
        private string _logExtension = "log";
        private LoggingLevel _logLevel = LoggingLevel.Debug;
        private LogRollInterval _rollingInterval = LogRollInterval.Monthly;
        private long _fileSizeLimit = 10 * 1024 * 1024; // 10 MB
        private int _retainedFileCountLimit = 31;
        private string _outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
        private string _category = Assembly.GetExecutingAssembly().GetName().Name ?? "AppLogger";
        private IConfiguration _configuration;

        public IDirectoryManagement FileManager { get; }

        public LoggerFactoryBuilder(IDirectoryManagement fileManager = null)
        {
            FileManager = fileManager ?? throw new NullReferenceException("Directory manager cannot be null");
        }

        /// <summary>
        /// Initializes a new instance of the LoggerFactoryBuilder class.
        /// </summary>
        /// <returns></returns>
        public LoggerFactoryBuilder EnableConsoleOutput()
        {
            _enableConsoleOutput = true;
            return this;
        }

        /// <summary>
        /// Enables file output for the logger. This allows log entries to be written to a file, with options for log path, name, and extension.
        /// </summary>
        /// <param name="createLogPath"></param>
        /// <param name="logPath"></param>
        /// <param name="logName"></param>
        /// <param name="logExtension"></param>
        /// <returns></returns>
        public LoggerFactoryBuilder EnableFileOutput(bool createLogPath = false, string logPath = null, string logName = null, string logExtension = null)
        {
            _enableFileOutput = true;
            _createLogPath = createLogPath;
            if (!string.IsNullOrWhiteSpace(logPath))
                _logPath = logPath;
            if (!string.IsNullOrWhiteSpace(logName))
                _logName = logName;
            if (!string.IsNullOrWhiteSpace(logExtension))
                _logExtension = logExtension.TrimStart('.');
            return this;
        }

        /// <summary>
        /// Sets the minimum logging level for the logger. This determines which log entries are captured based on their severity.
        /// </summary>
        /// <param name="logLevel"></param>
        /// <returns></returns>
        public LoggerFactoryBuilder SetMinimumLevel(LoggingLevel logLevel)
        {
            _logLevel = logLevel;
            return this;
        }

        /// <summary>
        /// Sets the rolling interval for log files. This determines how often log files are rolled over, such as daily, weekly, or monthly.
        /// </summary>
        /// <param name="rollingInterval"></param>
        /// <returns></returns>
        public LoggerFactoryBuilder SetRollingInterval(LogRollInterval rollingInterval)
        {
            _rollingInterval = rollingInterval;
            return this;
        }

        /// <summary>
        /// Sets the maximum size of a log file in bytes. When a log file exceeds this size, it will be rolled over to a new file.
        /// </summary>
        /// <param name="fileSizeLimit"></param>
        /// <returns></returns>
        public LoggerFactoryBuilder SetFileSizeLimit(long fileSizeLimit)
        {
            _fileSizeLimit = fileSizeLimit;
            return this;
        }

        /// <summary>
        /// Sets the maximum number of log files to retain. When this limit is reached, the oldest log file will be deleted to make room for new logs.
        /// </summary>
        /// <param name="fileCount"></param>
        /// <returns></returns>
        public LoggerFactoryBuilder SetFileRetentionLimit(int fileCount)
        {
            _retainedFileCountLimit = fileCount;
            return this;
        }

        /// <summary>
        /// Sets the output template for log entries. This defines how log messages are formatted when written to the output (console or file).
        /// </summary>
        /// <param name="outputTemplate"></param>
        /// <returns></returns>
        public LoggerFactoryBuilder SetOutputTemplate(string outputTemplate)
        {
            _outputTemplate = outputTemplate;
            return this;
        }

        /// <summary>
        /// Sets the category for the logger. This is typically used to group log entries by application or module, and can be useful for filtering logs in larger applications.
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public LoggerFactoryBuilder SetCategory(string category)
        {
            if (!string.IsNullOrWhiteSpace(category))
                _category = category;
            return this;
        }

        /// <summary>
        /// Sets the configuration for the logger factory builder. This allows for additional configuration settings to be applied from an IConfiguration instance, such as appsettings.json or environment variables.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public LoggerFactoryBuilder SetConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
            return this;
        }

        /// <summary>
        /// Builds a Serilog configuration based on the settings provided in this builder.
        /// </summary>
        /// <returns></returns>
        public ILogger BuildSerilog()
        {
            var builder = new SerilogBuilder(FileManager)
                .WithMinimumLevel(_logLevel)
                .WithRollingInterval(_rollingInterval)
                .WithFileSizeLimit(_fileSizeLimit)
                .WithFileRetentionLimit(_retainedFileCountLimit)
                .WithOutputTemplate(_outputTemplate)
                .WithCategory(_category);

            if (_enableConsoleOutput)
                builder.OutputToConsole();

            if (_enableFileOutput)
                builder.OutputToFile(_createLogPath, _logPath, _logName, _logExtension);

            if (_configuration != null)
                builder.FromConfiguration(_configuration);

            return builder.Build();
        }

        /// <summary>
        /// Builds an NLog configuration based on the settings provided in this builder.
        /// </summary>
        /// <returns></returns>
        public ILogger BuildNLog()
        {
            var builder = new NLogBuilder(FileManager)
                .WithMinimumLevel(_logLevel)
                .WithRollingInterval(_rollingInterval)
                .WithFileSizeLimit(_fileSizeLimit)
                .WithFileRetentionLimit(_retainedFileCountLimit)
                .WithOutputTemplate(_outputTemplate)
                .WithCategory(_category);

            if (_enableConsoleOutput)
                builder.OutputToConsole();

            if (_enableFileOutput)
                builder.OutputToFile(_createLogPath, _logPath, _logName, _logExtension);

            if (_configuration != null)
                builder.FromConfiguration(_configuration);

            return builder.Build();
        }
    }
}
