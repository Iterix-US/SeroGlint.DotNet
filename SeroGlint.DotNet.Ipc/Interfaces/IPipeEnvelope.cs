using SeroGlint.DotNet.Ipc.Objects;

namespace SeroGlint.DotNet.Ipc.Interfaces
{
    public interface IPipeEnvelope<T>
    {
        /// <summary>
        /// Unique identifier for the message.
        /// </summary>
        T Payload { get; set; }

        /// <summary>
        /// Timestamp of when the message was created.
        /// </summary>
        /// <returns></returns>
        byte[] Serialize();

        /// <summary>
        /// Deserializes a JSON string into the specified target object type.
        /// </summary>
        /// <typeparam name="TTargetObject"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        PipeEnvelope<TTargetObject> Deserialize<TTargetObject>(string json);
    }
}
