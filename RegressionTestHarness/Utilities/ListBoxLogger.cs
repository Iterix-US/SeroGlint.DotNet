using System.IO;
using Microsoft.Extensions.Logging;
using SeroGlint.DotNet.Logging;
using SeroGlint.DotNet;
using System.Windows.Controls;
using System.Windows.Threading;
using SeroGlint.DotNet.Common;

namespace RegressionTestHarness.Utilities;

public class ListBoxLogger(string category, Action<string> logAction) : ILogger
{
    public IDisposable BeginScope<TState>(TState state) => null!;
    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId,
        TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var message = $"{DateTime.Now:HH:mm:ss} [{logLevel}] {category}: {formatter(state, exception)}";

        if (exception != null)
        {
            message += $" | {exception.GetType().Name}: {exception.Message}";
        }

        logAction(message);
    }

    internal static ILogger? RouteLoggerToListBox(Dispatcher dispatcher, ListBox targetListBox, string category, string fileSuffix)
    {
        var logPath = Path.Combine(Environment.CurrentDirectory, "SeroGlint_Logs");
        var coreLogger = new LoggerFactoryBuilder()
            .EnableConsoleOutput()
            .EnableFileOutput(true, logPath, $"THarness_{fileSuffix}", "sglog")
            .SetMinimumLevel(LoggingLevel.Debug)
            .SetOutputTemplate("{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .SetCategory(category)
            .BuildSerilog();

        var factory = LoggerFactory.Create(builder =>
        {
            builder.AddProvider(new ListBoxLoggerProvider(message => AppendLog(dispatcher, targetListBox, message)));
        });

        var logger = new CompositeLogger(coreLogger, factory.CreateLogger(category));
        logger.LogInformation("Log can be found in {directory}", logPath);
        return logger;
    }

    private static void AppendLog(Dispatcher dispatcher, ListBox listBoxControl, string message)
    {
        dispatcher.Invoke(() =>
        {
            listBoxControl.Items.Add(message);
            listBoxControl.ScrollIntoView(message);
        });
    }
}