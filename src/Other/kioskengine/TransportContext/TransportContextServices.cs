using System.Collections;
using System.Runtime.InteropServices;
using Belikov.GenuineChannels.DirectExchange;
using Belikov.GenuineChannels.GenuineHttp;
using Belikov.GenuineChannels.GenuineSharedMemory;
using Belikov.GenuineChannels.GenuineTcp;
using Belikov.GenuineChannels.GenuineUdp;
using Belikov.GenuineChannels.GenuineXHttp;
using Belikov.GenuineChannels.Parameters;
using Belikov.GenuineChannels.Receiving;
using Belikov.GenuineChannels.Security;

namespace Belikov.GenuineChannels.TransportContext;

[ComVisible(true)]
public class TransportContextServices
{
	public static ITransportContext CreateDefaultTcpContext(IDictionary properties, ITransportUser defaultTransportUser)
	{
		TransportContext transportContext = new TransportContext();
		transportContext._iKeyStore = new KeyStore();
		transportContext._knownHosts = new KnownHosts(transportContext);
		transportContext._iParameterProvider = new ReadingCascadeParameterProvider(properties, new DefaultParameterProvider());
		transportContext._iGenuineEventProvider = new GenuineEventProvider(transportContext);
		transportContext._directExchangeManager = new DirectExchangeManager(transportContext);
		transportContext._connectionManager = new TcpConnectionManager(transportContext);
		transportContext._iIncomingStreamHandler = new GenuineReceivingHandler(transportContext, defaultTransportUser);
		return transportContext;
	}

	public static ITransportContext CreateDefaultSharedMemoryContext(IDictionary properties, ITransportUser defaultTransportUser)
	{
		TransportContext transportContext = new TransportContext();
		transportContext._iKeyStore = new KeyStore();
		transportContext._knownHosts = new KnownHosts(transportContext);
		transportContext._iParameterProvider = new ReadingCascadeParameterProvider(properties, new DefaultParameterProvider());
		transportContext._iGenuineEventProvider = new GenuineEventProvider(transportContext);
		transportContext._directExchangeManager = new DirectExchangeManager(transportContext);
		transportContext._connectionManager = new SharedMemoryConnectionManager(transportContext);
		transportContext._iIncomingStreamHandler = new GenuineReceivingHandler(transportContext, defaultTransportUser);
		return transportContext;
	}

	public static ITransportContext CreateDefaultUdpContext(IDictionary properties, ITransportUser defaultTransportUser)
	{
		TransportContext transportContext = new TransportContext();
		transportContext._iKeyStore = new KeyStore();
		transportContext._knownHosts = new KnownHosts(transportContext);
		transportContext._iParameterProvider = new ReadingCascadeParameterProvider(properties, new DefaultParameterProvider());
		transportContext._iGenuineEventProvider = new GenuineEventProvider(transportContext);
		transportContext._directExchangeManager = new DirectExchangeManager(transportContext);
		transportContext._connectionManager = new UdpConnectionManager(transportContext);
		transportContext._iIncomingStreamHandler = new GenuineReceivingHandler(transportContext, defaultTransportUser);
		return transportContext;
	}

	public static ITransportContext CreateDefaultClientHttpContext(IDictionary properties, ITransportUser defaultTransportUser)
	{
		TransportContext transportContext = new TransportContext();
		transportContext._iKeyStore = new KeyStore();
		transportContext._knownHosts = new KnownHosts(transportContext);
		transportContext._iParameterProvider = new ReadingCascadeParameterProvider(properties, new DefaultParameterProvider());
		transportContext._iGenuineEventProvider = new GenuineEventProvider(transportContext);
		transportContext._directExchangeManager = new DirectExchangeManager(transportContext);
		transportContext._connectionManager = new HttpClientConnectionManager(transportContext);
		transportContext._iIncomingStreamHandler = new GenuineReceivingHandler(transportContext, defaultTransportUser);
		return transportContext;
	}

	public static ITransportContext CreateDefaultServerHttpContext(IDictionary properties, ITransportUser defaultTransportUser)
	{
		TransportContext transportContext = new TransportContext();
		transportContext._iKeyStore = new KeyStore();
		transportContext._knownHosts = new KnownHosts(transportContext);
		transportContext._iParameterProvider = new ReadingCascadeParameterProvider(properties, new DefaultParameterProvider());
		transportContext._iGenuineEventProvider = new GenuineEventProvider(transportContext);
		transportContext._directExchangeManager = new DirectExchangeManager(transportContext);
		transportContext._connectionManager = new HttpServerConnectionManager(transportContext);
		transportContext._iIncomingStreamHandler = new GenuineReceivingHandler(transportContext, defaultTransportUser);
		return transportContext;
	}

	public static ITransportContext CreateDefaultXHttpContext(IDictionary properties, ITransportUser defaultTransportUser)
	{
		TransportContext transportContext = new TransportContext();
		transportContext._iKeyStore = new KeyStore();
		transportContext._knownHosts = new KnownHosts(transportContext);
		transportContext._iParameterProvider = new ReadingCascadeParameterProvider(properties, new DefaultParameterProvider());
		transportContext._iGenuineEventProvider = new GenuineEventProvider(transportContext);
		transportContext._directExchangeManager = new DirectExchangeManager(transportContext);
		transportContext._connectionManager = new XHttpConnectionManager(transportContext);
		transportContext._iIncomingStreamHandler = new GenuineReceivingHandler(transportContext, defaultTransportUser);
		return transportContext;
	}
}
