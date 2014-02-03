using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Redbox.TicketServices.Proxy.service;

[Serializable]
[DataContract(Name = "GetTicketsResponse", Namespace = "http://schemas.datacontract.org/2004/07/Redbox.Ticketing.ControllerServices.Contract.Schema.GetTickets")]
[DebuggerStepThrough]
[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
internal class GetTicketsResponse : Response
{
	[OptionalField]
	private int ProductionIdField;

	private SupplierDisclaimer[] SupplierDisclaimersField;

	private TicketGroup[] TicketGroupsField;

	[DataMember]
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

	[DataMember(IsRequired = true)]
	public SupplierDisclaimer[] SupplierDisclaimers
	{
		get
		{
			return SupplierDisclaimersField;
		}
		set
		{
			if (!object.ReferenceEquals(SupplierDisclaimersField, value))
			{
				SupplierDisclaimersField = value;
				RaisePropertyChanged("SupplierDisclaimers");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public TicketGroup[] TicketGroups
	{
		get
		{
			return TicketGroupsField;
		}
		set
		{
			if (!object.ReferenceEquals(TicketGroupsField, value))
			{
				TicketGroupsField = value;
				RaisePropertyChanged("TicketGroups");
			}
		}
	}
}
