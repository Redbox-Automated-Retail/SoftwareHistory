using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Redbox.TicketServices.Proxy.service;

[Serializable]
[DataContract(Name = "SendNotificationRequest", Namespace = "http://schemas.datacontract.org/2004/07/Redbox.Ticketing.ControllerServices.Contract.Schema.SendNotification")]
[DebuggerStepThrough]
[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
internal class SendNotificationRequest : Request
{
	private string CustomerEmailField;

	private string CustomerSMSPhoneField;

	private Subscription[] SubscriptionsField;

	[DataMember(IsRequired = true)]
	public string CustomerEmail
	{
		get
		{
			return CustomerEmailField;
		}
		set
		{
			if (!object.ReferenceEquals(CustomerEmailField, value))
			{
				CustomerEmailField = value;
				RaisePropertyChanged("CustomerEmail");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public string CustomerSMSPhone
	{
		get
		{
			return CustomerSMSPhoneField;
		}
		set
		{
			if (!object.ReferenceEquals(CustomerSMSPhoneField, value))
			{
				CustomerSMSPhoneField = value;
				RaisePropertyChanged("CustomerSMSPhone");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public Subscription[] Subscriptions
	{
		get
		{
			return SubscriptionsField;
		}
		set
		{
			if (!object.ReferenceEquals(SubscriptionsField, value))
			{
				SubscriptionsField = value;
				RaisePropertyChanged("Subscriptions");
			}
		}
	}
}
