using System.Runtime.InteropServices;

namespace Belikov.GenuineChannels.Security.SSPI;

[ComVisible(true)]
public enum SupportedSspiPackages
{
	Negotiate,
	NTLM,
	Kerberos
}
