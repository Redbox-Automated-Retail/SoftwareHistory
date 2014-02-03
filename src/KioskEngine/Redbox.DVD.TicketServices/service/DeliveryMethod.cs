using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Redbox.TicketServices.Proxy.service;

[Serializable]
[DataContract(Name = "DeliveryMethod", Namespace = "http://schemas.datacontract.org/2004/07/Redbox.Ticketing.ControllerServices.Contract.Schema.GetTickets")]
[DebuggerStepThrough]
[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
internal class DeliveryMethod : IExtensibleDataObject, INotifyPropertyChanged
{
	[NonSerialized]
	private ExtensionDataObject extensionDataField;

	private string DeliveryMethodDescriptionField;

	private string DeliveryMethodDisplayNameField;

	private int DeliveryMethodIdField;

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
	public string DeliveryMethodDescription
	{
		get
		{
			return DeliveryMethodDescriptionField;
		}
		set
		{
			if (!object.ReferenceEquals(DeliveryMethodDescriptionField, value))
			{
				DeliveryMethodDescriptionField = value;
				RaisePropertyChanged("DeliveryMethodDescription");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public string DeliveryMethodDisplayName
	{
		get
		{
			return DeliveryMethodDisplayNameField;
		}
		set
		{
			if (!object.ReferenceEquals(DeliveryMethodDisplayNameField, value))
			{
				DeliveryMethodDisplayNameField = value;
				RaisePropertyChanged("DeliveryMethodDisplayName");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public int DeliveryMethodId
	{
		get
		{
			return DeliveryMethodIdField;
		}
		set
		{
			if (!DeliveryMethodIdField.Equals(value))
			{
				DeliveryMethodIdField = value;
				RaisePropertyChanged("DeliveryMethodId");
			}
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected void RaisePropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
