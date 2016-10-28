using System.Runtime.InteropServices;

namespace Alsing.Windows;

[ComVisible(true)]
public enum TextBoxMessages
{
	EM_GETSEL = 176,
	EM_LINEINDEX = 187,
	EM_LINEFROMCHAR = 201,
	EM_POSFROMCHAR = 214
}
