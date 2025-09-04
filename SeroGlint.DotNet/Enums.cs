using System.ComponentModel;

namespace SeroGlint.DotNet.Common
{
    /// <summary>
    /// Friendly names for the time of day.
    /// EarlyMorning = 12:00 AM - 5:59 AM
    /// Dawn = 6:00 AM - 6:59 AM
    /// Morning = 7:00 AM - 11:59 AM
    /// Afternoon = 12:00 PM - 4:59 PM
    /// Dusk = 5:00 PM - 5:59 PM
    /// Evening = 6:00 PM - 8:59 PM
    /// Night = 9:00 PM - 11:59 PM
    /// </summary>
    public enum TimeOfDay
    {
        [Description("Early Morning")] 
        EarlyMorning,
        [Description("Dawn")] 
        Dawn,
        [Description("Morning")] 
        Morning,
        [Description("Afternoon")] 
        Afternoon,
        [Description("Dusk")] 
        Dusk,
        [Description("Evening")] 
        Evening,
        [Description("Night")] 
        Night
    }

    /// <summary>
    /// Represents the logging levels used in the application.
    /// </summary>
    public enum LoggingLevel
    {
        [Description("Verbose")]
        Verbose = 0,
        [Description("Debug")]
        Debug = 1,
        [Description("Information")]
        Information = 2,
        [Description("Warning")]
        Warning = 3,
        [Description("Error")]
        Error = 4,
        [Description("Fatal")]
        Fatal = 5
    }

    /// <summary>
    /// Represents the intervals at which log files can be rolled over.
    /// </summary>
    public enum LogRollInterval
    {
        [Description("Indefinite")]
        Indefinite = 0,
        [Description("Yearly")]
        Yearly = 1,
        [Description("Monthly")]
        Monthly = 2,
        [Description("Daily")]
        Daily = 3,
        [Description("Hourly")]
        Hourly = 4
    }

    /// <summary>
    /// Represents the modes in which a pipe server can operate.
    /// </summary>
    public enum PipeServerMode
    {
        [Description("Momentary")]
        Momentary,
        [Description("Perpetual")]
        Perpetual
    }

    /// <summary>
    /// Represents the error codes used in the application to indicate various types of errors.
    /// </summary>
    public enum Error
    {
        [Description("No Error")]
        None = 0,
        [Description("Invalid Argument")]
        InvalidArgument = 100,
        [Description("Not Found")]
        NotFound = 101,
        [Description("Access Denied")]
        AccessDenied = 102,
        [Description("Timeout")]
        Timeout = 103,
        [Description("Unknown Error")]
        UnknownError = 104,
        [Description("Operation Failed")]
        OperationFailed = 105,
    }

    /// <summary>
    /// Represents the event IDs used for logging various events in the application.
    /// </summary>
    public enum LoggingEventId
    {
        [Description("")]
        None = 0,
        [Description("Server is not running. No need to stop.")]
        ServerNotRunning = 80862,
        [Description("Disposing Pipe")]
        DisposingExistingPipe = 65845,
        [Description("Event Id for testing purposes only")]
        ReusingExistingPipe = 80841,
        [Description("Write Pipe Error")]
        WritePipeError = 48846
    }

    /// <summary>
    /// Represents the type of interaction that triggered the toast event.
    /// </summary>
    public enum ToastInteractionType
    {
        Click,
        ButtonClick,
        Dismiss
    }
}