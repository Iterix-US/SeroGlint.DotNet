using Microsoft.Extensions.Logging;
using SeroGlint.DotNet.Notifications.Interfaces;

namespace SeroGlint.DotNet.Notifications.Builders
{
    public class BasicToastBuilder
    {
        private IBasicToast _basicToast;
        private ILogger _logger;

        public BasicToastBuilder WithLogger(ILogger logger)
        {
            _logger = logger;
            return this;
        }

        public virtual IBasicToast Build()
        {
            return _basicToast;
        }
    }
}
