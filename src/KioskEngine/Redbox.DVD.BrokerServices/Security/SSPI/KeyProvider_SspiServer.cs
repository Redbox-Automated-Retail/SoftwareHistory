using System;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.Security.SSPI;

internal class KeyProvider_SspiServer : IKeyProvider
{
	public SspiFeatureFlags RequiredFeatures;

	public string PackageName;

	public KeyProvider_SspiServer(SspiFeatureFlags requiredFeatures, SupportedSspiPackages packageName)
	{
		RequiredFeatures = requiredFeatures;
		PackageName = Enum.Format(typeof(SupportedSspiPackages), packageName, "f");
	}

	public SecuritySession CreateSecuritySession(string name, HostInformation remote)
	{
		return new SecuritySession_SspiServer(name, remote, this);
	}
}
