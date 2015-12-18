using System;

namespace Redbox.Core;

internal interface Redbox_002ECommand_002ETokenizer2108_002EILogger
{
	void Configure();

	void Configure(string path);

	void ConfigureAndWatch(string path);

	void Log(string message, Exception e);

	void Log(string message, Redbox_002ECommand_002ETokenizer2108_002ELogEntryType type);

	void Log(string message, params object[] parms);

	void Log(string message, Exception e, Redbox_002ECommand_002ETokenizer2108_002ELogEntryType type);
}
