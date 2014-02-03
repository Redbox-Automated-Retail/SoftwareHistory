using System;
using System.IO;
using System.Reflection;
using Belikov.GenuineChannels.BroadcastEngine;
using Belikov.GenuineChannels.BufferPooling;
using Belikov.GenuineChannels.Messaging;
using Belikov.GenuineChannels.Parameters;
using Belikov.GenuineChannels.Security;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.Logbook;

internal class BinaryLogWriter
{
	public const int StreamHeaderMagicCode = 2028424106;

	private Stream _stream;

	private int[] levels;

	private BinaryWriter _binaryWriter;

	private static long _lifetimeId = DateTime.Now.Ticks;

	public Stream Stream => _stream;

	public int this[LogCategory logCategory]
	{
		get
		{
			return levels[(int)logCategory];
		}
		set
		{
			levels[(int)logCategory] = value;
		}
	}

	public BinaryWriter BinaryWriter => _binaryWriter;

	private object _streamLock => _stream;

	public static long LifetimeId => _lifetimeId;

	public BinaryLogWriter(Stream stream)
	{
		_stream = stream;
		_binaryWriter = new BinaryWriter(_stream);
		levels = new int[14];
		for (int i = 0; i < levels.Length; i++)
		{
			levels[i] = 2;
		}
		WriteVersionInformationEvent();
	}

	private void WriteString(string str)
	{
		if (str == null)
		{
			BinaryWriter.Write(string.Empty);
		}
		else
		{
			BinaryWriter.Write(str);
		}
	}

	private void WriteStringWithParameters(string str, params object[] parameters)
	{
		WriteString(string.Format(str, parameters));
	}

	private void WriteObjectAsString(object obj)
	{
		if (obj == null)
		{
			BinaryWriter.Write(string.Empty);
		}
		else
		{
			BinaryWriter.Write(obj.ToString());
		}
	}

	private void WriteException(Exception exception)
	{
		if (exception == null)
		{
			BinaryWriter.Write(value: false);
			return;
		}
		BinaryWriter.Write(value: true);
		WriteString(((object)exception).GetType().FullName);
		WriteString(exception.Message);
		WriteString(exception.StackTrace);
		WriteException(exception.InnerException);
	}

	private void WriteHostInformation(HostInformation remote)
	{
		if (remote == null)
		{
			BinaryWriter.Write(value: false);
			return;
		}
		BinaryWriter.Write(value: true);
		BinaryWriter.Write(remote.LocalHostUniqueIdentifier);
		BinaryWriter.Write(remote.RemoteHostUniqueIdentifier);
		WriteString(remote.Url);
		WriteString(remote.Uri);
		WriteString(remote.PrimaryUri);
		BinaryWriter.Write(GenuineUtility.GetMillisecondsLeft(remote.ExpireTime));
		WriteObjectAsString(remote.LocalPhysicalAddress);
		WriteObjectAsString(remote.PhysicalAddress);
	}

	private void WriteHostInformationId(HostInformation remote)
	{
		BinaryWriter.Write(remote != null);
		if (remote != null)
		{
			BinaryWriter.Write(remote.LocalHostUniqueIdentifier);
		}
	}

	private void WriteMessage(Message message, bool isSent, string invocationTarget, string methodName)
	{
		BinaryWriter.Write(message.SeqNo);
		BinaryWriter.Write(message.MessageId);
		BinaryWriter.Write(message.ReplyToId);
		BinaryWriter.Write(isSent);
		BinaryWriter.Write(message.IsOneWay);
		BinaryWriter.Write(message.IsSynchronous);
		BinaryWriter.Write((int)message.GenuineMessageType);
		BinaryWriter.Write(GenuineUtility.GetMillisecondsLeft(message.FinishTime));
		WriteString(invocationTarget);
		WriteString(methodName);
		if (message.Stream != null && message.Stream.CanSeek)
		{
			try
			{
				BinaryWriter.Write((int)message.Stream.Length);
				return;
			}
			catch
			{
				BinaryWriter.Write(0);
				return;
			}
		}
		BinaryWriter.Write(0);
	}

