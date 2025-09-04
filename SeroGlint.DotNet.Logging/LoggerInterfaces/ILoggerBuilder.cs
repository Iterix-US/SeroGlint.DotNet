using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SeroGlint.DotNet.Common;
using SeroGlint.DotNet.Common.Abstractions;

namespace SeroGlint.DotNet.Logging.LoggerInterfaces
{
    public interface ILoggerBuilder<out TLoggerBuilder, out TLogger, TLoggerConfiguration> 
        where TLoggerBuilder : class 
        where TLogger : class
        where TLoggerConfiguration : class
    {
        /// <summary>
        /// Gets the current logger instance. This is the concrete logger that has been built and configured based on the provided settings.
        /// </summary>
        TLogger Instance { get; }

        /// <summary>
        /// Gets the current logger configuration, which includes settings such as output paths, log levels, and other configurations.
        /// </summary>
        TLoggerConfiguration Configuration { get; }

        /// <summary>
        /// Wraps file and directory management functionality to allow for mocking and testing.
        /// </summary>
        IDirectoryManagement FileManager { get; }

        /// <summary>
        /// Gets a value indicating whether the logger has been built and is ready for use.
        /// </summary>
        bool IsBuilt { get; }

        /// <summary>
        /// Outputs log entries to a file.
        /// </summary>
        /// <param name="createLogPath">True: if the directory does not exist, create it</param>
        /// <param name="logPath">The directory, file name, and file extension to be used for the log (e.g. C:\Some\Directory\SomeLog.log)</param>
        /// <param name="logName">The name of the log file. If not specified, defaults to "AppLog"</param>
        /// <param name="logExtension">The file extension for the log file. If not specified, defaults to ".log"</param>
        /// <returns>The current snapshot of the log builder</returns>
        TLoggerBuilder OutputToFile(bool createLogPath = false, string logPath = null, string logName = null, string logExtension = null);

        /// <summary>
        /// Outputs log entries to the console using the default output template.
        /// </summary>
        /// <returns>The current snapshot of the log builder</returns>
        TLoggerBuilder OutputToConsole();

        /// <summary>
        /// Sets the minimum log level for the logger.
        /// </summary>
        /// <param name="logLevel"><see cref="LoggingLevel"/> defined within this library which maps to specific log level implementations like Serilog and NLog</param>
        /// <returns>The current snapshot of the log builder</returns>
        TLoggerBuilder WithMinimumLevel(LoggingLevel logLevel);

        /// <summary>
        /// Sets the rolling interval for log files. This determines how often a new log file is created based on time intervals (e.g., daily, weekly, monthly).
        /// </summary>
        /// <param name="rollingInterval"></param>
        /// <returns></returns>
        TLoggerBuilder WithRollingInterval(LogRollInterval rollingInterval);

        /// <summary>
        /// Sets the maximum size of a log file. When the file size exceeds this limit, it will be rolled over to a new file.
        /// </summary>
        /// <param name="fileSizeLimit"></param>
        /// <returns>The current snapshot of the log builder</returns>
        TLoggerBuilder WithFileSizeLimit(long fileSizeLimit);

        /// <summary>
        /// Sets the maximum number of log files to retain. When the limit is reached, the oldest log file will be deleted to make room for new logs.
        /// </summary>
        /// <param name="fileCount"></param>
        /// <returns>The current snapshot of the log builder</returns>
        TLoggerBuilder WithFileRetentionLimit(int fileCount);

        /// <summary>
        /// Sets the output template for the logger, which defines how log entries are formatted when written to the output (e.g., console or file).
        /// </summary>
        /// <param name="outputTemplate"></param>
        /// <returns>The current snapshot of the log builder</returns>
        TLoggerBuilder WithOutputTemplate(string outputTemplate);

        /// <summary>
        /// Sets the logger configuration, allowing for complete customization of the logger's behavior and output.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns>The current snapshot of the log builder</returns>
        TLoggerBuilder WithConfiguration(TLoggerConfiguration configuration);

        /// <summary>
        /// Applies an enhancement function to the current logger configuration, allowing for dynamic modifications to the logger's settings.
        /// </summary>
        /// <param name="enhancement"></param>
        /// <returns>The current snapshot of the log builder</returns>
        TLoggerBuilder WithEnhancement(Func<TLoggerConfiguration, TLoggerConfiguration> enhancement);

        /// <summary>
        /// Sets the category for the logger, which is typically used to group logs by a specific context or module.
        /// </summary>
        /// <param name="category"></param>
        /// <returns>The current snapshot of the log builder</returns>
        TLoggerBuilder WithCategory(string category);

        /// <summary>
        /// Registers the logger as a service in the provided <see cref="IServiceCollection"/>. This allows the logger to be injected into other services or components using dependency injection.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/> to which the logger will be added as a service.</param>
        /// <param name="lifetime">Overrides the default lifetime of the logger service. This can be used to specify how long the logger instance should live within the application (e.g., Singleton, Scoped, Transient).</param>
        void AddAsService(IServiceCollection serviceCollection, ServiceLifetime lifetime);

        /// <summary>
        /// Builds the concrete logger instance based on the current configuration.
        /// </summary>
        /// <returns>The concrete logger instance</returns>
        TLogger Build();

        /// <summary>
        /// Creates a logger builder from the provided configuration settings. This allows for the logger to be configured based on external settings, such as those defined in an appsettings.json file or environment variables.
        /// </summary>
        /// <param name="configuration">The configuration object containing settings for the logger.</param>
        /// <returns>The current snapshot of the log builder based on the settings passed in.</returns>
        TLoggerBuilder FromConfiguration(IConfiguration configuration);

        /// <summary>
        /// Resets the logger builder to its initial state, allowing for a fresh configuration without creating a new instance.
        /// </summary>
        void Reset();

        /// <summary>
        /// Builds the logger configuration that can be used to create a logger instance.
        /// </summary>
        /// <returns>The logger configuration object</returns>
        TLoggerConfiguration BuildLoggerConfiguration();
    }
}
