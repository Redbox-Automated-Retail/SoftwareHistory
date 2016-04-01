namespace Redbox.HAL.Component.Model;

internal interface IMessageSink
{
	bool Send(string message);
}
