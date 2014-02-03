using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Redbox.TicketServices.Proxy.service;

[Serializable]
[DataContract(Name = "Payment", Namespace = "http://schemas.datacontract.org/2004/07/Redbox.Ticketing.ControllerServices.Contract.Schema.PlaceOrder")]
[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
[DebuggerStepThrough]
internal class Payment : IExtensibleDataObject, INotifyPropertyChanged
{
	[NonSerialized]
	private ExtensionDataObject extensionDataField;

	private string EncryptedCardNumberField;

	private int ExpirationMonthField;

	private int ExpirationYearField;

	private string FirstNameField;

	private string HashedCardIdField;

	private string KeyIdField;

	private string LastNameField;

	private string PostalCodeField;

	private string Track2Field;

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
	public string EncryptedCardNumber
	{
		get
		{
			return EncryptedCardNumberField;
		}
		set
		{
			if (!object.ReferenceEquals(EncryptedCardNumberField, value))
			{
				EncryptedCardNumberField = value;
				RaisePropertyChanged("EncryptedCardNumber");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public int ExpirationMonth
	{
		get
		{
			return ExpirationMonthField;
		}
		set
		{
			if (!ExpirationMonthField.Equals(value))
			{
				ExpirationMonthField = value;
				RaisePropertyChanged("ExpirationMonth");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public int ExpirationYear
	{
		get
		{
			return ExpirationYearField;
		}
		set
		{
			if (!ExpirationYearField.Equals(value))
			{
				ExpirationYearField = value;
				RaisePropertyChanged("ExpirationYear");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public string FirstName
	{
		get
		{
			return FirstNameField;
		}
		set
		{
			if (!object.ReferenceEquals(FirstNameField, value))
			{
				FirstNameField = value;
				RaisePropertyChanged("FirstName");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public string HashedCardId
	{
		get
		{
			return HashedCardIdField;
		}
		set
		{
			if (!object.ReferenceEquals(HashedCardIdField, value))
			{
				HashedCardIdField = value;
				RaisePropertyChanged("HashedCardId");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public string KeyId
	{
		get
		{
			return KeyIdField;
		}
		set
		{
			if (!object.ReferenceEquals(KeyIdField, value))
			{
				KeyIdField = value;
				RaisePropertyChanged("KeyId");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public string LastName
	{
		get
		{
			return LastNameField;
		}
		set
		{
			if (!object.ReferenceEquals(LastNameField, value))
			{
				LastNameField = value;
				RaisePropertyChanged("LastName");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public string PostalCode
	{
		get
		{
			return PostalCodeField;
		}
		set
		{
			if (!object.ReferenceEquals(PostalCodeField, value))
			{
				PostalCodeField = value;
				RaisePropertyChanged("PostalCode");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public string Track2
	{
		get
		{
			return Track2Field;
		}
		set
		{
			if (!object.ReferenceEquals(Track2Field, value))
			{
				Track2Field = value;
				RaisePropertyChanged("Track2");
			}
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected void RaisePropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
