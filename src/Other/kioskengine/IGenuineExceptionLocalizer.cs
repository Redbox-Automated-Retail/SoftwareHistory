using System.Runtime.InteropServices;

namespace Belikov.GenuineChannels;

[ComVisible(true)]
public interface IGenuineExceptionLocalizer
{
	string Localize(string errorId, params object[] messageParameters);
}
