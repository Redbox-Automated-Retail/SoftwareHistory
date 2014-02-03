using System.CodeDom.Compiler;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Redbox.TicketServices.Proxy.service;

[DebuggerStepThrough]
[GeneratedCode("System.ServiceModel", "4.0.0.0")]
internal class ControllerServiceClient : ClientBase<IControllerService>, IControllerService
{
	public ControllerServiceClient()
	{
	}

	public ControllerServiceClient(string endpointConfigurationName)
		: base(endpointConfigurationName)
	{
	}

	public ControllerServiceClient(string endpointConfigurationName, string remoteAddress)
		: base(endpointConfigurationName, remoteAddress)
	{
	}

	public ControllerServiceClient(string endpointConfigurationName, EndpointAddress remoteAddress)
		: base(endpointConfigurationName, remoteAddress)
	{
	}

	public ControllerServiceClient(Binding binding, EndpointAddress remoteAddress)
		: base(binding, remoteAddress)
	{
	}

	public GetTicketsResponse GetTickets(GetTicketsRequest getTicketsRequest)
	{
		return base.Channel.GetTickets(getTicketsRequest);
	}

	public UpdateTicketLockResponse UpdateTicketLock(UpdateTicketLockRequest updateTicketLockRequest)
	{
		return base.Channel.UpdateTicketLock(updateTicketLockRequest);
	}

	public ReleaseTicketLockResponse ReleaseTicketLock(ReleaseTicketLockRequest releaseTicketLockRequest)
	{
		return base.Channel.ReleaseTicketLock(releaseTicketLockRequest);
	}

	public SendNotificationResponse SendNotification(SendNotificationRequest sendNotificationRequest)
	{
		return base.Channel.SendNotification(sendNotificationRequest);
	}

	public GetEmailByCardResponse GetEmailByCard(GetEmailByCardRequest getEmailByCardRequest)
	{
		return base.Channel.GetEmailByCard(getEmailByCardRequest);
	}

	public PlaceOrderResponse PlaceOrder(PlaceOrderRequest placeOrderRequest)
	{
		return base.Channel.PlaceOrder(placeOrderRequest);
	}
}
