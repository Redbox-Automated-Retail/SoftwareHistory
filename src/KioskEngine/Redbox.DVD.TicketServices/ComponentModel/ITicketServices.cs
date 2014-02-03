using System.Collections.Generic;
using Redbox.KioskServices.Proxy.ComponentModel;

namespace Redbox.TicketServices.Proxy.ComponentModel;

internal interface ITicketServices
{
	void Reset();

	void GetTickets(string storeNumber, string productionId, List<object> priceCodeQtyTable, List<object> lockGuidsToRelease, RemoteServiceCallback callback);

	void PlaceOrder(string storeNumber, string sessionId, Dictionary<string, string> customer, Dictionary<string, string> orderHeader, List<object> orderItems, List<object> unlockTickets, int[] promotions, RemoteServiceCallback callback);

	void UpdateTicketLock(string storeNumber, int ticketGroupId, string lockGuid, int quantity, RemoteServiceCallback callback);

	void ReleaseTicketLock(string storeNumber, int ticketGroupId, string lockGuid, int quantity, RemoteServiceCallback callback);

	void GetEmailByCard(string storeNumber, RemoteServiceCallback callback);

	void SendNotification(string storeNumber, List<object> subscriptionTable, string email, RemoteServiceCallback callback);

	bool CanSwitch();
}
