using System;
using System.Collections.Generic;

namespace Redbox.BrokerServices.Proxy.ComponentModel;

public delegate IReservationResult ReservationRequestCallback(string cardId, string email, long referenceNumber, DateTime reserveDate, IDictionary<string, List<int>> productsToReserve);
