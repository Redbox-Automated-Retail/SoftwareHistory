using System.CodeDom.Compiler;
using System.ServiceModel;

namespace Redbox.TicketServices.Proxy.service;

[GeneratedCode("System.ServiceModel", "4.0.0.0")]
[ServiceContract(Namespace = "http://www.redbox.com/ControllerServices/Tickets/2012/04", ConfigurationName = "service.IControllerService")]
internal interface IControllerService
{
	[FaultContract(typeof(ValidationError), Action = "http://www.redbox.com/ControllerServices/Tickets/2012/04/IControllerService/GetTicketsValidationErrorFault", Name = "ValidationError", Namespace = "http://schemas.datacontract.org/2004/07/Redbox.Ticketing.ControllerServices.Contract.Schema")]
	[OperationContract(Action = "http://www.redbox.com/ControllerServices/Tickets/2012/04/IControllerService/GetTickets", ReplyAction = "http://www.redbox.com/ControllerServices/Tickets/2012/04/IControllerService/GetTicketsResponse")]
	[FaultContract(typeof(GeneralError), Action = "http://www.redbox.com/ControllerServices/Tickets/2012/04/IControllerService/GetTicketsGeneralErrorFault", Name = "GeneralError")]
	GetTicketsResponse GetTickets(GetTicketsRequest getTicketsRequest);

	[FaultContract(typeof(GeneralError), Action = "http://www.redbox.com/ControllerServices/Tickets/2012/04/IControllerService/UpdateTicketLockGeneralErrorFault", Name = "GeneralError")]
	[OperationContract(Action = "http://www.redbox.com/ControllerServices/Tickets/2012/04/IControllerService/UpdateTicketLock", ReplyAction = "http://www.redbox.com/ControllerServices/Tickets/2012/04/IControllerService/UpdateTicketLockResponse")]
	[FaultContract(typeof(ValidationError), Action = "http://www.redbox.com/ControllerServices/Tickets/2012/04/IControllerService/UpdateTicketLockValidationErrorFault", Name = "ValidationError", Namespace = "http://schemas.datacontract.org/2004/07/Redbox.Ticketing.ControllerServices.Contract.Schema")]
	UpdateTicketLockResponse UpdateTicketLock(UpdateTicketLockRequest updateTicketLockRequest);

	[OperationContract(Action = "http://www.redbox.com/ControllerServices/Tickets/2012/04/IControllerService/ReleaseTicketLock", ReplyAction = "http://www.redbox.com/ControllerServices/Tickets/2012/04/IControllerService/ReleaseTicketLockResponse")]
	[FaultContract(typeof(ValidationError), Action = "http://www.redbox.com/ControllerServices/Tickets/2012/04/IControllerService/ReleaseTicketLockValidationErrorFault", Name = "ValidationError", Namespace = "http://schemas.datacontract.org/2004/07/Redbox.Ticketing.ControllerServices.Contract.Schema")]
	[FaultContract(typeof(GeneralError), Action = "http://www.redbox.com/ControllerServices/Tickets/2012/04/IControllerService/ReleaseTicketLockGeneralErrorFault", Name = "GeneralError")]
	ReleaseTicketLockResponse ReleaseTicketLock(ReleaseTicketLockRequest releaseTicketLockRequest);

	[FaultContract(typeof(GeneralError), Action = "http://www.redbox.com/ControllerServices/Tickets/2012/04/IControllerService/SendNotificationGeneralErrorFault", Name = "GeneralError")]
	[OperationContract(Action = "http://www.redbox.com/ControllerServices/Tickets/2012/04/IControllerService/SendNotification", ReplyAction = "http://www.redbox.com/ControllerServices/Tickets/2012/04/IControllerService/SendNotificationResponse")]
	[FaultContract(typeof(ValidationError), Action = "http://www.redbox.com/ControllerServices/Tickets/2012/04/IControllerService/SendNotificationValidationErrorFault", Name = "ValidationError", Namespace = "http://schemas.datacontract.org/2004/07/Redbox.Ticketing.ControllerServices.Contract.Schema")]
	SendNotificationResponse SendNotification(SendNotificationRequest sendNotificationRequest);

	[OperationContract(Action = "http://www.redbox.com/ControllerServices/Tickets/2012/04/IControllerService/GetEmailByCard", ReplyAction = "http://www.redbox.com/ControllerServices/Tickets/2012/04/IControllerService/GetEmailByCardResponse")]
	[FaultContract(typeof(ValidationError), Action = "http://www.redbox.com/ControllerServices/Tickets/2012/04/IControllerService/GetEmailByCardValidationErrorFault", Name = "ValidationError", Namespace = "http://schemas.datacontract.org/2004/07/Redbox.Ticketing.ControllerServices.Contract.Schema")]
	[FaultContract(typeof(GeneralError), Action = "http://www.redbox.com/ControllerServices/Tickets/2012/04/IControllerService/GetEmailByCardGeneralErrorFault", Name = "GeneralError")]
	GetEmailByCardResponse GetEmailByCard(GetEmailByCardRequest getEmailByCardRequest);

	[FaultContract(typeof(ValidationError), Action = "http://www.redbox.com/ControllerServices/Tickets/2012/04/IControllerService/PlaceOrderValidationErrorFault", Name = "ValidationError", Namespace = "http://schemas.datacontract.org/2004/07/Redbox.Ticketing.ControllerServices.Contract.Schema")]
	[FaultContract(typeof(GeneralError), Action = "http://www.redbox.com/ControllerServices/Tickets/2012/04/IControllerService/PlaceOrderGeneralErrorFault", Name = "GeneralError")]
	[OperationContract(Action = "http://www.redbox.com/ControllerServices/Tickets/2012/04/IControllerService/PlaceOrder", ReplyAction = "http://www.redbox.com/ControllerServices/Tickets/2012/04/IControllerService/PlaceOrderResponse")]
	PlaceOrderResponse PlaceOrder(PlaceOrderRequest placeOrderRequest);
}
