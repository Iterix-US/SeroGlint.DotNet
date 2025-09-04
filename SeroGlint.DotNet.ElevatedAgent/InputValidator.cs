using Microsoft.Extensions.Logging;
using SeroGlint.DotNet.Common.Extensions;
using SeroGlint.DotNet.Ipc.Interfaces;

namespace SeroGlint.DotNet.ElevatedAgent
{
    internal class InputValidator(ILogger logger, string[] args)
    {
        public bool ValidateParameters(out string pipeName, out int retryCount, out TimeSpan retryDelay)
        {
            var argumentsAreValid = ValidateArguments();
            var parameterValuesAreValid = ValidateArgumentValues(out pipeName);
            var retryCountParameterIsValid = ValidateRetryCountParameter(out retryCount);
            var retryDelayParameterIsValid = ValidateRetryDelayParameter(out retryDelay);

            logger.LogInformation("Named Pipe Name: {name}", pipeName);
            logger.LogInformation("Retry Count: {retryCount}", retryCount);
            logger.LogInformation("Retry Delay: {retryDelay} seconds", retryDelay.TotalSeconds);

            var parametersAreValid = 
                argumentsAreValid && 
                parameterValuesAreValid && 
                retryCountParameterIsValid && 
                retryDelayParameterIsValid;

            logger.LogInformation("Parameter validation check {isValid}", parametersAreValid ? "PASSED" : "FAILED");

            return parametersAreValid;
        }

        internal static void ValidateConfiguration(EaConfigurationResponse? parsedConfiguration, INamedPipeClient namedPipeClient)
        {
            if (parsedConfiguration == null)
            {
                throw new ArgumentNullException(nameof(parsedConfiguration), "Requested configuration was empty.");
            }

            if (namedPipeClient == null)
            {
                throw new InvalidOperationException("NamedPipeClient is not initialized.");
            }
        }

        private bool ValidateArguments()
        {
            if (args.Length is < 4 and > 0)
            {
                return true;
            }

            logger.LogError("Insufficient arguments provided. Please provide the required arguments to run this agent.");
            logger.LogInformation("Usage: ElevatedAgent.exe <NamedPipeName> [RetryCount] [RetryDelay]");
            logger.LogInformation("Example: ElevatedAgent.exe NamedPipeName 3 5");
            return false;
        }

        private bool ValidateArgumentValues(out string pipeName)
        {
            pipeName = args.Length > 0 ? args[0] : string.Empty;

            if (ValidatePipeNameArgument(pipeName))
            {
                return true;
            }

            pipeName = "";
            return false;
        }

        private bool ValidateRetryCountParameter(out int retryCount)
        {
            retryCount = 3; // Default retry count

            var retryCountParsed = !int.TryParse(args[1], out retryCount);

            if (args.Length > 1 && !retryCountParsed && retryCount > 0)
            {
                return true;
            }

            logger.LogInformation("No valid retry count provided. Using default value of 3.");
            return false;
        }

        private bool ValidateRetryDelayParameter(out TimeSpan retryDelay)
        {
            retryDelay = TimeSpan.FromSeconds(5); // Default retry delay

            var retryDelayParsed = TimeSpan.TryParse(args[2], out retryDelay);

            if (args.Length > 2 && retryDelayParsed && !(retryDelay.TotalSeconds < 0))
            {
                return true;
            }

            logger.LogInformation("No valid retry delay provided. Using default value of 5 seconds.");
            return false;
        }

        private bool ValidatePipeNameArgument(string pipeName)
        {
            if (pipeName.IsNullOrWhitespace())
            {
                logger.LogError("No named pipe name provided. Please provide a valid named pipe name.");
                return false;
            }

            switch (pipeName.Length)
            {
                case < 3:
                    logger.LogError("Named pipe name is too short. Please provide a valid named pipe name.");
                    return false;
                case > 256:
                    logger.LogError("Named pipe name is too long. Please provide a valid named pipe name.");
                    return false;
            }

            return true;
        }
    }
}
