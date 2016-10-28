using System.Collections.Generic;

namespace Redbox.KioskServices.Proxy.ComponentModel;

public interface IRemoteServiceProviderInstruction
{
	InstructionType Type { get; }

	IDictionary<string, object> Parameters { get; }
}
