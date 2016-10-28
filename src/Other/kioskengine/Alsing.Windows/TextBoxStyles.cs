using System.Runtime.InteropServices;

namespace Alsing.Windows;

[ComVisible(true)]
public enum TextBoxStyles
{
	ES_LEFT = 0,
	ES_CENTER = 1,
	ES_RIGHT = 2,
	ES_MULTILINE = 4,
	ES_UPPERCASE = 8,
	ES_LOWERCASE = 0x10,
	ES_PASSWORD = 0x20,
	ES_AUTOVSCROLL = 0x40,
	ES_AUTOHSCROLL = 0x80,
	ES_NOHIDESEL = 0x100,
	ES_OEMCONVERT = 0x400,
	ES_READONLY = 0x800,
	ES_WANTRETURN = 0x1000
}
