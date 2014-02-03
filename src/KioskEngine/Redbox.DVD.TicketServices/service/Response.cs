using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Redbox.TicketServices.Proxy.service;

[Serializable]
[DebuggerStepThrough]
[KnownType(typeof(ReleaseTicketLockResponse))]
[KnownType(typeof(SendNotificationResponse))]
[KnownType(typeof(GetEmailByCardResponse))]
[KnownType(typeof(PlaceOrderResponse))]
[KnownType(typeof(GetTicketsResponse))]
[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
[DataContract(Name = "Response", Namespace = "http://schemas.datacontract.org/2004/07/Redbox.Ticketing.ControllerServices.Contract.Schema")]
[KnownType(typeof(UpdateTicketLockResponse))]
internal class Response : IExtensibleDataObject, INotifyPropertyChanged
{
	[NonSerialized]
	private ExtensionDataObject extensionDataField;

	[OptionalField]
	private string CodeField;

	[OptionalField]
	private string MessageField;

	private bool SuccessField;

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

	[DataMember]
	public string Code
	{
		get
		{
			return CodeField;
		}
		set
		{
			if (!object.ReferenceEquals(CodeField, value))
			{
				CodeField = value;
				RaisePropertyChanged("Code");
			}
		}
	}

	[DataMember]
	public string Message
	{
		get
		{
			return MessageField;
		}
		set
		{
			if (!object.ReferenceEquals(MessageField, value))
			{
				MessageField = value;
				RaisePropertyChanged("Message");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public bool Success
	{
		get
		{
			return SuccessField;
		}
		set
		{
			if (!SuccessField.Equals(value))
			{
				SuccessField = value;
				RaisePropertyChanged("Success");
			}
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected void RaisePropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
