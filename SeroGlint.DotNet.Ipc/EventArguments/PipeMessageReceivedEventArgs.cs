using System;
using System.Diagnostics.CodeAnalysis;

namespace SeroGlint.DotNet.Ipc.EventArguments
{
    [ExcludeFromCodeCoverage]
    public class PipeMessageReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// The JSON representation of the received message.
        /// </summary>
        public string Json { get; }
        /// <summary>
        /// The deserialized message object that was received through the pipe.
        /// </summary>
        public object DeserializedMessage { get; }

        public PipeMessageReceivedEventArgs(string json, object deserializedMessage)
        {
            Json = json;
            DeserializedMessage = deserializedMessage;
        }
    }
}
