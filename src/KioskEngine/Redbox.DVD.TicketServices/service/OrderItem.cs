using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Redbox.TicketServices.Proxy.service;

[Serializable]
[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
[DebuggerStepThrough]
[DataContract(Name = "OrderItem", Namespace = "http://schemas.datacontract.org/2004/07/Redbox.Ticketing.ControllerServices.Contract.Schema.PlaceOrder")]
internal class OrderItem : IExtensibleDataObject, INotifyPropertyChanged
{
	[NonSerialized]
	private ExtensionDataObject extensionDataField;

	private string DeliverToNameField;

	private int DeliveryMethodIdField;

	private Guid LockGuidField;

	private OrderPriceCode[] PriceCodesField;

	private int SuggestionTypeIdField;

	private int? SupplierNoteLogIdField;

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
	public string DeliverToName
	{
		get
		{
			return DeliverToNameField;
		}
		set
		{
			if (!object.ReferenceEquals(DeliverToNameField, value))
			{
				DeliverToNameField = value;
				RaisePropertyChanged("DeliverToName");
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
	public OrderPriceCode[] PriceCodes
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

	public event PropertyChangedEventHandler PropertyChanged;

	protected void RaisePropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
