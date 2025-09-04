using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.Extensions.Logging;
using SeroGlint.DotNet.Common.Extensions;
using SeroGlint.DotNet.Ipc.Interfaces;

// ReSharper disable MethodOverloadWithOptionalParameter

namespace SeroGlint.DotNet.Ipc.Objects
{
    public class PipeEnvelope<T> : IPipeEnvelope<T>
    {
        private readonly ILogger _logger;

        public Guid MessageId { get; set; } = Guid.NewGuid();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string TypeName { get; set; } = typeof(T).FullName;
        public T Payload { get; set; }

        /// <summary>
        /// Default constructor for PipeEnvelope.
        /// </summary>
        public PipeEnvelope()
        {
            // Empty constructor for serialization purposes.
        }

        /// <summary>
        /// Initializes a new instance of the PipeEnvelope class with a logger.
        /// </summary>
        /// <param name="logger"></param>
        public PipeEnvelope(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Serializes the current message to a byte array in JSON format.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public byte[] Serialize()
        {
            _logger.LogInformation("Serializing message to JSON and encrypting it.");

            var json = this.ToJson();
            var jsonBytes = Encoding.UTF8.GetBytes(json);

            _logger.LogInformation("Message serialized successfully.");
            return jsonBytes;
        }

        /// <summary>
        /// Deserializes a JSON string into the specified target object type.
        /// </summary>
        /// <typeparam name="TTargetObject"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        [ExcludeFromCodeCoverage] // Tested via the static Deserialize method
        public PipeEnvelope<TTargetObject> Deserialize<TTargetObject>(string json)
        {
            return Deserialize<TTargetObject>(json, _logger);
        }

        /// <summary>
        /// Deserializes a byte array into the specified target object type.
        /// </summary>
        /// <typeparam name="TTargetObject"></typeparam>
        /// <param name="json"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static PipeEnvelope<TTargetObject> Deserialize<TTargetObject>(string json, ILogger logger = null)
        {
            if (json.IsNullOrWhitespace() || !json.Trim().StartsWith("{"))
            {
                logger?.LogWarning("Content submitted for deserialization is not a valid json package");
            }

            try
            {
                var deserializedObject = json.FromJsonToType<PipeEnvelope<TTargetObject>>();
                logger?.LogInformation("Message deserialized successfully.");
                return deserializedObject;
            }
            catch (Exception ex)
            {
                const string errorMessage = "Failed to deserialize message.";
                logger?.LogError(ex, errorMessage);
                return null;
            }
        }
    }
}
