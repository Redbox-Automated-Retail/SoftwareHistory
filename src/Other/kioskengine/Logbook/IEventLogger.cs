using System.Runtime.InteropServices;

namespace Belikov.GenuineChannels.Logbook;

[ComVisible(true)]
public interface IEventLogger
{
	BinaryLogWriter BinaryLogWriter { get; }
}
