using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.KioskServices.Proxy;

public class RemoteServiceProviderInstruction : IRemoteServiceProviderInstruction
{
	private readonly IDictionary<string, object> m_parameters = new Dictionary<string, object>();

	public InstructionType Type { get; internal set; }

	public IDictionary<string, object> Parameters => m_parameters;
}
