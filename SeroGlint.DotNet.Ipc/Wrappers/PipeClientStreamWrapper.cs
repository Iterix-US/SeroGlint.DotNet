using System;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using SeroGlint.DotNet.Ipc.Interfaces;

namespace SeroGlint.DotNet.Ipc.Wrappers
{
    [ExcludeFromCodeCoverage] // No need to test a wrapper pass-through
    public class PipeClientStreamWrapper : IPipeClientStreamWrapper
    {
        public Guid Id { get; } = Guid.NewGuid();
        public bool IsConnected => ClientStream?.IsConnected ?? false;
        public NamedPipeClientStream ClientStream { get; }

        public PipeClientStreamWrapper(NamedPipeClientStream pipeClientStream = null)
        {
            ClientStream = pipeClientStream;
        }

        public async Task ConnectAsync(CancellationToken token)
        {
            await ClientStream.ConnectAsync(token);
        }

        public async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken token)
        {
            await ClientStream.WriteAsync(buffer, offset, count, token);
        }

        public async Task FlushAsync(CancellationToken token)
        {
            await ClientStream.FlushAsync(token);
        }

        public void Dispose()
        {
            ClientStream?.Dispose();
        }

        public async Task<int> ReadAsync(byte[] buffer, int i, int bufferLength, CancellationToken token)
        {
            return await ClientStream.ReadAsync(buffer, i, bufferLength, token);
        }
    }
}
