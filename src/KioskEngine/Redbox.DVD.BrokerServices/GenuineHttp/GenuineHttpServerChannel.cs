using System.Collections;
using System.Runtime.Remoting.Channels;
using Belikov.GenuineChannels.DotNetRemotingLayer;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.GenuineHttp;

internal class GenuineHttpServerChannel : BasicChannelWithSecurity
{
	public override string UriPrefix => "_ghttp";

	public GenuineHttpServerChannel(IDictionary properties, IClientChannelSinkProvider iClientChannelSinkProvider, IServerChannelSinkProvider iServerChannelSinkProvider)
		: base(iClientChannelSinkProvider, iServerChannelSinkProvider)
	{
		base.ITransportContext = TransportContextServices.CreateDefaultServerHttpContext(properties, this);
		InitializeInstance(properties);
		if (_channelName == null)
		{
			_channelName = "ghttp";
		}
		if (_urlPrefix == null)
		{
			_urlPrefix = "ghttp";
		}
		_possibleChannelPrefixes = new string[2] { base.UrlPrefix, UriPrefix };
	}

	public override string[] GetUrlsForUri(string objectUri)
	{
		return new string[1] { UriPrefix + "://" + base.ITransportContext.HostIdentifier + "/" + objectUri };
	}
}
