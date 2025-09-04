using System;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipes;
using SeroGlint.DotNet.Ipc.Objects;

namespace SeroGlint.DotNet.Ipc.EventArguments
{
    [ExcludeFromCodeCoverage]
    public class PipeResponseRequestedEventArgs : EventArgs
    {
        /// <summary>
        /// Unique identifier for the correlation of the request and response.
        /// </summary>
        public Guid Id { get; }
        /// <summary>
        /// The response object that is being requested to be sent back through the pipe.
        /// </summary>
        public PipeEnvelope<dynamic> ResponseObject { get; }
        /// <summary>
        /// The named pipe stream through which the response should be sent.
        /// </summary>
        public NamedPipeServerStream Stream { get; }

        public PipeResponseRequestedEventArgs(Guid correlationId, PipeEnvelope<dynamic> responseObject, NamedPipeServerStream stream)
        {
            Id = correlationId;
            ResponseObject = responseObject;
            Stream = stream;
        }
    }
}
