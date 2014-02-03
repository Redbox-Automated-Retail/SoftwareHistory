using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Redbox.TicketServices.Proxy.service;

[Serializable]
[DebuggerStepThrough]
[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
[DataContract(Name = "TicketLockRequest", Namespace = "http://schemas.datacontract.org/2004/07/Redbox.Ticketing.ControllerServices.Contract.Schema")]
[KnownType(typeof(UpdateTicketLockRequest))]
[KnownType(typeof(ReleaseTicketLockRequest))]
internal class TicketLockRequest : Request
{
	private Guid LockGuidField;

	private int QuantityField;

	private int TicketGroupIdField;

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
	public int Quantity
	{
		get
		{
			return QuantityField;
		}
		set
		{
			if (!QuantityField.Equals(value))
			{
				QuantityField = value;
				RaisePropertyChanged("Quantity");
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
}
