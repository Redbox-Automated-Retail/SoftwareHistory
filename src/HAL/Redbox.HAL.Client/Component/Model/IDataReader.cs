using System;

namespace Redbox.HAL.Component.Model;

internal interface IDataReader
{
	bool ReadBoolean(int idx);

	int ReadInt(int idx);

	long ReadInt64(int idx);

	string ReadString(int idx);

	DateTime? ReadDateTime(int idx);

	bool IsNull(int idx);

	decimal ReadDecimal(int idx);
}
