using System;

namespace Redbox.Core;

internal interface Redbox_002EMacros1938_002EIConsole : IDisposable
{
	bool IsAttachedToConsole { get; }

	void Write(string value);

	void WriteLine();

	void WriteLine(object value);

	void WriteLine(string value);

	void WriteLine(string format, params object[] parms);
}
