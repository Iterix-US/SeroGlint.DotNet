using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Extensions.Logging;
using SeroGlint.DotNet.Ipc.Interfaces;
using SeroGlint.DotNet.Security.Interfaces;

namespace SeroGlint.DotNet.Ipc.Objects
{
    /// <summary>
    /// Base class for configuring named pipe servers, providing default implementations and properties.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class NamedPipeConfigurationBase : INamedPipeConfiguration
    {
        public virtual ILogger Logger { get; set; }
        public virtual string Id { get; set; } = Guid.NewGuid().ToString("N");
        public virtual string ServerName { get; private set; } = ".";
        public virtual string PipeName { get; private set; } = $"{GetName()}-Pipe";
        public virtual bool UseEncryption { get; set; } = true;
        public virtual IEncryptionService EncryptionService { get; private set; }
        public virtual CancellationTokenSource CancellationTokenSource { get; private set; } = new CancellationTokenSource();

        private static string GetName() => AppDomain.CurrentDomain.FriendlyName.Split('.')[0];

        public void Initialize(
            string serverName, 
            string pipeName,
            ILogger logger,
            IEncryptionService encryptionService = null, 
            CancellationTokenSource cancellationTokenSource = null)
        {
            Logger = logger;
            ServerName = serverName ?? ServerName;
            PipeName = pipeName ?? PipeName;
            EncryptionService = encryptionService ?? EncryptionService;
            CancellationTokenSource = cancellationTokenSource ?? CancellationTokenSource;
        }

        public void Reset()
        {
            Id = Guid.NewGuid().ToString("N");
            CancellationTokenSource?.Cancel();
            CancellationTokenSource = new CancellationTokenSource();
        }
    }
}
