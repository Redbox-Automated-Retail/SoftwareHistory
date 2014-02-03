using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Redbox.TicketServices.Proxy.service;

[Serializable]
[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
[DataContract(Name = "PriceCodeResponse", Namespace = "http://schemas.datacontract.org/2004/07/Redbox.Ticketing.ControllerServices.Contract.Schema.GetTickets")]
[DebuggerStepThrough]
internal class PriceCodeResponse : PriceCode
{
	private decimal FeePerTicketField;

	private decimal FeePerTicketTaxField;

	private decimal PricePerTicketField;

	private decimal PricePerTicketTaxField;

	private TaxItem[] TaxCollectionField;

	[DataMember(IsRequired = true)]
	public decimal FeePerTicket
	{
		get
		{
			return FeePerTicketField;
		}
		set
		{
			if (!FeePerTicketField.Equals(value))
			{
				FeePerTicketField = value;
				RaisePropertyChanged("FeePerTicket");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public decimal FeePerTicketTax
	{
		get
		{
			return FeePerTicketTaxField;
		}
		set
		{
			if (!FeePerTicketTaxField.Equals(value))
			{
				FeePerTicketTaxField = value;
				RaisePropertyChanged("FeePerTicketTax");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public decimal PricePerTicket
	{
		get
		{
			return PricePerTicketField;
		}
		set
		{
			if (!PricePerTicketField.Equals(value))
			{
				PricePerTicketField = value;
				RaisePropertyChanged("PricePerTicket");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public decimal PricePerTicketTax
	{
		get
		{
			return PricePerTicketTaxField;
		}
		set
		{
			if (!PricePerTicketTaxField.Equals(value))
			{
				PricePerTicketTaxField = value;
				RaisePropertyChanged("PricePerTicketTax");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public TaxItem[] TaxCollection
	{
		get
		{
			return TaxCollectionField;
		}
		set
		{
			if (!object.ReferenceEquals(TaxCollectionField, value))
			{
				TaxCollectionField = value;
				RaisePropertyChanged("TaxCollection");
			}
		}
	}
}