	private void WriteMessageSeqNo(Message message)
	{
		BinaryWriter.Write(message != null);
		if (message != null)
		{
			BinaryWriter.Write(message.SeqNo);
		}
	}

	private void WriteSecuritySessionParameters(SecuritySessionParameters parameters)
	{
		BinaryWriter.Write(parameters != null);
		if (parameters != null)
		{
			WriteString(parameters.Name);
			BinaryWriter.Write((int)parameters.GenuineConnectionType);
			WriteString(parameters.ConnectionName);
			BinaryWriter.Write(parameters.Timeout.Ticks);
			BinaryWriter.Write((int)parameters.Attributes);
			BinaryWriter.Write(parameters.CloseAfterInactivity.Ticks);
		}
	}

	private void WriteSecuritySessionId(SecuritySession securitySession)
	{
		BinaryWriter.Write(securitySession != null);
		if (securitySession != null)
		{
			BinaryWriter.Write(securitySession.SecuritySessionId);
		}
	}

	private void WriteBinaryContent(Stream stream)
	{
		if (stream == null)
		{
			BinaryWriter.Write(value: false);
			return;
		}
		BinaryWriter.Write(value: true);
		BufferKeeper bufferKeeper = new BufferKeeper(0);
		try
		{
			while (true)
			{
				int num = stream.Read(bufferKeeper.Buffer, 0, bufferKeeper.Buffer.Length);
				if (num <= 0)
				{
					break;
				}
				BinaryWriter.Write(num);
				BinaryWriter.Write(bufferKeeper.Buffer, 0, num);
			}
			BinaryWriter.Write(0);
			stream.Position = 0L;
		}
		finally
		{
			bufferKeeper.Dispose();
		}
	}

	public void WriteRecordHeader(BinaryRecordVersion binaryRecordVersion, LogCategory logCategory, LogMessageType type, string author)
	{
		BinaryWriter.Write(2028424106);
		BinaryWriter.Write(LifetimeId);
		BinaryWriter.Write((int)binaryRecordVersion);
		BinaryWriter.Write((int)logCategory);
		BinaryWriter.Write(DateTime.Now.Ticks);
		BinaryWriter.Write((int)type);
		WriteString(author);
	}

