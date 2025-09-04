using Microsoft.Extensions.Logging;
using SeroGlint.DotNet.Ipc;
using SeroGlint.DotNet.Ipc.Interfaces;

namespace SeroGlint.DotNet.Tests.TestClasses.NamedPipes.TestObjects
{
    internal class TestableNamedPipeClient(
        INamedPipeConfiguration config,
        ILogger logger,
        IPipeClientStreamWrapper? wrapper = null)
        : NamedPipeClient(config, logger, wrapper)
    {
        public override async Task<string> SendAsync<T>(IPipeEnvelope<T> message)
        {
            if (!ValidateMessageSettings(Configuration.ServerName, Configuration.PipeName, message))
            {
                return "Pipe envelope was empty. Refusing to send.";
            }

            return await base.SendAsync(message);
        }
    }
}
