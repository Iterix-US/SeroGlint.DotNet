namespace SeroGlint.DotNet.Notifications.Events
{
    /// <summary>
    /// Represents a delegate for handling toast interaction events.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public delegate void ToastInteractionTriggered(object sender, ToastInteractionTriggeredEventArgs args);
}
