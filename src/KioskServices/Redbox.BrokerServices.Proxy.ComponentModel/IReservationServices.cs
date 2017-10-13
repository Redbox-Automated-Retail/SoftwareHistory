using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Redbox.BrokerServices.Proxy.ComponentModel;

public interface IReservationServices
{
	bool CanSwitch();

	void Reset();

	void Pickup(string storeNumber, string emailAddress, long referenceNumber, IDictionary<string, ReadOnlyCollection<IProductItem>> pickupResults, bool isBlocklisted, RemoteServiceCallback callback);

	void ModResvPickup(string storeNumber, string emailAddress, long referenceNumber, string sessionId, bool modifiedCart, RemoteServiceCallback callback);

	void Expire(string storeNumber, long referenceNumber, RemoteServiceCallback callback);

	void Register(string storeNumber, IDictionary<string, IDictionary<string, IDictionary<string, decimal>>> prices, ReservationRequestCallback reservationRequestCallback);

	void Register(string storeNumber, IDictionary<string, IDictionary<string, IDictionary<string, decimal>>> prices, ReservationRequestCallback2 reservationRequestCallback);

	void Register(string storeNumber, IDictionary<string, IDictionary<string, IDictionary<string, decimal>>> prices, ReservationRequestCallback3 reservationRequestCallback);

	void GetReservations(string storeNumber, RemoteServiceCallback completeCallback);

	IProductItem NewProductItem();

	IReservedItem NewReservedItem();

	IReservationResult NewReservationResult();
}
