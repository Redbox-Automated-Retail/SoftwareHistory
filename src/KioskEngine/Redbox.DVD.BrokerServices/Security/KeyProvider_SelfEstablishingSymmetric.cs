using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.Security;

internal class KeyProvider_SelfEstablishingSymmetric : IKeyProvider
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
