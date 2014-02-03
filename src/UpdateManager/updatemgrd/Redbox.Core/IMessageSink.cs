namespace Redbox.Core;

internal interface IMessageSink
{
	bool Send(string message);
}
