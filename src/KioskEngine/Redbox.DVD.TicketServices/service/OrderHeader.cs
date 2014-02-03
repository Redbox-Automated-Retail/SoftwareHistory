using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Redbox.TicketServices.Proxy.service;

[Serializable]
[DebuggerStepThrough]
[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
[DataContract(Name = "OrderHeader", Namespace = "http://schemas.datacontract.org/2004/07/Redbox.Ticketing.ControllerServices.Contract.Schema.PlaceOrder")]
internal class OrderHeader : IExtensibleDataObject, INotifyPropertyChanged
{
	[NonSerialized]
	private ExtensionDataObject extensionDataField;

	private int OrderIdField;

	private decimal SubtotalField;

	private decimal TotalField;

	private decimal TotalFeeField;

	private decimal TotalTaxField;

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

	[DataMember(IsRequired = true)]
	public decimal Subtotal
	{
		get
		{
			return SubtotalField;
		}
		set
		{
			if (!SubtotalField.Equals(value))
			{
				SubtotalField = value;
				RaisePropertyChanged("Subtotal");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public decimal Total
	{
		get
		{
			return TotalField;
		}
		set
		{
			if (!TotalField.Equals(value))
			{
				TotalField = value;
				RaisePropertyChanged("Total");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public decimal TotalFee
	{
		get
		{
			return TotalFeeField;
		}
		set
		{
			if (!TotalFeeField.Equals(value))
			{
				TotalFeeField = value;
				RaisePropertyChanged("TotalFee");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public decimal TotalTax
	{
		get
		{
			return TotalTaxField;
		}
		set
		{
			if (!TotalTaxField.Equals(value))
			{
				TotalTaxField = value;
				RaisePropertyChanged("TotalTax");
			}
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected void RaisePropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
