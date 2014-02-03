namespace Belikov.GenuineChannels.Logbook;

internal interface IEventLogger
{
	BinaryLogWriter BinaryLogWriter { get; }
}
