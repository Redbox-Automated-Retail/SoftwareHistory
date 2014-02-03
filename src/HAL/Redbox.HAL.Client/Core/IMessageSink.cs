namespace Redbox.HAL.Core;

internal interface IMessageSink
{
	bool Send(string message);
}
