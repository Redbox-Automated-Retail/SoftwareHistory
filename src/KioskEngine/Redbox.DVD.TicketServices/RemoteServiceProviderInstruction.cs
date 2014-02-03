using System.Collections.Generic;
using Redbox.KioskServices.Proxy.ComponentModel;

namespace Redbox.TicketServices.Proxy;

internal class RemoteServiceProviderInstruction : IRemoteServiceProviderInstruction
{
	private readonly IDictionary<string, object> m_parameters = new Dictionary<string, object>();

	public InstructionType Type { get; internal set; }

	public IDictionary<string, object> Parameters => m_parameters;
}
