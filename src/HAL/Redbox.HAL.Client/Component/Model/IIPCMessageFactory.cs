namespace Redbox.HAL.Component.Model;

internal interface IIPCMessageFactory
{
	IIPCMessage Create(IPipeChannel channel);

	bool Write(IIPCMessage msg, IPipeChannel channel);
}
