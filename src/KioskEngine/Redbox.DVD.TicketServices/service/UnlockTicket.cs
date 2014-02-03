using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Redbox.TicketServices.Proxy.service;

[Serializable]
[DataContract(Name = "UnlockTicket", Namespace = "http://schemas.datacontract.org/2004/07/Redbox.Ticketing.ControllerServices.Contract.Schema")]
[DebuggerStepThrough]
[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
internal class UnlockTicket : IExtensibleDataObject, INotifyPropertyChanged
{
	[NonSerialized]
	private ExtensionDataObject extensionDataField;

	private int QuantityField;

	private Guid TicketGroupGuidField;

	private int TicketGroupIdField;

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

	[DataMember(IsRequired = true)]
	public Guid TicketGroupGuid
	{
		get
		{
			return TicketGroupGuidField;
		}
		set
		{
			if (!TicketGroupGuidField.Equals(value))
			{
				TicketGroupGuidField = value;
				RaisePropertyChanged("TicketGroupGuid");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public int TicketGroupId
	{
		get
		{
			return TicketGroupIdField;
		}
		set
		{
			if (!TicketGroupIdField.Equals(value))
			{
				TicketGroupIdField = value;
				RaisePropertyChanged("TicketGroupId");
			}
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected void RaisePropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
