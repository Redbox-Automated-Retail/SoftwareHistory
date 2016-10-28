using System.Runtime.InteropServices;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.Security;

[ComVisible(true)]
public class KeyProvider_SelfEstablishingSymmetric : IKeyProvider
{
	public SecuritySession CreateSecuritySession(string name, HostInformation remote)
	{
		return new SecuritySession_SelfEstablishingSymmetric(name, remote);
	}

	public override string ToString()
	{
		return $"KeyProvider_SelfEstablishingSymmetric.";
	}
}
