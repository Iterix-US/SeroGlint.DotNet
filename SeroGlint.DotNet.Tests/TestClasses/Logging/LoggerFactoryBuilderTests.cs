using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog;
using NSubstitute;
using Serilog;
using SeroGlint.DotNet.Common;
using SeroGlint.DotNet.Common.Abstractions;
using SeroGlint.DotNet.FrameworkWrappers;
using SeroGlint.DotNet.Logging;
using SeroGlint.DotNet.Tests.Interfaces;
using SeroGlint.DotNet.Tests.Utilities;

namespace SeroGlint.DotNet.Tests.TestClasses.Logging
{
    public class LoggerFactoryBuilderTests : IDisposable
    {
        private readonly ILogFileCleaner _cleaner = new LogFileCleaner();
        private readonly string _testLogPath = @".\tmp\log_test_tagsw40k";

        public LoggerFactoryBuilderTests()
        {
            if (!Directory.Exists(_testLogPath))
            {
                Directory.CreateDirectory(_testLogPath);
            }
        }

        public void Dispose()
        {
            Thread.Sleep(100);
            _cleaner.TryDelete(FindFile());
        }

        private string FindFile()
        {
            return Directory
                .GetFiles(_testLogPath, "TestLog*.log")
                .FirstOrDefault() ?? string.Empty;
        }

        [Fact]
        public void LoggerFactoryBuilder_WhenBuildingSerilogWithFileAndConsole_ThenCreatesLogFileWithMessage()
        {
            var directoryManager = new DirectoryManagementWrapper();
            var builder = new LoggerFactoryBuilder(directoryManager)
                .EnableConsoleOutput()
                .EnableFileOutput(createLogPath: true, logPath: _testLogPath, logName: "TestLog", logExtension: "log")
                .SetMinimumLevel(LoggingLevel.Information)
                .SetRollingInterval(LogRollInterval.Monthly)
                .SetFileSizeLimit(5 * 1024 * 1024)
                .SetFileRetentionLimit(5)
                .SetOutputTemplate("{Timestamp:HH:mm:ss} {Message}{NewLine}")
                .SetCategory("TestCategory");

            var logger = builder.BuildSerilog();

            Assert.NotNull(logger);
            logger.LogInformation("This is a test log message.");

            // Force Serilog to flush and delay to ensure file is written
            Log.CloseAndFlush();
            Thread.Sleep(100);

            Assert.True(File.Exists(FindFile()), "Expected log file was not created.");
            var contents = File.ReadAllText(FindFile());
            Assert.Contains("This is a test log message", contents);
        }

        [Fact]
        public void LoggerFactoryBuilder_WhenBuildingNLogWithFileAndConsole_ThenCreatesLogFileWithMessage()
        {
            // Arrange
            var directoryManager = new DirectoryManagementWrapper();
            var builder = new LoggerFactoryBuilder(directoryManager)
                .EnableConsoleOutput()
                .EnableFileOutput(createLogPath: true, logPath: _testLogPath, logName: "TestLog", logExtension: "log")
                .SetMinimumLevel(LoggingLevel.Information)
                .SetRollingInterval(LogRollInterval.Monthly)
                .SetFileSizeLimit(5 * 1024 * 1024)
                .SetFileRetentionLimit(5)
                .SetOutputTemplate("{longdate} {message}{newline}")
                .SetCategory("TestCategory");

            // Act
            var logger = builder.BuildNLog();
            Assert.NotNull(logger);
            logger.LogInformation("This is a test log message from NLog.");

            // Let NLog flush and release handles
            LogManager.Flush();
            Thread.Sleep(100);

            // Assert
            Assert.True(File.Exists(FindFile()), "Expected log file was not created.");
        }

        [Fact]
        public void LoggerFactoryBuilder_WhenBuildingSerilogWithConfiguration_ThenUsesProvidedSettings()
        {
            var configBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection([
                    new KeyValuePair<string, string?>("Serilog:MinimumLevel:Default", "Warning")
                ]);

            var configuration = configBuilder.Build();

            var directoryManager = new DirectoryManagementWrapper();
            var builder = new LoggerFactoryBuilder(directoryManager)
                .EnableConsoleOutput()
                .EnableFileOutput(createLogPath: true, logPath: _testLogPath, logName: "TestLog", logExtension: "log")
                .SetConfiguration(configuration);

            var logger = builder.BuildSerilog();

            Assert.NotNull(logger);
        }

        [Fact]
        public void LoggerFactoryBuilder_WhenBuildingNLogWithConfiguration_ThenUsesProvidedSettings()
        {
            var configBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection([
                    new KeyValuePair<string, string?>("NLog:targets:file:type", "File"),
                    new KeyValuePair<string, string?>("NLog:targets:file:fileName", "TestLog.log"),
                    new KeyValuePair<string, string?>("NLog:targets:file:layout", "{message}"),
                    new KeyValuePair<string, string?>("NLog:rules:0:logger", "*"),
                    new KeyValuePair<string, string?>("NLog:rules:0:minLevel", "Info"),
                    new KeyValuePair<string, string?>("NLog:rules:0:writeTo", "file")
                ]);

            var configuration = configBuilder.Build();

            var directoryManager = new DirectoryManagementWrapper();
            var builder = new LoggerFactoryBuilder(directoryManager)
                .SetConfiguration(configuration);

            var logger = builder.BuildNLog();

            Assert.NotNull(logger);
        }

        [Fact]
        public void LoggerFactoryBuilder_WhenCreatedWithCustomFileManager_ThenFileManagerPropertyReturnsIt()
        {
            // Arrange
            var mockManager = Substitute.For<IDirectoryManagement>();
            var loggerBuilder = new LoggerFactoryBuilder(mockManager)
                .EnableConsoleOutput()
                .EnableFileOutput(createLogPath: true, logPath: _testLogPath, logName: "TestLog", logExtension: "log");

            var actual = loggerBuilder.FileManager;

            // Assert
            Assert.NotNull(actual);
        }
    }
}
