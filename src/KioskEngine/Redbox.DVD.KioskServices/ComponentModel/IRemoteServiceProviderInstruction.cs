using System.Collections.Generic;

namespace Redbox.KioskServices.Proxy.ComponentModel;

internal interface IRemoteServiceProviderInstruction
{
	InstructionType Type { get; }

	IDictionary<string, object> Parameters { get; }
}
