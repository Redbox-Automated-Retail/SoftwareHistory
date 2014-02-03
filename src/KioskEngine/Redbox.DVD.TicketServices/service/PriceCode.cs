using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Redbox.TicketServices.Proxy.service;

[Serializable]
[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
[KnownType(typeof(PriceCodeResponse))]
[DataContract(Name = "PriceCode", Namespace = "http://schemas.datacontract.org/2004/07/Redbox.Ticketing.ControllerServices.Contract.Schema.GetTickets")]
[DebuggerStepThrough]
internal class PriceCode : IExtensibleDataObject, INotifyPropertyChanged
{
	[NonSerialized]
	private ExtensionDataObject extensionDataField;

	private int PriceCodeIdField;

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
