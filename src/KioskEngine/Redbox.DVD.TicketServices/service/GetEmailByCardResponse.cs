using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Redbox.TicketServices.Proxy.service;

[Serializable]
[DebuggerStepThrough]
[DataContract(Name = "GetEmailByCardResponse", Namespace = "http://schemas.datacontract.org/2004/07/Redbox.Ticketing.ControllerServices.Contract.Schema.GetEmailByCard")]
[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
internal class GetEmailByCardResponse : Response
{
	private string EmailField;

	[DataMember(IsRequired = true)]
	public string Email
	{
		get
		{
			return EmailField;
		}
		set
		{
			if (!object.ReferenceEquals(EmailField, value))
			{
				EmailField = value;
				RaisePropertyChanged("Email");
			}
		}
	}
}
