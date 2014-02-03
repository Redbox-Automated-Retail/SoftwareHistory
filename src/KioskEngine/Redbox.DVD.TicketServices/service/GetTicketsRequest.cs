using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Redbox.TicketServices.Proxy.service;

[Serializable]
[DebuggerStepThrough]
[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
[DataContract(Name = "GetTicketsRequest", Namespace = "http://schemas.datacontract.org/2004/07/Redbox.Ticketing.ControllerServices.Contract.Schema.GetTickets")]
internal class GetTicketsRequest : Request
{
	private PriceCode[] PriceCodesField;

	private int ProductionIdField;

	[OptionalField]
	private UnlockTicket[] UnlockTicketsField;

	[DataMember(IsRequired = true)]
	public PriceCode[] PriceCodes
	{
		get
		{
			return PriceCodesField;
		}
		set
		{
			if (!object.ReferenceEquals(PriceCodesField, value))
			{
				PriceCodesField = value;
				RaisePropertyChanged("PriceCodes");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public int ProductionId
	{
		get
		{
			return ProductionIdField;
		}
		set
		{
			if (!ProductionIdField.Equals(value))
			{
				ProductionIdField = value;
				RaisePropertyChanged("ProductionId");
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
