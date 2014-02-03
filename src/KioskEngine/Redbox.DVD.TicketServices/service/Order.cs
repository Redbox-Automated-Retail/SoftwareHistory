using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Redbox.TicketServices.Proxy.service;

[Serializable]
[DebuggerStepThrough]
[DataContract(Name = "Order", Namespace = "http://schemas.datacontract.org/2004/07/Redbox.Ticketing.ControllerServices.Contract.Schema.PlaceOrder")]
[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
internal class Order : IExtensibleDataObject, INotifyPropertyChanged
{
	[NonSerialized]
	private ExtensionDataObject extensionDataField;

	private Customer CustomerField;

	private OrderHeader OrderHeaderField;

	private OrderItem[] OrderItemsField;

	private Payment PaymentField;

	private int[] PromotionListField;

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
	public Customer Customer
	{
		get
		{
			return CustomerField;
		}
		set
		{
			if (!object.ReferenceEquals(CustomerField, value))
			{
				CustomerField = value;
				RaisePropertyChanged("Customer");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public OrderHeader OrderHeader
	{
		get
		{
			return OrderHeaderField;
		}
		set
		{
			if (!object.ReferenceEquals(OrderHeaderField, value))
			{
				OrderHeaderField = value;
				RaisePropertyChanged("OrderHeader");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public OrderItem[] OrderItems
	{
		get
		{
			return OrderItemsField;
		}
		set
		{
			if (!object.ReferenceEquals(OrderItemsField, value))
			{
				OrderItemsField = value;
				RaisePropertyChanged("OrderItems");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public Payment Payment
	{
		get
		{
			return PaymentField;
		}
		set
		{
			if (!object.ReferenceEquals(PaymentField, value))
			{
				PaymentField = value;
				RaisePropertyChanged("Payment");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public int[] PromotionList
	{
		get
		{
			return PromotionListField;
		}
		set
		{
			if (!object.ReferenceEquals(PromotionListField, value))
			{
				PromotionListField = value;
				RaisePropertyChanged("PromotionList");
			}
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected void RaisePropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
