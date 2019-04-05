namespace Redbox.BrokerServices.Proxy.ComponentModel;

public delegate ICancelReservationResult CancelReservationCallback(long referenceNumber, bool cancelOnServer = true);
