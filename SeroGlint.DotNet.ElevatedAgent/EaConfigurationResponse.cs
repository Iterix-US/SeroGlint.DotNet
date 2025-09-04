namespace SeroGlint.DotNet.ElevatedAgent
{
    public class EaConfigurationResponse
    {
        public string ExecutablePath { get; set; }
        public bool RunAsAdministrator { get; set; }
        public string ProgramArguments { get; set; }
    }
}