	public void WriteConnectionParameters(IParameterProvider iParameterProvider)
	{
		Type typeFromHandle = typeof(GenuineParameter);
		FieldInfo[] fields = typeFromHandle.GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			if (fieldInfo.FieldType == typeFromHandle)
			{
				GenuineParameter genuineParameter = (GenuineParameter)fieldInfo.GetValue(null);
				BinaryWriter.Write(value: true);
				WriteString(Enum.Format(typeFromHandle, genuineParameter, "g"));
				WriteString((iParameterProvider[genuineParameter] == null) ? "<<null>>" : iParameterProvider[genuineParameter].ToString());
			}
		}
		BinaryWriter.Write(value: false);
	}

	public void WriteDispatcherSettings(bool writeDispatcherSettings, Dispatcher dispatcher)
	{
		BinaryWriter.Write(writeDispatcherSettings);
		BinaryWriter.Write(dispatcher.DbgDispatcherId);
		WriteString(dispatcher.DbgDispatcherName);
		if (writeDispatcherSettings)
		{
			BinaryWriter.Write(value: true);
			WriteString("Interface");
			WriteString(dispatcher.SupportedInterface.ToString());
			BinaryWriter.Write(value: true);
			WriteString("ReceiveResultsTimeOut");
			WriteString(dispatcher.ReceiveResultsTimeOut.TotalMilliseconds.ToString());
			BinaryWriter.Write(value: true);
			WriteString("CallIsAsync");
			WriteString(dispatcher.CallIsAsync.ToString());
			BinaryWriter.Write(value: true);
			WriteString("MaximumNumberOfConsecutiveFailsToExcludeReceiverAutomatically");
			WriteString(dispatcher.MaximumNumberOfConsecutiveFailsToExcludeReceiverAutomatically.ToString());
			BinaryWriter.Write(value: true);
			WriteString("MaximumNumberOfConsecutiveFailsToEnableSimulationMode");
			WriteString(dispatcher.MaximumNumberOfConsecutiveFailsToEnableSimulationMode.ToString());
			BinaryWriter.Write(value: true);
			WriteString("IgnoreRecurrentCalls");
			WriteString(dispatcher.IgnoreRecurrentCalls.ToString());
			BinaryWriter.Write(value: true);
			WriteString("Local URI base");
			WriteString(GenuineUtility.DefaultHostIdentifier.ToString("N"));
			BinaryWriter.Write(value: false);
		}
	}

	public void WriteResultCollectorId(ResultCollector resultCollector)
	{
		BinaryWriter.Write(resultCollector != null);
		if (resultCollector != null)
		{
			BinaryWriter.Write(resultCollector.DbgResultCollectorId);
		}
	}

	public void WriteReceiverInfo(bool writeFullInformation, ReceiverInfo receiverInfo)
	{
		BinaryWriter.Write(receiverInfo != null);
		if (receiverInfo == null)
		{
			return;
		}
		BinaryWriter.Write(receiverInfo.DbgRecipientId);
		BinaryWriter.Write(writeFullInformation);
		if (writeFullInformation)
		{
			WriteString(receiverInfo.MbrUri);
			BinaryWriter.Write(receiverInfo.Local);
			if (receiverInfo.GeneralBroadcastSender != null)
			{
				WriteString(receiverInfo.GeneralBroadcastSender.Court);
			}
			else
			{
				BinaryWriter.Write(string.Empty);
			}
		}
	}

	public void WriteImplementationWarningEvent(string author, LogMessageType type, Exception exception, int sourceThreadId, string sourceThreadName, string description, params object[] parameters)
	{
		lock (_streamLock)
		{
			WriteRecordHeader(BinaryRecordVersion.ImplementationWarning, LogCategory.ImplementationWarning, type, author);
			WriteException(exception);
			BinaryWriter.Write(sourceThreadId);
			WriteString(sourceThreadName);
			WriteStringWithParameters(description, parameters);
			BinaryWriter.Flush();
		}
	}

	public void WriteEvent(LogCategory logCategory, string author, LogMessageType type, Exception exception, Message message, HostInformation remote, Stream content, int sourceThreadId, string sourceThreadName, SecuritySession securitySession, string securitySessionName, int connectionId, int number1, int number2, int number3, string string1, string string2, string string3, string string4, string description, params object[] parameters)
	{
		lock (_streamLock)
		{
			WriteRecordHeader(BinaryRecordVersion.GeneralRecord, logCategory, type, author);
			WriteException(exception);
			WriteMessageSeqNo(message);
			WriteHostInformationId(remote);
			WriteBinaryContent(content);
			BinaryWriter.Write(sourceThreadId);
			WriteString(sourceThreadName);
			WriteSecuritySessionId(securitySession);
			WriteString(securitySessionName);
			BinaryWriter.Write(connectionId);
			BinaryWriter.Write(number1);
			BinaryWriter.Write(number2);
			BinaryWriter.Write(number3);
			WriteString(string1);
			WriteString(string2);
			WriteString(string3);
			WriteString(string4);
			WriteStringWithParameters(description, parameters);
			BinaryWriter.Flush();
		}
	}

	public void WriteMessageCreatedEvent(string author, LogMessageType type, Exception exception, Message message, bool isSent, HostInformation remote, Stream content, string invocationTarget, string methodName, int sourceThreadId, string sourceThreadName, string transportName, int connectionId, int clssId, string clssName, int ilssId, string ilssName, string description, params object[] parameters)
	{
		if (message == null)
		{
			WriteImplementationWarningEvent("BinaryLogWriter.WriteMessageCreatedEvent", LogMessageType.Error, null, sourceThreadId, sourceThreadName, "The message is not provided. Stack trace: " + Environment.StackTrace);
			return;
		}
		lock (_streamLock)
		{
			WriteRecordHeader(BinaryRecordVersion.MessageRecord, LogCategory.MessageProcessing, type, author);
			WriteException(exception);
			WriteMessage(message, isSent, invocationTarget, methodName);
			WriteHostInformationId(remote);
			WriteBinaryContent(content);
			BinaryWriter.Write(sourceThreadId);
			WriteString(sourceThreadName);
			WriteString(transportName);
			BinaryWriter.Write(connectionId);
			BinaryWriter.Write(clssId);
			WriteString(clssName);
			BinaryWriter.Write(ilssId);
			WriteString(ilssName);
			WriteStringWithParameters(description, parameters);
			BinaryWriter.Flush();
		}
	}

	public void WriteSecuritySessionParametersEvent(string author, LogMessageType type, Exception exception, Message message, HostInformation remote, int sourceThreadId, string sourceThreadName, SecuritySessionParameters securitySessionParameters, string description, params object[] parameters)
	{
		if (message == null || securitySessionParameters == null)
		{
			WriteImplementationWarningEvent("BinaryLogWriter.WriteMessageCreatedEvent", LogMessageType.Error, null, sourceThreadId, sourceThreadName, "The message or SSP is not provided. Stack trace: " + Environment.StackTrace);
			return;
		}
		lock (_streamLock)
		{
			WriteRecordHeader(BinaryRecordVersion.SecuritySessionParameters, LogCategory.Security, type, author);
			WriteException(exception);
			WriteMessageSeqNo(message);
			WriteHostInformationId(remote);
			BinaryWriter.Write(sourceThreadId);
			WriteString(sourceThreadName);
			WriteSecuritySessionParameters(securitySessionParameters);
			WriteStringWithParameters(description, parameters);
			BinaryWriter.Flush();
		}
	}

	public void WriteHostInformationEvent(string author, LogMessageType type, Exception exception, HostInformation remote, int sourceThreadId, string sourceThreadName, SecuritySession securitySession, string securitySessionName, int connectionId, string description, params object[] parameters)
	{
		if (remote == null)
		{
			WriteImplementationWarningEvent("BinaryLogWriter.WriteHostInformationCreatedEvent", LogMessageType.Error, null, sourceThreadId, sourceThreadName, "The reference is null. Stack trace: " + Environment.StackTrace);
			return;
		}
		lock (_streamLock)
		{
			WriteRecordHeader(BinaryRecordVersion.HostInformationInfo, LogCategory.HostInformation, type, author);
			WriteException(exception);
			WriteHostInformation(remote);
			BinaryWriter.Write(sourceThreadId);
			WriteString(sourceThreadName);
			WriteSecuritySessionId(securitySession);
			WriteString(securitySessionName);
			BinaryWriter.Write(connectionId);
			WriteStringWithParameters(description, parameters);
			BinaryWriter.Flush();
		}
	}

	public void WriteConnectionParameterEvent(LogCategory logCategory, string author, LogMessageType type, Exception exception, HostInformation remote, IParameterProvider iParameterProvider, int sourceThreadId, string sourceThreadName, int connectionId, string description, params object[] parameters)
	{
		lock (_streamLock)
		{
			WriteRecordHeader(BinaryRecordVersion.ConnectionParametersRecord, logCategory, type, author);
			WriteException(exception);
			WriteHostInformationId(remote);
			BinaryWriter.Write(connectionId);
			WriteConnectionParameters(iParameterProvider);
			BinaryWriter.Write(sourceThreadId);
			WriteString(sourceThreadName);
			WriteStringWithParameters(description, parameters);
			BinaryWriter.Flush();
		}
	}

	public void WriteTransportContentEvent(LogCategory logCategory, string author, LogMessageType type, Exception exception, Message message, HostInformation remote, Stream content, int sourceThreadId, string sourceThreadName, int connectionId, int contentSize, string remoteHostAddress, string string1, string string2, string description, params object[] parameters)
	{
		lock (_streamLock)
		{
			WriteRecordHeader(BinaryRecordVersion.TransportContentRecord, logCategory, type, author);
			WriteException(exception);
			WriteMessageSeqNo(message);
			WriteHostInformationId(remote);
			WriteBinaryContent(content);
			BinaryWriter.Write(sourceThreadId);
			WriteString(sourceThreadName);
			BinaryWriter.Write(connectionId);
			BinaryWriter.Write(contentSize);
			WriteString(remoteHostAddress);
			WriteString(string1);
			WriteString(string2);
			WriteStringWithParameters(description, parameters);
			BinaryWriter.Flush();
		}
	}

	public void WriteBroadcastEngineEvent(LogCategory logCategory, string author, LogMessageType type, Exception exception, Message message, HostInformation remote, Stream content, int sourceThreadId, string sourceThreadName, SecuritySession securitySession, string securitySessionName, bool writeDispatcherSettings, Dispatcher dispatcher, ResultCollector resultCollector, bool writeReceiverInfoSettings, ReceiverInfo receiverInfo, string string1, string string2, string description, params object[] parameters)
	{
		if (dispatcher == null)
		{
			WriteImplementationWarningEvent("BinaryLogWriter.WriteBroadcastEngineEvent", LogMessageType.Error, null, sourceThreadId, sourceThreadName, "The reference is null. Stack trace: " + Environment.StackTrace);
			return;
		}
		lock (_streamLock)
		{
			WriteRecordHeader(BinaryRecordVersion.TransportBroadcastEngineRecord, logCategory, type, author);
			WriteException(exception);
			WriteMessageSeqNo(message);
			WriteHostInformationId(remote);
			WriteBinaryContent(content);
			BinaryWriter.Write(sourceThreadId);
			WriteString(sourceThreadName);
			WriteSecuritySessionId(securitySession);
			WriteString(securitySessionName);
			WriteResultCollectorId(resultCollector);
			WriteDispatcherSettings(writeDispatcherSettings, dispatcher);
			WriteReceiverInfo(writeReceiverInfoSettings, receiverInfo);
			WriteString(string1);
			WriteString(string2);
			WriteStringWithParameters(description, parameters);
			BinaryWriter.Flush();
		}
	}

	protected void WriteVersionInformationEvent()
	{
		lock (_streamLock)
		{
			WriteRecordHeader(BinaryRecordVersion.VersionRecord, LogCategory.Version, LogMessageType.Warning, "BinaryLogWriter.WriteVersionInformationEvent");
			BinaryWriter.Write(value: true);
			WriteString("Genuine Channels Version");
			WriteString(GetType().AssemblyQualifiedName);
			BinaryWriter.Write(value: true);
			WriteString("Is Genuine Channels in GAC");
			WriteString(GetType().Assembly.GlobalAssemblyCache.ToString());
			BinaryWriter.Write(value: true);
			WriteString("Location of Genuine Channels");
			WriteString(GetType().Assembly.Location);
			BinaryWriter.Write(value: true);
			WriteString("OS version");
			WriteString(Environment.OSVersion.ToString());
			BinaryWriter.Write(value: true);
			WriteString("CLR version");
			WriteString(Environment.Version.ToString());
			BinaryWriter.Write(value: false);
			BinaryWriter.Flush();
		}
	}

	public static string ParseInvocationMethod(string methodName, string typeName)
	{
		if (methodName == null)
		{
			methodName = string.Empty;
		}
		if (typeName == null)
		{
			typeName = string.Empty;
		}
		int num = typeName.IndexOf(',');
		if (num > 0)
		{
			return typeName.Substring(0, num) + "." + methodName;
		}
		return typeName + "; " + methodName;
	}
}
