using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Redbox.TicketServices.Proxy.service;

[Serializable]
[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
[DataContract(Name = "PlaceOrderRequest", Namespace = "http://schemas.datacontract.org/2004/07/Redbox.Ticketing.ControllerServices.Contract.Schema.PlaceOrder")]
[DebuggerStepThrough]
internal class PlaceOrderRequest : Request
{
	private Order OrderField;

	[OptionalField]
	private UnlockTicket[] UnlockTicketsField;

	[DataMember(IsRequired = true)]
	public Order Order
	{
		get
		{
			return OrderField;
		}
		set
		{
			if (!object.ReferenceEquals(OrderField, value))
			{
				OrderField = value;
				RaisePropertyChanged("Order");
			}
		}
	}

	[DataMember]
	public UnlockTicket[] UnlockTickets
	{
		get
		{
			return UnlockTicketsField;
		}
		set
		{
			if (!object.ReferenceEquals(UnlockTicketsField, value))
			{
				UnlockTicketsField = value;
				RaisePropertyChanged("UnlockTickets");
			}
		}
	}
}
