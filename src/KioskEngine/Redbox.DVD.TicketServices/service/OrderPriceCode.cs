using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Redbox.TicketServices.Proxy.service;

[Serializable]
[DebuggerStepThrough]
[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
[DataContract(Name = "OrderPriceCode", Namespace = "http://schemas.datacontract.org/2004/07/Redbox.Ticketing.ControllerServices.Contract.Schema.PlaceOrder")]
internal class OrderPriceCode : IExtensibleDataObject, INotifyPropertyChanged
{
	[NonSerialized]
	private ExtensionDataObject extensionDataField;

	private decimal FeePerTicketField;

	private decimal FeePerTicketTaxField;

	private OrderTaxItem[] OrderTaxCollectionField;

	private int PriceCodeIdField;

	private decimal PricePerTicketField;

	private decimal PricePerTicketTaxField;

	private int QuantityField;

	[Browsable(false)]
	public ExtensionDataObject ExtensionData
	{
		get
		{
			return extensionDataField;
		}
		set
		{
			extensionDataField = value;
		}
	}

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
	public OrderTaxItem[] OrderTaxCollection
	{
		get
		{
			return OrderTaxCollectionField;
		}
		set
		{
			if (!object.ReferenceEquals(OrderTaxCollectionField, value))
			{
				OrderTaxCollectionField = value;
				RaisePropertyChanged("OrderTaxCollection");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public int PriceCodeId
	{
		get
		{
			return PriceCodeIdField;
		}
		set
		{
			if (!PriceCodeIdField.Equals(value))
			{
				PriceCodeIdField = value;
				RaisePropertyChanged("PriceCodeId");
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
	public int Quantity
	{
		get
		{
			return QuantityField;
		}
		set
		{
			if (!QuantityField.Equals(value))
			{
				QuantityField = value;
				RaisePropertyChanged("Quantity");
			}
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected void RaisePropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
