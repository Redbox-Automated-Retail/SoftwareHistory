using System;
using Belikov.GenuineChannels.Connection;
using Belikov.GenuineChannels.DirectExchange;
using Belikov.GenuineChannels.Logbook;
using Belikov.GenuineChannels.Parameters;
using Belikov.GenuineChannels.Receiving;
using Belikov.GenuineChannels.Security;

namespace Belikov.GenuineChannels.TransportContext;

internal class TransportContext : MarshalByRefObject, ITransportContext, ISetSecuritySession
{
	private object _accessToLocalMembers = new object();

	internal IKeyStore _iKeyStore;

	internal ConnectionManager _connectionManager;

	internal IIncomingStreamHandler _iIncomingStreamHandler;

	internal DirectExchangeManager _directExchangeManager;

	internal KnownHosts _knownHosts;

	internal string _hostIdentifier;

	internal byte[] _binaryHostIdentifier;

	internal IParameterProvider _iParameterProvider;

	internal IGenuineEventProvider _iGenuineEventProvider;

	private SecuritySessionParameters _securitySessionParameters;

	private object _securitySessionParametersLock = new object();

	private SecuritySessionAttributes _prohibitedSecuritySessionAttributes = SecuritySessionAttributes.None;

	private object _prohibitedSecuritySessionAttributesLock = new object();

	private SecuritySessionAttributes _forcedSecuritySessionAttributes = SecuritySessionAttributes.None;

	private object _forcedSecuritySessionAttributesLock = new object();

	public IKeyStore IKeyStore => _iKeyStore;

	public ConnectionManager ConnectionManager => _connectionManager;

	public IIncomingStreamHandler IIncomingStreamHandler => _iIncomingStreamHandler;

	public DirectExchangeManager DirectExchangeManager => _directExchangeManager;

	public KnownHosts KnownHosts => _knownHosts;

	public string HostIdentifier => _hostIdentifier;

	public byte[] BinaryHostIdentifier => _binaryHostIdentifier;

	public IParameterProvider IParameterProvider => _iParameterProvider;

	public IGenuineEventProvider IGenuineEventProvider => _iGenuineEventProvider;

	public BinaryLogWriter BinaryLogWriter
	{
		get
		{
			lock (_accessToLocalMembers)
			{
				return GenuineLoggingServices.BinaryLogWriter;
			}
		}
		set
		{
			lock (_accessToLocalMembers)
			{
				GenuineLoggingServices.BinaryLogWriter = value;
			}
		}
	}

	public SecuritySessionParameters SecuritySessionParameters
	{
		get
		{
			lock (_securitySessionParametersLock)
			{
				return _securitySessionParameters;
			}
		}
		set
		{
			lock (_securitySessionParametersLock)
			{
				_securitySessionParameters = value;
			}
		}
	}

	public SecuritySessionAttributes ProhibitedSecuritySessionAttributes
	{
		get
		{
			lock (_prohibitedSecuritySessionAttributesLock)
			{
				return _prohibitedSecuritySessionAttributes;
			}
		}
		set
		{
			lock (_prohibitedSecuritySessionAttributesLock)
			{
				_prohibitedSecuritySessionAttributes = value;
			}
		}
	}

	public SecuritySessionAttributes ForcedSecuritySessionAttributes
	{
		get
		{
			lock (_forcedSecuritySessionAttributesLock)
			{
				return _forcedSecuritySessionAttributes;
			}
		}
		set
		{
			lock (_forcedSecuritySessionAttributesLock)
			{
				_forcedSecuritySessionAttributes = value;
			}
		}
	}

	internal TransportContext()
	{
		Guid defaultHostIdentifier = GenuineUtility.DefaultHostIdentifier;
		_binaryHostIdentifier = defaultHostIdentifier.ToByteArray();
		_hostIdentifier = defaultHostIdentifier.ToString("N");
	}

	public SecuritySessionParameters FixSecuritySessionParameters(SecuritySessionParameters securitySessionParameters)
	{
		SecuritySessionAttributes securitySessionAttributes = (securitySessionParameters.Attributes | ForcedSecuritySessionAttributes) & ~ProhibitedSecuritySessionAttributes;
		return new SecuritySessionParameters(securitySessionParameters.Name, securitySessionAttributes, TimeSpan.MinValue, securitySessionParameters.GenuineConnectionType, securitySessionParameters.ConnectionName, TimeSpan.MinValue, securitySessionParameters.RemoteTransportUser);
	}
}
