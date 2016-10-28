using System.Runtime.InteropServices;
using Belikov.GenuineChannels.Connection;
using Belikov.GenuineChannels.DirectExchange;
using Belikov.GenuineChannels.Logbook;
using Belikov.GenuineChannels.Parameters;
using Belikov.GenuineChannels.Receiving;
using Belikov.GenuineChannels.Security;

namespace Belikov.GenuineChannels.TransportContext;

[ComVisible(true)]
public interface ITransportContext : ISetSecuritySession
{
	IKeyStore IKeyStore { get; }

	ConnectionManager ConnectionManager { get; }

	IIncomingStreamHandler IIncomingStreamHandler { get; }

	DirectExchangeManager DirectExchangeManager { get; }

	KnownHosts KnownHosts { get; }

	string HostIdentifier { get; }

	byte[] BinaryHostIdentifier { get; }

	IParameterProvider IParameterProvider { get; }

	IGenuineEventProvider IGenuineEventProvider { get; }

	BinaryLogWriter BinaryLogWriter { get; set; }

	SecuritySessionAttributes ProhibitedSecuritySessionAttributes { get; set; }

	SecuritySessionAttributes ForcedSecuritySessionAttributes { get; set; }

	SecuritySessionParameters FixSecuritySessionParameters(SecuritySessionParameters securitySessionParameters);
}
