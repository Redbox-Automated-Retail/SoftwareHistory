using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Redbox.TicketServices.Proxy.service;

[Serializable]
[DataContract(Name = "UpdateTicketLockResponse", Namespace = "http://schemas.datacontract.org/2004/07/Redbox.Ticketing.ControllerServices.Contract.Schema.UpdateTicketLock")]
[DebuggerStepThrough]
[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
internal class UpdateTicketLockResponse : Response
{
	private int LockDurationField;

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
}
