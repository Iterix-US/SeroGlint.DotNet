using System.Security.Principal;
using SeroGlint.DotNet.Common.Extensions;
using SeroGlint.DotNet.Windows.Abstractions;

namespace SeroGlint.DotNet.Windows.Wrappers
{
    public class WindowsPrincipalWrapper : IWindowsPrincipalWrapper
    {
        private readonly IPrincipal _principal;

        public WindowsPrincipalWrapper(IWindowsIdentityWrapper identityWrapper)
        {
            _principal = new WindowsPrincipal(identityWrapper.GetIdentity() as WindowsIdentity);
        }

        public bool IsInRole(WindowsBuiltInRole role)
        {
            return _principal.IsInRole(role.GetDescription());
        }
    }
}
