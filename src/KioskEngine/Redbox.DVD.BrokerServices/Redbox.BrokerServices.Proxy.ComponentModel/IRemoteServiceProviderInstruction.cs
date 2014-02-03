using System.Collections.Generic;

namespace Redbox.BrokerServices.Proxy.ComponentModel;

internal interface IRemoteServiceProviderInstruction
{
	InstructionType Type { get; }

	IDictionary<string, object> Parameters { get; }
}
