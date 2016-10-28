using System;
using System.Runtime.InteropServices;
using Belikov.GenuineChannels.Connection;

namespace Belikov.GenuineChannels.Security;

[ComVisible(true)]
public class SecuritySessionParameters
{
	public readonly string Name;

	public readonly SecuritySessionAttributes Attributes;

	internal GenuineConnectionType _genuineConnectionType;

	internal string _connectionName;

	public readonly string RemoteTransportUser = string.Empty;

	public readonly TimeSpan Timeout = TimeSpan.MinValue;

	public TimeSpan CloseAfterInactivity = TimeSpan.MinValue;

	public int MaxContentSize = 0;

	public GenuineConnectionType GenuineConnectionType => _genuineConnectionType;

	public string ConnectionName => _connectionName;

	public SecuritySessionParameters(string securitySessionName)
	{
		Name = securitySessionName;
		_genuineConnectionType = GenuineConnectionType.Persistent;
	}

	public SecuritySessionParameters(string securitySessionName, SecuritySessionAttributes securitySessionAttributes, TimeSpan timeout)
	{
		Name = securitySessionName;
		Attributes = securitySessionAttributes;
		Timeout = timeout;
		_genuineConnectionType = GenuineConnectionType.Persistent;
	}

	public SecuritySessionParameters(string securitySessionName, SecuritySessionAttributes securitySessionAttributes, TimeSpan timeout, string remoteTransportUser)
	{
		Name = securitySessionName;
		Attributes = securitySessionAttributes;
		Timeout = timeout;
		_genuineConnectionType = GenuineConnectionType.Persistent;
		RemoteTransportUser = remoteTransportUser;
	}

	public SecuritySessionParameters(string securitySessionName, SecuritySessionAttributes securitySessionAttributes, TimeSpan timeout, GenuineConnectionType genuineConnectionType, string connecitonName, TimeSpan closeAfterInactivity)
	{
		Name = securitySessionName;
		Attributes = securitySessionAttributes;
		Timeout = timeout;
		_genuineConnectionType = genuineConnectionType;
		_connectionName = connecitonName;
		CloseAfterInactivity = closeAfterInactivity;
	}

	public SecuritySessionParameters(string securitySessionName, SecuritySessionAttributes securitySessionAttributes, TimeSpan timeout, GenuineConnectionType genuineConnectionType, string connecitonName, TimeSpan closeAfterInactivity, string remoteTransportUser)
	{
		Name = securitySessionName;
		Attributes = securitySessionAttributes;
		Timeout = timeout;
		_genuineConnectionType = genuineConnectionType;
		_connectionName = connecitonName;
		CloseAfterInactivity = closeAfterInactivity;
		RemoteTransportUser = remoteTransportUser;
	}

	public override string ToString()
	{
		return string.Format("SecuritySessionParameters (Name: {0}; Attributes: {1}; Connection name: {2}; Remote transport user: {3}; Timeout: {4} ms; Close after inactivity: {5} ms; Max content size: {6}", Name, Enum.Format(typeof(GenuineConnectionType), GenuineConnectionType, "g"), ConnectionName, RemoteTransportUser, Timeout.TotalMilliseconds, CloseAfterInactivity.TotalMilliseconds, MaxContentSize);
	}
}
