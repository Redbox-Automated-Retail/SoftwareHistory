using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Common.GUI.Functions;

public sealed class NilFormattedLog : IFormattedLog
{
	public static NilFormattedLog Instance => Singleton<NilFormattedLog>.Instance;

	public void WriteFormatted(string msg)
	{
	}

	public void WriteFormatted(string msg, LogEntryType type)
	{
	}

	public void WriteFormatted(string format, params object[] stuff)
	{
	}

	public void WriteFormatted(LogEntryType type, string format, params object[] stuff)
	{
	}

	private NilFormattedLog()
	{
	}
}
