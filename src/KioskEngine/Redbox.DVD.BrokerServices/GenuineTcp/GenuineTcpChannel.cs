using System.Collections;
using System.Runtime.Remoting.Channels;
using Belikov.GenuineChannels.DotNetRemotingLayer;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.GenuineTcp;

internal class GenuineTcpChannel : BasicChannelWithSecurity
{
	private int _localPort = -1;

	private string _localInterface = "0.0.0.0";

	public int LocalPort
	{
		get
		{
			TcpConnectionManager tcpConnectionManager = (TcpConnectionManager)base.ITransportContext.ConnectionManager;
			return tcpConnectionManager.LocalPort;
		}
	}

	public override string UriPrefix => "_gtcp";

	public string ListeningEntry => base.UrlPrefix + "://" + _localInterface + ":" + _localPort;

	public GenuineTcpChannel(IDictionary properties, IClientChannelSinkProvider iClientChannelSinkProvider, IServerChannelSinkProvider iServerChannelSinkProvider)
		: base(iClientChannelSinkProvider, iServerChannelSinkProvider)
	{
		base.ITransportContext = TransportContextServices.CreateDefaultTcpContext(properties, this);
		InitializeInstance(properties);
		if (_channelName == null)
		{
			_channelName = "gtcp";
		}
		if (_urlPrefix == null)
		{
			_urlPrefix = "gtcp";
		}
		_possibleChannelPrefixes = new string[2] { base.UrlPrefix, UriPrefix };
		StartListening(null);
	}

	protected override IClientChannelSinkProvider CreateClientChannelSinkProvider()
	{
		return new GenuineTcpClientTransportSinkProvider(base.ITransportContext);
	}

	public override string[] GetUrlsForUri(string objectUri)
	{
		return new string[1] { UriPrefix + "://" + base.ITransportContext.HostIdentifier + "/" + objectUri };
	}

	public override void StartListening(object data)
	{
		base.StartListening(data);
		if (_localPort >= 0)
		{
			base.ITransportContext.ConnectionManager.StartListening(ListeningEntry);
		}
	}

	public override void StopListening(object data)
	{
		if (_localPort >= 0)
		{
			base.ITransportContext.ConnectionManager.StopListening(ListeningEntry);
			base.StopListening(data);
		}
	}

	protected override void ReadSettings(IDictionary properties)
	{
		base.ReadSettings(properties);
		foreach (object property in properties)
		{
			DictionaryEntry dictionaryEntry = (DictionaryEntry)property;
			if (string.Compare(dictionaryEntry.Key.ToString(), "interface", ignoreCase: true) == 0)
			{
				_localInterface = dictionaryEntry.Value.ToString();
			}
			if (string.Compare(dictionaryEntry.Key.ToString(), "port", ignoreCase: true) == 0)
			{
				_localPort = GenuineUtility.SafeConvertToInt32(dictionaryEntry.Value, _localPort);
			}
		}
	}
}
