using System;
using System.Collections;
using System.IO;
using System.Runtime.Remoting.Channels;
using System.Runtime.Serialization.Formatters.Binary;
using Belikov.GenuineChannels.Connection;
using Belikov.GenuineChannels.Security;
using ICSharpCode.SharpZipLib.GZip;

namespace Belikov.GenuineChannels.Messaging;

internal class MessageCoder
{
	public const byte COMMAND_MAGIC_CODE = 55;

	public const byte PROTOCOL_VERSION = 3;

	public const int LABEL_HEADER_SIZE = 6;

	private MessageCoder()
	{
	}

	public static void Serialize(GenuineChunkedStream stream, Message message, bool compress)
	{
		BinaryWriter binaryWriter = new BinaryWriter(stream);
		binaryWriter.Write(compress);
		GenuineChunkedStream genuineChunkedStream = stream;
		if (compress)
		{
			genuineChunkedStream = new GenuineChunkedStream(releaseOnReadMode: false);
		}
		binaryWriter = new BinaryWriter(genuineChunkedStream);
		binaryWriter.Write((byte)message.GenuineMessageType);
		binaryWriter.Write(message.MessageId);
		binaryWriter.Write(message.ReplyToId);
		binaryWriter.Write(message.IsOneWay);
		object obj = message.ITransportHeaders["__genbdc"];
		if (obj != null)
		{
			message.DestinationMarshalByRef = obj;
		}
		if (message.DestinationMarshalByRef != null)
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			binaryWriter.Write(value: true);
			binaryFormatter.Serialize(binaryWriter.BaseStream, message.DestinationMarshalByRef);
		}
		else
		{
			binaryWriter.Write(value: false);
		}
		binaryWriter.Write((short)message.SecuritySessionParameters.Attributes);
		binaryWriter.Write(message.SecuritySessionParameters.RemoteTransportUser);
		foreach (object iTransportHeader in message.ITransportHeaders)
		{
			DictionaryEntry dictionaryEntry = (DictionaryEntry)iTransportHeader;
			string text = dictionaryEntry.Key as string;
			switch (text)
			{
			case null:
			case "__":
			case "__genbdc":
				continue;
			}
			if (dictionaryEntry.Value != null && !(text == "__genmt"))
			{
				string value = dictionaryEntry.Value.ToString();
				binaryWriter.Write(text);
				binaryWriter.Write(value);
			}
		}
		binaryWriter.Write("__");
		genuineChunkedStream.WriteStream(message.Stream);
		if (compress)
		{
			GZipOutputStream gZipOutputStream = new GZipOutputStream(new NonClosableStream(stream));
			GenuineUtility.CopyStreamToStream(genuineChunkedStream, gZipOutputStream);
			gZipOutputStream.Finish();
		}
	}

	public static Message Deserialize(Stream stream, string securitySessionName)
	{
		BinaryReader binaryReader = new BinaryReader(stream);
		if (binaryReader.ReadBoolean())
		{
			stream = new GZipInputStream(stream);
		}
		binaryReader = new BinaryReader(stream);
		Message message = new Message();
		message.GenuineMessageType = (GenuineMessageType)binaryReader.ReadByte();
		message.MessageId = binaryReader.ReadInt32();
		message.ReplyToId = binaryReader.ReadInt32();
		message.IsOneWay = binaryReader.ReadBoolean();
		if (binaryReader.ReadBoolean())
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			message.DestinationMarshalByRef = binaryFormatter.Deserialize(stream);
		}
		short securitySessionAttributes = binaryReader.ReadInt16();
		string remoteTransportUser = binaryReader.ReadString();
		message.SecuritySessionParameters = new SecuritySessionParameters(securitySessionName, (SecuritySessionAttributes)securitySessionAttributes, TimeSpan.MinValue, remoteTransportUser);
		message.ITransportHeaders = new TransportHeaders();
		while (true)
		{
			string text = binaryReader.ReadString();
			if (text == "__")
			{
				break;
			}
			string value = binaryReader.ReadString();
			message.ITransportHeaders[text] = value;
		}
		message.Stream = stream;
		return message;
	}

	public static int ReadInt32(byte[] content, int position)
	{
		return Buffer.GetByte(content, position) | (Buffer.GetByte(content, position + 1) << 8) | (Buffer.GetByte(content, position + 2) << 16) | (Buffer.GetByte(content, position + 3) << 24);
	}

	public static void WriteInt32(byte[] content, int position, int val)
	{
		Buffer.SetByte(content, position, (byte)val);
		Buffer.SetByte(content, position + 1, (byte)(val >> 8));
		Buffer.SetByte(content, position + 2, (byte)(val >> 16));
		Buffer.SetByte(content, position + 3, (byte)(val >> 24));
	}

	public static BinaryWriter SerializeConnectionHeader(byte protocolVersion, GenuineConnectionType genuineConnectionType, string connectionId)
	{
		GenuineChunkedStream output = new GenuineChunkedStream(releaseOnReadMode: false);
		BinaryWriter binaryWriter = new BinaryWriter(output);
		binaryWriter.Write(protocolVersion);
		binaryWriter.Write((byte)genuineConnectionType);
		binaryWriter.Write(connectionId);
		return binaryWriter;
	}

	public static void DeserializeConnectionHeader(BinaryReader binaryReader, out byte protocolVersion, out GenuineConnectionType genuineConnectionType, out string connectionId)
	{
		connectionId = "/__GC/Default";
		protocolVersion = binaryReader.ReadByte();
		genuineConnectionType = (GenuineConnectionType)binaryReader.ReadByte();
		if (!Enum.IsDefined(typeof(GenuineConnectionType), genuineConnectionType) || genuineConnectionType == GenuineConnectionType.None || genuineConnectionType == GenuineConnectionType.All)
		{
			throw GenuineExceptions.Get_Connect_CanNotAcceptIncomingConnection("Incorrect type of the connection.");
		}
		if (protocolVersion > 0)
		{
			connectionId = binaryReader.ReadString();
			if (connectionId == null || connectionId.Length <= 0)
			{
				throw GenuineExceptions.Get_Connect_CanNotAcceptIncomingConnection("Incorrect name of the connection.");
			}
		}
	}

	public static void WriteLabelledStream(Stream inputStream, GenuineChunkedStream outputStream, BinaryWriter writer, byte[] intermediateBuffer)
	{
		if (inputStream.CanSeek)
		{
			writer.Write((byte)55);
			writer.Write((int)inputStream.Length);
			writer.Write((byte)1);
			outputStream.WriteStream(inputStream);
			return;
		}
		int num;
		do
		{
			num = inputStream.Read(intermediateBuffer, 0, intermediateBuffer.Length);
			writer.Write((byte)55);
			writer.Write(num);
			writer.Write((byte)((num < intermediateBuffer.Length) ? 1u : 0u));
			writer.BaseStream.Write(intermediateBuffer, 0, num);
		}
		while (num >= intermediateBuffer.Length);
	}

	public static void FillInLabelledStream(Message message, MessageContainer messageContainer, ArrayList packedMessages, GenuineChunkedStream resultContent, byte[] intermediateBuffer, int recommendedSize)
	{
		BinaryWriter binaryWriter = new BinaryWriter(resultContent);
		packedMessages?.Clear();
		while (message != null)
		{
			binaryWriter.Write((byte)0);
			packedMessages?.Add(message);
			WriteLabelledStream(message.SerializedContent, resultContent, binaryWriter, intermediateBuffer);
			message = null;
			if (resultContent.Length > recommendedSize)
			{
				break;
			}
			if (messageContainer != null)
			{
				message = messageContainer.GetMessage();
			}
			if (message == null)
			{
				break;
			}
		}
		binaryWriter.Write((byte)1);
	}
}
