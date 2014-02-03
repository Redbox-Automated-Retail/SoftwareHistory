using System;
using VistaDB.Provider;

namespace Redbox.HAL.DataStorage;

internal class VistaDBReader : IDataReader
{
	private VistaDBDataReader m_reader;

	public int ReadInt(int idx)
	{
		return m_reader.GetInt32(idx);
	}

	public bool ReadBoolean(int idx)
	{
		return m_reader.GetBoolean(idx);
	}

	public long ReadInt64(int idx)
	{
		return m_reader.GetInt64(idx);
	}

	public string ReadString(int idx)
	{
		return m_reader.GetString(idx);
	}

	public bool IsNull(int idx)
	{
		return m_reader.IsDBNull(idx);
	}

	public DateTime? ReadDateTime(int idx)
	{
		if (m_reader.IsDBNull(idx))
		{
			return null;
		}
		return m_reader.GetDateTime(idx);
	}

	internal VistaDBReader(VistaDBDataReader reader)
	{
		m_reader = reader;
	}
}
