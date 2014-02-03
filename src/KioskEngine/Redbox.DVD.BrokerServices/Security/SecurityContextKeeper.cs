using System;

namespace Belikov.GenuineChannels.Security;

internal struct SecurityContextKeeper(SecuritySessionParameters securitySessionParameters) : IDisposable
{
	private SecuritySessionParameters _previousSecuritySessionParameters = SecuritySessionServices.SetCurrentSecurityContext(securitySessionParameters);

	public void Dispose()
	{
		SecuritySessionServices.SetCurrentSecurityContext(_previousSecuritySessionParameters);
	}
}
