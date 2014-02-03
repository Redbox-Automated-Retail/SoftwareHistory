using System.Collections;
using System.Runtime.Remoting.Channels;
using Belikov.GenuineChannels.BroadcastEngine;
using Belikov.GenuineChannels.DotNetRemotingLayer;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.GenuineUdp;

internal class GenuineUdpChannel : BasicChannelWithSecurity, IBroadcastSenderProvider
{
	public override string UriPrefix => "_gudp";

	public GenuineUdpChannel(IDictionary properties, IClientChannelSinkProvider iClientChannelSinkProvider, IServerChannelSinkProvider iServerChannelSinkProvider)
		: base(iClientChannelSinkProvider, iServerChannelSinkProvider)
	{
		base.ITransportContext = TransportContextServices.CreateDefaultUdpContext(properties, this);
		InitializeInstance(properties);
		if (_channelName == null)
		{
			_channelName = "gudp";
		}
		if (_urlPrefix == null)
		{
			_urlPrefix = "gudp";
		}
		_possibleChannelPrefixes = new string[4]
		{
			base.UrlPrefix,
			UriPrefix,
			GetBroadcastUriPrefix(base.UrlPrefix),
			GetBroadcastUriPrefix(UriPrefix)
		};
		string text = this["Address"] as string;
		foreach (object property in properties)
		{
			DictionaryEntry dictionaryEntry = (DictionaryEntry)property;
			if (string.Compare(dictionaryEntry.Key.ToString(), "Address", ignoreCase: true) == 0)
			{
				text = dictionaryEntry.Value.ToString();
			}
		}
		if (text == null || text.Length <= 0 || !text.StartsWith(base.UrlPrefix + ":"))
		{
			throw GenuineExceptions.Get_Server_IncorrectAddressToListen(text);
		}
		StartListening(text);
	}

	public override string[] GetUrlsForUri(string objectUri)
	{
		return new string[1] { UriPrefix + "://" + base.ITransportContext.HostIdentifier + "/" + objectUri };
	}

	public override void StartListening(object data)
	{
		base.StartListening(data);
		base.ITransportContext.ConnectionManager.StartListening(data);
	}

	public override void StopListening(object data)
	{
		base.ITransportContext.ConnectionManager.StopListening(data);
		base.StopListening(data);
	}

	private string GetBroadcastUriPrefix(string prefix)
	{
		if (prefix.StartsWith("g"))
		{
			return "gb" + prefix.Substring(1);
		}
		return "_gb" + prefix.Substring(2);
	}

	public GeneralBroadcastSender GetBroadcastSender(string court)
	{
		return new IPMulticastSender(court, base.ITransportContext);
	}
}
