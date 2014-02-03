using System;
using System.IO;
using Belikov.GenuineChannels.Connection;

namespace Belikov.GenuineChannels.GenuineHttp;

internal class HttpMessageCoder
{
	private HttpMessageCoder()
	{
	}

	public static void WriteRequestHeader(BinaryWriter binaryWriter, byte protocolVersion, GenuineConnectionType genuineConnectionType, byte[] hostId, HttpPacketType httpPacketType, int sequenceNo, string connectionName, int localHostUniqueIdentifier)
	{
		binaryWriter.Write((byte)55);
		binaryWriter.Write(protocolVersion);
		binaryWriter.Write((byte)genuineConnectionType);
		binaryWriter.Write(hostId);
		binaryWriter.Write((byte)httpPacketType);
		binaryWriter.Write(sequenceNo);
		binaryWriter.Write(connectionName);
		binaryWriter.Write(localHostUniqueIdentifier);
	}

	public static void ReadRequestHeader(BinaryReader binaryReader, out byte protocolVersion, out GenuineConnectionType genuineConnectionType, out Guid hostId, out HttpPacketType httpPacketType, out int sequenceNo, out string connectionName, out int remoteHostUniqueIdentifier)
	{
		if (binaryReader.ReadByte() != 55)
		{
			throw GenuineExceptions.Get_Receive_IncorrectData();
		}
		protocolVersion = binaryReader.ReadByte();
		genuineConnectionType = (GenuineConnectionType)binaryReader.ReadByte();
		hostId = new Guid(binaryReader.ReadBytes(16));
		httpPacketType = (HttpPacketType)binaryReader.ReadByte();
		sequenceNo = binaryReader.ReadInt32();
		connectionName = "$/__GC/" + hostId;
		remoteHostUniqueIdentifier = 0;
		if (protocolVersion >= 1)
		{
			connectionName = binaryReader.ReadString();
			remoteHostUniqueIdentifier = binaryReader.ReadInt32();
		}
	}

	public static void WriteResponseHeader(BinaryWriter binaryWriter, byte protocolVersion, string serverUri, int sequenceNo, HttpPacketType httpPacketType, int localHostUniqueIdentifier)
	{
		binaryWriter.Write((byte)55);
		binaryWriter.Write(serverUri);
		binaryWriter.Write(sequenceNo);
		binaryWriter.Write((byte)httpPacketType);
		if (protocolVersion > 0)
		{
			binaryWriter.Write(localHostUniqueIdentifier);
		}
	}

	public static void ReadResponseHeader(BinaryReader binaryReader, out string serverUri, out int sequenceNo, out HttpPacketType httpPacketType, out int remoteHostUniqueIdentifier)
	{
		if (binaryReader.ReadByte() != 55)
		{
			throw GenuineExceptions.Get_Receive_IncorrectData();
		}
		serverUri = binaryReader.ReadString();
		sequenceNo = binaryReader.ReadInt32();
		httpPacketType = (HttpPacketType)binaryReader.ReadByte();
		remoteHostUniqueIdentifier = binaryReader.ReadInt32();
	}
}
