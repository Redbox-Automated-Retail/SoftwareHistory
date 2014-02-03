using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.Security;

internal class KeyProvider_Basic : IKeyProvider
{
	public SecuritySession CreateSecuritySession(string name, HostInformation remote)
	{
		return new SecuritySession_Basic(name);
	}

	public override string ToString()
	{
		return "KeyProvider_Basic. No security features provided";
	}
}
