using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using Belikov.GenuineChannels.Logbook;
using Belikov.GenuineChannels.Messaging;
using Belikov.GenuineChannels.Parameters;
using Belikov.GenuineChannels.Receiving;
using Belikov.GenuineChannels.Security;
using Belikov.GenuineChannels.TransportContext;
using Belikov.GenuineChannels.Utilities;

namespace Belikov.GenuineChannels.DotNetRemotingLayer;

[ComVisible(true)]
public abstract class BasicChannelWithSecurity : BaseChannelWithProperties, ISetSecuritySession, IChannelReceiver, IChannelSender, IChannel, ITransportUser, IGenuineChannelEventProvider, IGenuineEventHandler, ITransportContextProvider
{
	private ITransportContext _iTransportContext;

	private IServerChannelSink _iServerChannelSink;

	protected IClientChannelSinkProvider _iClientChannelSinkProvider;

	private IServerChannelSinkProvider _iServerChannelSinkProvider;

	protected IDictionary _properties;

	protected string _channelName;

	protected int _channelPriority;

	protected ChannelDataStore _channelData;

	protected string _urlPrefix;

	protected string[] _possibleChannelPrefixes;

	private object _genuineChannelsEventLock = new object();

	public ITransportContext ITransportContext
	{
		get
		{
			return _iTransportContext;
		}
		set
		{
			if (_iTransportContext != null)
			{
				_iTransportContext.IGenuineEventProvider.Unregister(this);
			}
			_iTransportContext = value;
			_iTransportContext.IGenuineEventProvider.Register(this);
		}
	}

	public IServerChannelSink IServerChannelSink => _iServerChannelSink;

	public override ICollection Keys => _properties.Keys;

	public override object this[object key]
	{
		get
		{
			return _properties[key];
		}
		set
		{
			_properties[key] = value;
		}
	}

	public string ChannelName => _channelName;

	public int ChannelPriority => _channelPriority;

	public object ChannelData
	{
		get
		{
			if (_channelData == null)
			{
				_channelData = new ChannelDataStore(GetUrlsForUri(string.Empty));
			}
			return _channelData;
		}
	}

	public SecuritySessionParameters SecuritySessionParameters
	{
		get
		{
			return ITransportContext.SecuritySessionParameters;
		}
		set
		{
			ITransportContext.SecuritySessionParameters = value;
		}
	}

	public string UrlPrefix => _urlPrefix;

	public abstract string UriPrefix { get; }

	public event GenuineChannelsGlobalEventHandler GenuineChannelsEvent
	{
		add
		{
			lock (_genuineChannelsEventLock)
			{
				_genuineChannelsEvent = (GenuineChannelsGlobalEventHandler)Delegate.Combine(_genuineChannelsEvent, value);
			}
		}
		remove
		{
			lock (_genuineChannelsEventLock)
			{
				_genuineChannelsEvent = (GenuineChannelsGlobalEventHandler)Delegate.Combine(_genuineChannelsEvent, value);
			}
		}
	}

	private event GenuineChannelsGlobalEventHandler _genuineChannelsEvent;

	public BasicChannelWithSecurity(IClientChannelSinkProvider iClientChannelSinkProvider, IServerChannelSinkProvider iServerChannelSinkProvider)
	{
		_iClientChannelSinkProvider = iClientChannelSinkProvider;
		_iServerChannelSinkProvider = iServerChannelSinkProvider;
	}

	protected void InitializeInstance(IDictionary properties)
	{
		if (properties == null)
		{
			_properties = new Hashtable();
		}
		else
		{
			_properties = properties;
		}
		ReadSettings(properties);
		if (_iClientChannelSinkProvider == null)
		{
			_iClientChannelSinkProvider = new BinaryClientFormatterSinkProvider();
		}
		IClientChannelSinkProvider clientChannelSinkProvider = _iClientChannelSinkProvider;
		while (clientChannelSinkProvider.Next != null)
		{
			clientChannelSinkProvider = clientChannelSinkProvider.Next;
		}
		clientChannelSinkProvider.Next = new GenuineTcpClientTransportSinkProvider(ITransportContext);
		if (_iServerChannelSinkProvider == null)
		{
			_iServerChannelSinkProvider = GenuineUtility.GetDefaultServerSinkChain();
		}
		for (IServerChannelSinkProvider serverChannelSinkProvider = _iServerChannelSinkProvider; serverChannelSinkProvider != null; serverChannelSinkProvider = serverChannelSinkProvider.Next)
		{
			serverChannelSinkProvider.GetChannelData((IChannelDataStore)ChannelData);
		}
		_iServerChannelSink = new GenuineUniversalServerTransportSink(this, ChannelServices.CreateServerChannelSinkChain(_iServerChannelSinkProvider, this), ITransportContext);
	}

