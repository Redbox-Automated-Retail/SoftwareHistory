namespace Redbox.HAL.Component.Model;

public interface IIPCMessageFactory
{
	IIPCMessage Create(IPipeChannel channel);

	bool Write(IIPCMessage msg, IPipeChannel channel);
}
