using System.Runtime.InteropServices;

namespace Belikov.GenuineChannels.Utilities;

[ComVisible(true)]
public interface ITimerConsumer
{
	void TimerCallback();
}
