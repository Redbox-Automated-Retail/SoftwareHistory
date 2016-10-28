using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using Belikov.GenuineChannels.GenuineHttp;
using Belikov.GenuineChannels.Parameters;
using Belikov.GenuineChannels.Security;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.Messaging;

[ComVisible(true)]
public class Message
{
	public const string TransportHeadersSenderEntryName = "_gens";

	public const string TransportHeadersMbrUriName = "_genu";

	public const string TransportHeadersBroadcastSendGuid = "_genbg";

	public const string TransportHeadersBroadcastObjRefOrCourt = "__genbdc";

	public const string TransportHeadersSerializedException = "__gensrlex";

	public const string TransportHeadersGenuineMessageType = "__genmt";

	public const string TransportHeadersInvocationTarget = "__genit";

	public const string TransportHeadersMethodName = "__genmn";

	private object _accessToLocalMembers = new object();

	public static string TransportHeaderName_RemoteObjectUri = "__RequestUri";

	public GenuineMessageType GenuineMessageType = GenuineMessageType.Ordinary;

	public int MessageId;

	private static int _currentMessageId = 1;

	public bool IsOneWay = false;

	public int ReplyToId;

	public ITransportHeaders ITransportHeaders;

	public Stream Stream;

	public int EffectiveMessageSize = 0;

	public Stream SerializedContent;

	public SecuritySession ConnectionLevelSecuritySession;

	public SecuritySessionParameters SecuritySessionParameters;

	public object DestinationMarshalByRef;

	public object Tag;

	public object ConnectionInfo;

	public IMessage IMessage;

	public HttpServerRequestResult HttpServerRequestResult;

	public bool HasBeenAsnwered = false;

	public ITransportContext ITransportContext;

	public HostInformation Sender;

	public HostInformation Recipient;

	private int _finishTime = GenuineUtility.FurthestFuture;

	private bool _finishTime_Initialized = false;

	public string ConnectionName;

	public bool IsSynchronous;

	public bool IsResendAfterFail = false;

	internal ManualResetEvent ConnectionAvailable;

	internal ManualResetEvent CancelSending;

	internal Exception SyncWaitException;

	internal bool IsValid = true;

	private int _seqNo = Interlocked.Increment(ref _globalSeqNo);

	private static int _globalSeqNo = 0;

	public bool ContainsSerializedException
	{
		get
		{
			return ITransportHeaders["__gensrlex"] != null;
		}
		set
		{
			if (value)
			{
				ITransportHeaders["__gensrlex"] = "";
			}
			else
			{
				ITransportHeaders["__gensrlex"] = null;
			}
		}
	}

	public int FinishTime
	{
		get
		{
			lock (_accessToLocalMembers)
			{
				return _finishTime;
			}
		}
		set
		{
			lock (_accessToLocalMembers)
			{
				_finishTime = value;
				FinishTime_Initialized = true;
			}
		}
	}

	public bool FinishTime_Initialized
	{
		get
		{
			lock (_accessToLocalMembers)
			{
				return _finishTime_Initialized;
			}
		}
		set
		{
			lock (_accessToLocalMembers)
			{
				_finishTime_Initialized = value;
			}
		}
	}

	public int SeqNo => _seqNo;

	public Message()
	{
	}

	public Message(ITransportContext iTransportContext, HostInformation recipient, int replyToId, ITransportHeaders iTransportHeaders, Stream stream)
	{
		ITransportContext = iTransportContext;
		ITransportHeaders = iTransportHeaders;
		Recipient = recipient;
		ReplyToId = replyToId;
		Stream = stream;
		if (ITransportHeaders != null)
		{
			ITransportHeaders["_gens"] = Sender;
		}
		MessageId = Interlocked.Increment(ref _currentMessageId);
	}

	public Message(Message sourceMessage, ITransportHeaders iTransportHeaders, Stream stream)
	{
		ITransportContext = sourceMessage.ITransportContext;
		ITransportHeaders = iTransportHeaders;
		Recipient = sourceMessage.Sender;
		ReplyToId = sourceMessage.MessageId;
		GenuineMessageType = sourceMessage.GenuineMessageType;
		Stream = stream;
		ConnectionName = sourceMessage.ConnectionName;
		SecuritySessionParameters = ITransportContext.FixSecuritySessionParameters(sourceMessage.SecuritySessionParameters);
		IsSynchronous = (SecuritySessionParameters.Attributes & SecuritySessionAttributes.ForceSync) != SecuritySessionAttributes.None || ((bool)ITransportContext.IParameterProvider[GenuineParameter.SyncResponses] && (SecuritySessionParameters.Attributes & SecuritySessionAttributes.ForceAsync) == 0);
		if (ITransportHeaders != null)
		{
			ITransportHeaders["_gens"] = Sender;
		}
		MessageId = Interlocked.Increment(ref _currentMessageId);
	}

	public static Message CreateOutcomingMessage(ITransportContext iTransportContext, IMessage iMessage, ITransportHeaders iTransportHeaders, Stream stream, bool isSynchronous)
	{
		string text = (string)iMessage.Properties["__Uri"];
		string text2 = GenuineUtility.Parse(text, out var objectURI);
		if (objectURI == null)
		{
			objectURI = text;
		}
		iTransportHeaders[TransportHeaderName_RemoteObjectUri] = objectURI;
		Message message = new Message(iTransportContext, null, 0, iTransportHeaders, stream);
		message.IMessage = iMessage;
		message.IsSynchronous = isSynchronous;
		if (iTransportHeaders["__genmt"] != null)
		{
			message.GenuineMessageType = (GenuineMessageType)iTransportHeaders["__genmt"];
		}
		return message;
	}

	public void Dispose()
	{
		if (SerializedContent != null)
		{
			SerializedContent.Close();
		}
		if (Stream != null)
		{
			Stream.Close();
		}
	}

	public override string ToString()
	{
		return string.Format("MESSAGE (Id: {0} Reply: {1} Sender: {2} Recipient: {3} Security Session Name: {4} Security Session Parameters: {5} One-way: {6} Synchronous: {7} Timeout: {8})", MessageId, ReplyToId, (Sender == null) ? "<none>" : Sender.ToString(), (Recipient == null) ? "<none>" : Recipient.ToString(), (SecuritySessionParameters == null) ? "<none>" : SecuritySessionParameters.Name, (SecuritySessionParameters == null) ? "<none>" : Enum.Format(typeof(SecuritySessionAttributes), SecuritySessionParameters.Attributes, "G"), IsOneWay, IsSynchronous.ToString(), FinishTime);
	}
}
