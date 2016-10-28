using System;
using System.Runtime.InteropServices;

namespace Belikov.GenuineChannels.Security;

[ComVisible(true)]
public struct SecurityContextKeeper(SecuritySessionParameters securitySessionParameters) : IDisposable
{
	private SecuritySessionParameters _previousSecuritySessionParameters = SecuritySessionServices.SetCurrentSecurityContext(securitySessionParameters);

	public void Dispose()
	{
		SecuritySessionServices.SetCurrentSecurityContext(_previousSecuritySessionParameters);
	}
}