	protected virtual IClientChannelSinkProvider CreateClientChannelSinkProvider()
	{
		return new GenuineTcpClientTransportSinkProvider(ITransportContext);
	}

	public string Parse(string url, out string objectURI)
	{
		objectURI = null;
		return null;
	}

	public abstract string[] GetUrlsForUri(string objectUri);

	public virtual void StartListening(object data)
	{
		ITransportContext.IGenuineEventProvider.Register(this);
		ReadSettings(_properties);
	}

	public virtual void StopListening(object data)
	{
		ITransportContext.IGenuineEventProvider.Unregister(this);
	}

	public IMessageSink CreateMessageSink(string url, object remoteChannelData, out string objectURI)
	{
		objectURI = null;
		string text = null;
		if (url == null && remoteChannelData is IChannelDataStore)
		{
			url = ((IChannelDataStore)remoteChannelData).ChannelUris[0];
		}
		if (url == null)
		{
			return null;
		}
		string[] possibleChannelPrefixes = _possibleChannelPrefixes;
		int i;
		for (i = 0; i < possibleChannelPrefixes.Length && !url.StartsWith(possibleChannelPrefixes[i]); i++)
		{
		}
		if (i == possibleChannelPrefixes.Length)
		{
			return null;
		}
		text = GenuineUtility.Parse(url, out objectURI);
		if (!GenuineUtility.CheckUrlOnConnectivity(url) && UriStorage.GetTransportContext(text) == null)
		{
			return null;
		}
		if (text == null)
		{
			return null;
		}
		return _iClientChannelSinkProvider.CreateSink(this, url, remoteChannelData) as IMessageSink;
	}

	public void FireGenuineEvent(GenuineEventArgs e)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.ChannelEvent] > 0)
		{
			binaryLogWriter.WriteEvent(LogCategory.ChannelEvent, "BasicChannelWithSecurity.FireGenuineEvent", LogMessageType.ChannelEvent, e.SourceException, null, e.HostInformation, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, (int)e.EventType, 0, 0, null, null, null, null, "The channel event is raised.");
		}
		if (e.EventType == GenuineEventType.GTcpConnectionAccepted || e.EventType == GenuineEventType.GHttpConnectionAccepted)
		{
			PerformEventSending(e);
		}
		else
		{
			GenuineThreadPool.QueueUserWorkItem(PerformEventSending, e, longDuration: false);
		}
	}

	public void PerformEventSending(object genuineEventArgsAsObject)
	{
		GenuineEventArgs e = (GenuineEventArgs)genuineEventArgsAsObject;
		GenuineChannelsGlobalEventHandler genuineChannelsGlobalEventHandler = null;
		lock (_genuineChannelsEventLock)
		{
			if (_genuineChannelsEvent == null)
			{
				return;
			}
			genuineChannelsGlobalEventHandler = (GenuineChannelsGlobalEventHandler)_genuineChannelsEvent.Clone();
		}
		genuineChannelsGlobalEventHandler(this, e);
	}

	public void OnGenuineEvent(GenuineEventArgs genuineEventArgs)
	{
		FireGenuineEvent(genuineEventArgs);
	}

	protected virtual void ReadSettings(IDictionary properties)
	{
		foreach (object property in properties)
		{
			DictionaryEntry dictionaryEntry = (DictionaryEntry)property;
			if (string.Compare(dictionaryEntry.Key.ToString(), "name", ignoreCase: true) == 0)
			{
				_channelName = dictionaryEntry.Value.ToString();
			}
			if (string.Compare(dictionaryEntry.Key.ToString(), "priority", ignoreCase: true) == 0)
			{
				_channelPriority = GenuineUtility.SafeConvertToInt32(dictionaryEntry.Value, _channelPriority);
			}
			if (string.Compare(dictionaryEntry.Key.ToString(), "prefix", ignoreCase: true) == 0)
			{
				_urlPrefix = dictionaryEntry.Value.ToString();
				if (!_urlPrefix.StartsWith("g") || _urlPrefix.Length < 4 || _urlPrefix.Length > 8)
				{
					GenuineExceptions.Get_Channel_InvalidParameter("prefix");
				}
			}
		}
		if ((bool)ITransportContext.IParameterProvider[GenuineParameter.Compression])
		{
			SecuritySessionParameters = SecuritySessionServices.DefaultContextWithCompression;
		}
	}

	public void HandleMessage(Message message)
	{
		((GenuineUniversalServerTransportSink)_iServerChannelSink).HandleIncomingMessage(message);
	}
}
