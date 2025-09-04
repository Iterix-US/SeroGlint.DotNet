namespace SeroGlint.DotNet.Notifications.Interfaces
{
    public interface IBasicToast : IToast
    {
        /// <summary>
        /// Gets the body text of the toast, which provides additional information or context.
        /// </summary>
        string Body { get; }
        /// <summary>
        /// Gets the icon path for the toast, which is used to display an image or icon alongside the toast content.
        /// </summary>
        string IconPath { get; }
    }
}
