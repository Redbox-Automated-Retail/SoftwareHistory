using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using Belikov.GenuineChannels.DotNetRemotingLayer;
using Belikov.GenuineChannels.Parameters;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.GenuineSharedMemory;

[ComVisible(true)]
public class GenuineSharedMemoryChannel : BasicChannelWithSecurity
{
	private string _uriToListen;

	public override string UriPrefix => "_gshmem";

	public GenuineSharedMemoryChannel(IDictionary properties, IClientChannelSinkProvider iClientChannelSinkProvider, IServerChannelSinkProvider iServerChannelSinkProvider)
		: base(iClientChannelSinkProvider, iServerChannelSinkProvider)
	{
		base.ITransportContext = TransportContextServices.CreateDefaultSharedMemoryContext(properties, this);
		InitializeInstance(properties);
		if (_channelName == null)
		{
			_channelName = "gshmem";
		}
		if (_urlPrefix == null)
		{
			_urlPrefix = "gshmem";
		}
		_possibleChannelPrefixes = new string[2] { base.UrlPrefix, UriPrefix };
		foreach (object property in properties)
		{
			DictionaryEntry dictionaryEntry = (DictionaryEntry)property;
			if (string.Compare(dictionaryEntry.Key.ToString(), "listen", ignoreCase: true) == 0)
			{
				_uriToListen = dictionaryEntry.Value.ToString();
			}
		}
		if (_uriToListen != null && (_uriToListen.Length <= 0 || !_uriToListen.StartsWith("gshmem:")))
		{
			throw GenuineExceptions.Get_Server_IncorrectAddressToListen(_uriToListen);
		}
		if (_uriToListen != null)
		{
			StartListening(null);
		}
	}

	public static string ConstructSharedObjectName(string name, IParameterProvider parameters)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (parameters == null)
		{
			throw new ArgumentNullException("parameters");
		}
		return (((bool)parameters[GenuineParameter.SMSessionLocal]) ? "Local\\" : "Global\\") + name;
	}

	public override string[] GetUrlsForUri(string objectUri)
	{
		return new string[1] { UriPrefix + "://" + base.ITransportContext.HostIdentifier + "/" + objectUri };
	}

	public override void StartListening(object data)
	{
		base.StartListening(data);
		base.ITransportContext.ConnectionManager.StartListening(_uriToListen);
	}

	public override void StopListening(object data)
	{
		base.ITransportContext.ConnectionManager.StopListening(_uriToListen);
		base.StopListening(data);
	}
}
