using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Redbox.TicketServices.Proxy.service;

[Serializable]
[DebuggerStepThrough]
[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
[DataContract(Name = "PlaceOrderResponse", Namespace = "http://schemas.datacontract.org/2004/07/Redbox.Ticketing.ControllerServices.Contract.Schema.PlaceOrder")]
internal class PlaceOrderResponse : Response
{
	private int OrderIdField;

	[DataMember(IsRequired = true)]
	public int OrderId
	{
		get
		{
			return OrderIdField;
		}
		set
		{
			if (!OrderIdField.Equals(value))
			{
				OrderIdField = value;
				RaisePropertyChanged("OrderId");
			}
		}
	}
}
