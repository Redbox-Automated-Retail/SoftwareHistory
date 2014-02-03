using System.Collections.Generic;
using Redbox.BrokerServices.Proxy.ComponentModel;

namespace Redbox.BrokerServices.Proxy;

internal class RemoteServiceProviderInstruction : IRemoteServiceProviderInstruction
{
	private readonly IDictionary<string, object> m_parameters = new Dictionary<string, object>();

	public InstructionType Type { get; internal set; }

	public IDictionary<string, object> Parameters => m_parameters;
}
