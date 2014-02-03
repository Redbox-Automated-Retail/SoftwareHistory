using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Redbox.TicketServices.Proxy.service;

[Serializable]
[DebuggerStepThrough]
[DataContract(Name = "ValidationError", Namespace = "http://schemas.datacontract.org/2004/07/Redbox.Ticketing.ControllerServices.Contract.Schema")]
[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
internal class ValidationError : IExtensibleDataObject, INotifyPropertyChanged
{
	[NonSerialized]
	private ExtensionDataObject extensionDataField;

	private Guid ErrorCorrelationIdField;

	private string MessageField;

	private string MessageIdField;

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
	public Guid ErrorCorrelationId
	{
		get
		{
			return ErrorCorrelationIdField;
		}
		set
		{
			if (!ErrorCorrelationIdField.Equals(value))
			{
				ErrorCorrelationIdField = value;
				RaisePropertyChanged("ErrorCorrelationId");
			}
		}
	}

	[DataMember(IsRequired = true)]
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
	public string MessageId
	{
		get
		{
			return MessageIdField;
		}
		set
		{
			if (!object.ReferenceEquals(MessageIdField, value))
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
