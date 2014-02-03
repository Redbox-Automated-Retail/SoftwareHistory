using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.Security;

internal interface IKeyProvider
{
	SecuritySession CreateSecuritySession(string name, HostInformation remote);
}
