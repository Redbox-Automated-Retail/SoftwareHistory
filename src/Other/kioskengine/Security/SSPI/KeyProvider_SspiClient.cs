using System;
using System.Net;
using System.Runtime.InteropServices;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.Security.SSPI;

[ComVisible(true)]
public class KeyProvider_SspiClient : IKeyProvider
{
	public SspiFeatureFlags RequiredFeatures;

	public string PackageName;

	public NetworkCredential AuthIdentity;

	public string ServerName;

	public SecuritySession_SspiServer DelegatedContext;

	public KeyProvider_SspiClient(SspiFeatureFlags requiredFeatures, SupportedSspiPackages packageName, NetworkCredential authIdentity, string targetName)
		: this(requiredFeatures, packageName, authIdentity, targetName, null)
	{
	}

	public KeyProvider_SspiClient(SspiFeatureFlags requiredFeatures, SupportedSspiPackages packageName, NetworkCredential authIdentity, string targetName, SecuritySession_SspiServer delegatedContext)
	{
		RequiredFeatures = requiredFeatures;
		PackageName = Enum.Format(typeof(SupportedSspiPackages), packageName, "f");
		AuthIdentity = authIdentity;
		ServerName = targetName;
		DelegatedContext = delegatedContext;
	}

	public SecuritySession CreateSecuritySession(string name, HostInformation remote)
	{
		return new SecuritySession_SspiClient(name, remote, this);
	}

	public override string ToString()
	{
		return string.Format("KeyProvider_SspiClient Features: {0}; Package: {1}; Target name: {2}; Delegated context: {3}.", Enum.Format(typeof(SspiFeatureFlags), RequiredFeatures, "g"), PackageName, ServerName, (DelegatedContext == null) ? "<not specified>" : "<specified!>");
	}
}
