using System;
using System.Diagnostics.CodeAnalysis;
using SeroGlint.DotNet.Ipc.Interfaces;

namespace SeroGlint.DotNet.Ipc.EventArguments
{
    [ExcludeFromCodeCoverage] // Excluding because there's no actual logic here.
    public class PipeServerStateChangedEventArgs
    {
        public string ContextLabel { get; private set; }
        public string StateDescription { get; private set; }

        public static PipeServerStateChangedEventArgs SetPipeServerStopped(INamedPipeServer namedPipeServer)
        {
            return new PipeServerStateChangedEventArgs
            {
                ContextLabel = "Stopped",
                StateDescription = 
                    "Server is stopped and disposed. " +
                    $"Timestamp: {DateTime.Now}. " +
                    $"Configuration untouched. Server Id = {namedPipeServer.Id}"
            };
        }

        public static PipeServerStateChangedEventArgs SetPipeServerStarted(INamedPipeServer namedPipeServer)
        {
            return new PipeServerStateChangedEventArgs
            {
                ContextLabel = "Started",
                StateDescription = 
                    "Server is started and listening for connections. " +
                    $"Timestamp: {DateTime.Now}." +
                    $" Server Id = {namedPipeServer.Id}"
            };
        }

        public string GetLogMessage()
        {
            return $"{ContextLabel} | {StateDescription}";
        }
    }
}
