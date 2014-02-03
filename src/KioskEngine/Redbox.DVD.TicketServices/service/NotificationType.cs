using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace Redbox.TicketServices.Proxy.service;

[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
[DataContract(Name = "NotificationType", Namespace = "http://schemas.datacontract.org/2004/07/Redbox.Ticketing.ControllerServices.Contract.Schema.SendNotification")]
internal enum NotificationType
{
	[EnumMember]
	Email = 2,
	[EnumMember]
	SMS = 4
}
