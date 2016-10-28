using System;
using System.Runtime.InteropServices;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.Security.ZeroProofAuthorization;

[ComVisible(true)]
public class KeyProvider_ZpaClient : IKeyProvider
{
	private object _accessToLocalMembers = new object();

	private ZpaFeatureFlags _zpaFeatureFlags;

	private object _login;

	private string _password;

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

	public object Login
	{
		get
		{
			lock (_accessToLocalMembers)
			{
				return _login;
			}
		}
		set
		{
			lock (_accessToLocalMembers)
			{
				_login = value;
			}
		}
	}

	public string Password
	{
		get
		{
			lock (_accessToLocalMembers)
			{
				return _password;
			}
		}
		set
		{
			lock (_accessToLocalMembers)
			{
				_password = value;
			}
		}
	}

	public KeyProvider_ZpaClient(ZpaFeatureFlags zpaFeatureFlags, object login, string password)
	{
		_zpaFeatureFlags = zpaFeatureFlags;
		_login = login;
		_password = password;
	}

	public SecuritySession CreateSecuritySession(string name, HostInformation remote)
	{
		return new SecuritySession_ZpaClient(name, remote, this);
	}

	public override string ToString()
	{
		return string.Format("KeyProvider_ZpaClient Features: {0}.", Enum.Format(typeof(ZpaFeatureFlags), ZpaFeatureFlags, "g"));
	}
}
