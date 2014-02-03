using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Redbox.TicketServices.Proxy.service;

[Serializable]
[KnownType(typeof(GetEmailByCardRequest))]
[DataContract(Name = "Request", Namespace = "http://schemas.datacontract.org/2004/07/Redbox.Ticketing.ControllerServices.Contract.Schema")]
[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
[KnownType(typeof(UpdateTicketLockRequest))]
[KnownType(typeof(ReleaseTicketLockRequest))]
[KnownType(typeof(SendNotificationRequest))]
[KnownType(typeof(TicketLockRequest))]
[KnownType(typeof(PlaceOrderRequest))]
[KnownType(typeof(GetTicketsRequest))]
[DebuggerStepThrough]
internal class Request : IExtensibleDataObject, INotifyPropertyChanged
{
	[NonSerialized]
	private ExtensionDataObject extensionDataField;

	private string BundleVersionField;

	private ChannelType ChannelTypeField;

	private string EngineVersionField;

	private int KioskIdField;

	private DateTime MessageDateTimeUtcField;

	private Guid MessageIdField;

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
	public string BundleVersion
	{
		get
		{
			return BundleVersionField;
		}
		set
		{
			if (!object.ReferenceEquals(BundleVersionField, value))
			{
				BundleVersionField = value;
				RaisePropertyChanged("BundleVersion");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public ChannelType ChannelType
	{
		get
		{
			return ChannelTypeField;
		}
		set
		{
			if (!ChannelTypeField.Equals(value))
			{
				ChannelTypeField = value;
				RaisePropertyChanged("ChannelType");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public string EngineVersion
	{
		get
		{
			return EngineVersionField;
		}
		set
		{
			if (!object.ReferenceEquals(EngineVersionField, value))
			{
				EngineVersionField = value;
				RaisePropertyChanged("EngineVersion");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public int KioskId
	{
		get
		{
			return KioskIdField;
		}
		set
		{
			if (!KioskIdField.Equals(value))
			{
				KioskIdField = value;
				RaisePropertyChanged("KioskId");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public DateTime MessageDateTimeUtc
	{
		get
		{
			return MessageDateTimeUtcField;
		}
		set
		{
			if (!MessageDateTimeUtcField.Equals(value))
			{
				MessageDateTimeUtcField = value;
				RaisePropertyChanged("MessageDateTimeUtc");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public Guid MessageId
	{
		get
		{
			return MessageIdField;
		}
		set
		{
			if (!MessageIdField.Equals(value))
			{
				MessageIdField = value;
				RaisePropertyChanged("MessageId");
			}
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected void RaisePropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
