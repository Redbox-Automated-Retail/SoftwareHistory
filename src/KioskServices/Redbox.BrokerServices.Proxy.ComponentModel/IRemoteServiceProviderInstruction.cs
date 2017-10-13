using System.Collections.Generic;

namespace Redbox.BrokerServices.Proxy.ComponentModel;

public interface IRemoteServiceProviderInstruction
{
	InstructionType Type { get; }

	IDictionary<string, object> Parameters { get; }
}
