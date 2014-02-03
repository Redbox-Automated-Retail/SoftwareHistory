using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Redbox.TicketServices.Proxy.service;

[Serializable]
[DataContract(Name = "TaxItem", Namespace = "http://schemas.datacontract.org/2004/07/Redbox.Ticketing.ControllerServices.Contract.Schema.GetTickets")]
[DebuggerStepThrough]
[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
internal class TaxItem : IExtensibleDataObject, INotifyPropertyChanged
{
	[NonSerialized]
	private ExtensionDataObject extensionDataField;

	private decimal TaxAmtField;

	private int TaxTypeIdField;

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
	public decimal TaxAmt
	{
		get
		{
			return TaxAmtField;
		}
		set
		{
			if (!TaxAmtField.Equals(value))
			{
				TaxAmtField = value;
				RaisePropertyChanged("TaxAmt");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public int TaxTypeId
	{
		get
		{
			return TaxTypeIdField;
		}
		set
		{
			if (!TaxTypeIdField.Equals(value))
			{
				TaxTypeIdField = value;
				RaisePropertyChanged("TaxTypeId");
			}
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected void RaisePropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
