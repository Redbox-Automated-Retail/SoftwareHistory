using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Redbox.TicketServices.Proxy.service;

[Serializable]
[DebuggerStepThrough]
[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
[DataContract(Name = "TicketGroup", Namespace = "http://schemas.datacontract.org/2004/07/Redbox.Ticketing.ControllerServices.Contract.Schema.GetTickets")]
internal class TicketGroup : IExtensibleDataObject, INotifyPropertyChanged
{
	[NonSerialized]
	private ExtensionDataObject extensionDataField;

	private DeliveryMethod[] DeliveryMethodsField;

	private int DisplayOrderField;

	private string LevelField;

	private int LockDurationField;

	private Guid LockGuidField;

	private PriceCodeResponse[] PriceCodesField;

	private string RowField;

	private string SectionField;

	private string SuggestionTypeDisplayNameField;

	private int SuggestionTypeIdField;

	private int SupplierIdField;

	private int? SupplierNoteLogIdField;

	private int TicketGroupIdField;

	private string ZoneField;

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
	public DeliveryMethod[] DeliveryMethods
	{
		get
		{
			return DeliveryMethodsField;
		}
		set
		{
			if (!object.ReferenceEquals(DeliveryMethodsField, value))
			{
				DeliveryMethodsField = value;
				RaisePropertyChanged("DeliveryMethods");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public int DisplayOrder
	{
		get
		{
			return DisplayOrderField;
		}
		set
		{
			if (!DisplayOrderField.Equals(value))
			{
				DisplayOrderField = value;
				RaisePropertyChanged("DisplayOrder");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public string Level
	{
		get
		{
			return LevelField;
		}
		set
		{
			if (!object.ReferenceEquals(LevelField, value))
			{
				LevelField = value;
				RaisePropertyChanged("Level");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public int LockDuration
	{
		get
		{
			return LockDurationField;
		}
		set
		{
			if (!LockDurationField.Equals(value))
			{
				LockDurationField = value;
				RaisePropertyChanged("LockDuration");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public Guid LockGuid
	{
		get
		{
			return LockGuidField;
		}
		set
		{
			if (!LockGuidField.Equals(value))
			{
				LockGuidField = value;
				RaisePropertyChanged("LockGuid");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public PriceCodeResponse[] PriceCodes
	{
		get
		{
			return PriceCodesField;
		}
		set
		{
			if (!object.ReferenceEquals(PriceCodesField, value))
			{
				PriceCodesField = value;
				RaisePropertyChanged("PriceCodes");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public string Row
	{
		get
		{
			return RowField;
		}
		set
		{
			if (!object.ReferenceEquals(RowField, value))
			{
				RowField = value;
				RaisePropertyChanged("Row");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public string Section
	{
		get
		{
			return SectionField;
		}
		set
		{
			if (!object.ReferenceEquals(SectionField, value))
			{
				SectionField = value;
				RaisePropertyChanged("Section");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public string SuggestionTypeDisplayName
	{
		get
		{
			return SuggestionTypeDisplayNameField;
		}
		set
		{
			if (!object.ReferenceEquals(SuggestionTypeDisplayNameField, value))
			{
				SuggestionTypeDisplayNameField = value;
				RaisePropertyChanged("SuggestionTypeDisplayName");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public int SuggestionTypeId
	{
		get
		{
			return SuggestionTypeIdField;
		}
		set
		{
			if (!SuggestionTypeIdField.Equals(value))
			{
				SuggestionTypeIdField = value;
				RaisePropertyChanged("SuggestionTypeId");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public int SupplierId
	{
		get
		{
			return SupplierIdField;
		}
		set
		{
			if (!SupplierIdField.Equals(value))
			{
				SupplierIdField = value;
				RaisePropertyChanged("SupplierId");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public int? SupplierNoteLogId
	{
		get
		{
			return SupplierNoteLogIdField;
		}
		set
		{
			if (!SupplierNoteLogIdField.Equals(value))
			{
				SupplierNoteLogIdField = value;
				RaisePropertyChanged("SupplierNoteLogId");
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

	[DataMember(IsRequired = true)]
	public string Zone
	{
		get
		{
			return ZoneField;
		}
		set
		{
			if (!object.ReferenceEquals(ZoneField, value))
			{
				ZoneField = value;
				RaisePropertyChanged("Zone");
			}
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected void RaisePropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
