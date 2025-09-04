using SeroGlint.DotNet.Common.Abstractions;
using SeroGlint.DotNet.Common.Wrappers;
using SeroGlint.DotNet.FileManagement;
using SeroGlint.DotNet.FrameworkWrappers;
using SeroGlint.DotNet.Windows.Abstractions;
using SeroGlint.DotNet.Windows.Registry.Interfaces;
using SeroGlint.DotNet.Windows.Registry;

namespace SeroGlint.DotNet.Windows.Wrappers
{
    public class RegistrarWrapperPackage
    {
        private readonly IRegistryKey _baseKey;
        private readonly IDirectoryManagement _pathWrapper;
        private readonly IProcessWrapper _processWrapper;
        private readonly IWindowsPrincipalWrapper _windowsPrincipalWrapper;
        private readonly IEnvironmentWrapper _environmentWrapper;

        public RegistrarWrapperPackage(IRegistryKey baseKey = null,
            IDirectoryManagement pathWrapper = null,
            IProcessWrapper processWrapper = null,
            IWindowsIdentityWrapper windowsIdentityWrapper = null,
            IWindowsPrincipalWrapper windowsPrincipalWrapper = null,
            IEnvironmentWrapper environmentWrapper = null)
        {
            var identityWrapper = windowsIdentityWrapper ?? new WindowsIdentityWrapper();

            _baseKey = baseKey ?? new RegistryKeyWrapper(Microsoft.Win32.Registry.CurrentUser);
            _pathWrapper = pathWrapper ?? new DirectoryManagementWrapper();
            _processWrapper = processWrapper ?? new ProcessWrapper();
            _windowsPrincipalWrapper = windowsPrincipalWrapper ?? new WindowsPrincipalWrapper(identityWrapper);
            _environmentWrapper = environmentWrapper ?? new EnvironmentWrapper();
        }

        public void ContributeWrappers(
            ref IRegistryKey baseKey,
            ref IDirectoryManagement pathWrapper,
            ref IProcessWrapper processWrapper,
            ref IEnvironmentWrapper environmentWrapper,
            ref IWindowsPrincipalWrapper windowsPrincipalWrapper)
        {
            baseKey = _baseKey ?? new RegistryKeyWrapper(Microsoft.Win32.Registry.CurrentUser);
            pathWrapper = _pathWrapper ?? new DirectoryManagementWrapper();
            processWrapper = _processWrapper ?? new ProcessWrapper();
            environmentWrapper = _environmentWrapper ?? new EnvironmentWrapper();
            windowsPrincipalWrapper = _windowsPrincipalWrapper ?? new WindowsPrincipalWrapper(new WindowsIdentityWrapper());
        }
    }
}
