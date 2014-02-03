using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Redbox.TicketServices.Proxy.service;

[Serializable]
[DataContract(Name = "Subscription", Namespace = "http://schemas.datacontract.org/2004/07/Redbox.Ticketing.ControllerServices.Contract.Schema.SendNotification")]
[DebuggerStepThrough]
[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
internal class Subscription : IExtensibleDataObject, INotifyPropertyChanged
{
	[NonSerialized]
	private ExtensionDataObject extensionDataField;

	private NotificationType NotificationTypeField;

	private SubscriptionType SubscriptionTypeField;

	private string SubscriptionValueField;

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
	public NotificationType NotificationType
	{
		get
		{
			return NotificationTypeField;
		}
		set
		{
			if (!NotificationTypeField.Equals(value))
			{
				NotificationTypeField = value;
				RaisePropertyChanged("NotificationType");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public SubscriptionType SubscriptionType
	{
		get
		{
			return SubscriptionTypeField;
		}
		set
		{
			if (!SubscriptionTypeField.Equals(value))
			{
				SubscriptionTypeField = value;
				RaisePropertyChanged("SubscriptionType");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public string SubscriptionValue
	{
		get
		{
			return SubscriptionValueField;
		}
		set
		{
			if (!object.ReferenceEquals(SubscriptionValueField, value))
			{
				SubscriptionValueField = value;
				RaisePropertyChanged("SubscriptionValue");
			}
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected void RaisePropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
