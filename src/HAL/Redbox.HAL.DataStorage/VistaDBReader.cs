using System;
using Redbox.HAL.Component.Model;
using VistaDB.Provider;

namespace Redbox.HAL.DataStorage;

internal sealed class VistaDBReader : IDataReader
{
	private readonly VistaDBDataReader Reader;

	public int ReadInt(int idx)
	{
		return Reader.GetInt32(idx);
	}

	public bool ReadBoolean(int idx)
	{
		return Reader.GetBoolean(idx);
	}

	public long ReadInt64(int idx)
	{
		return Reader.GetInt64(idx);
	}

	public string ReadString(int idx)
	{
		return Reader.GetString(idx);
	}

	public bool IsNull(int idx)
	{
		return Reader.IsDBNull(idx);
	}

	public DateTime? ReadDateTime(int idx)
	{
		if (Reader.IsDBNull(idx))
		{
			return null;
		}
		return Reader.GetDateTime(idx);
	}

	public decimal ReadDecimal(int idx)
	{
		return Reader.GetDecimal(idx);
	}

	internal VistaDBReader(VistaDBDataReader reader)
	{
		Reader = reader;
	}
}
