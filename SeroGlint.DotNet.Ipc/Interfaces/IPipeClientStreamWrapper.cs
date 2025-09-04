using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace SeroGlint.DotNet.Ipc.Interfaces
{
    public interface IPipeClientStreamWrapper : IDisposable
    {
        Guid Id { get; }
        bool IsConnected { get; }
        NamedPipeClientStream ClientStream { get; }
        Task ConnectAsync(CancellationToken token);
        Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken token);
        Task FlushAsync(CancellationToken token);
        Task<int> ReadAsync(byte[] any, int i, int any1, CancellationToken cancellationToken);
    }
}
