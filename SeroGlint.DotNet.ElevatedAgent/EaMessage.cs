using SeroGlint.DotNet.Ipc.Objects;

namespace SeroGlint.DotNet.ElevatedAgent
{
    public class EaMessage
    {
        public string Id { get; } = Guid.NewGuid().ToString();
        public string OperationType { get; private set; }
        public string Message { get; private set; }
        public string PipeName { get; private set; }
        public int RetryCount { get; private set; }
        public Type ResponseFormatType { get; private set; }
        public TimeSpan RetryDelay { get; private set; }
        public DateTime Timestamp { get; private set; } = DateTime.Now;

        public EaMessage WithOperationType(string operationType)
        {
            OperationType = operationType;
            return this;
        }

        public EaMessage WithRetryCount(int retryCount)
        {
            RetryCount = retryCount;
            return this;
        }

        public EaMessage WithRetryDelay(TimeSpan retryDelay)
        {
            RetryDelay = retryDelay;
            return this;
        }

        public EaMessage WithMessage(string message)
        {
            Message = message;
            return this;
        }

        public EaMessage WithPipeName(string pipeName)
        {
            PipeName = pipeName;
            return this;
        }

        public EaMessage WithResponseFormatType(Type responseFormatType)
        {
            ResponseFormatType = responseFormatType;
            return this;
        }

        public PipeEnvelope<EaMessage> BuildPipeEnvelope()
        {
            var connectionEnvelope = new PipeEnvelope<EaMessage>
            {
                Payload = this
            };

            return connectionEnvelope;
        }
    }
}
