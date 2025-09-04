using SeroGlint.DotNet.Notifications.Interfaces;

namespace SeroGlint.DotNet.Notifications.Toasts
{
    public class ProgressToast : ToastBase<ProgressToast>, IProgressToast
    {
        public decimal MinValue { get; } = 0;
        public decimal MaxValue { get; } = 100;
        public decimal CurrentValue { get; set; } = 0;

        public void Update()
        {
            // Resend notification, but with new values.
        }
    }
}
