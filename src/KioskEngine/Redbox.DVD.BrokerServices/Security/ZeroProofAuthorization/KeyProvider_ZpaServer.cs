using System;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.Security.ZeroProofAuthorization;

internal class KeyProvider_ZpaServer : IKeyProvider
{
	private object _accessToLocalMembers = new object();

	private ZpaFeatureFlags _zpaFeatureFlags;

	private IAuthorizationManager _iAuthorizationManager;

	public ZpaFeatureFlags ZpaFeatureFlags
	{
		get
		{
			lock (_accessToLocalMembers)
			{
				return _zpaFeatureFlags;
			}
		}
		set
		{
			lock (_accessToLocalMembers)
			{
				_zpaFeatureFlags = value;
			}
		}
	}

	public IAuthorizationManager IAuthorizationManager
	{
		get
		{
			lock (_accessToLocalMembers)
			{
				return _iAuthorizationManager;
			}
		}
		set
		{
			lock (_accessToLocalMembers)
			{
				_iAuthorizationManager = value;
			}
		}
	}

	public KeyProvider_ZpaServer(ZpaFeatureFlags zpaFeatureFlags, IAuthorizationManager iAuthorizationManager)
	{
		_zpaFeatureFlags = zpaFeatureFlags;
		_iAuthorizationManager = iAuthorizationManager;
	}

	public SecuritySession CreateSecuritySession(string name, HostInformation remote)
	{
		return new SecuritySession_ZpaServer(name, remote, this);
	}

	public override string ToString()
	{
		return string.Format("KeyProvider_ZpaServer Features: {0}.", Enum.Format(typeof(ZpaFeatureFlags), ZpaFeatureFlags, "g"));
	}
}
