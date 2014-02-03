using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace Redbox.TicketServices.Proxy.service;

[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
[DataContract(Name = "ChannelType", Namespace = "http://schemas.datacontract.org/2004/07/Redbox.Ticketing.ControllerServices.Contract.Schema")]
internal enum ChannelType
{
	[EnumMember]
	Web = 2,
	[EnumMember]
	Kiosk = 4
}
