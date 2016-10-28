using System.Runtime.InteropServices;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.Security;

[ComVisible(true)]
public interface IKeyProvider
{
	SecuritySession CreateSecuritySession(string name, HostInformation remote);
}
