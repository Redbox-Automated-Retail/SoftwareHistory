using System;
using System.Collections.Generic;

namespace Redbox.BrokerServices.Proxy.ComponentModel;

internal delegate IReservationResult ReservationRequestCallback(string cardId, string email, long referenceNumber, DateTime reserveDate, IDictionary<string, List<int>> productsToReserve);
