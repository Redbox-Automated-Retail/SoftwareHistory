using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Redbox.TicketServices.Proxy.service;

[Serializable]
[DebuggerStepThrough]
[DataContract(Name = "GetEmailByCardRequest", Namespace = "http://schemas.datacontract.org/2004/07/Redbox.Ticketing.ControllerServices.Contract.Schema.GetEmailByCard")]
[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
internal class GetEmailByCardRequest : Request
{
	private string HashedCreditCardNumberField;

	[DataMember(IsRequired = true)]
	public string HashedCreditCardNumber
	{
		get
		{
			return HashedCreditCardNumberField;
		}
		set
		{
			if (!object.ReferenceEquals(HashedCreditCardNumberField, value))
			{
				HashedCreditCardNumberField = value;
				RaisePropertyChanged("HashedCreditCardNumber");
			}
		}
	}
}
