using System.Threading;
using Redbox.BrokerServices.Proxy.ComponentModel;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Services.KioskBrokerServices.KioskShared.DomainObjects;

namespace Redbox.BrokerServices.Proxy;

public class CancelReservationCallbackEntry : ICallbackEntry
{
	internal readonly AutoResetEvent ResetEvent;

	private long _referenceNumber;

	public string Name => $"CancelReservationCallbackEntry - {_referenceNumber}";

	public CancelReservationResult BrokerResult { get; }

	public CancelReservationCallbackEntry(long referenceNumber)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		ResetEvent = new AutoResetEvent(initialState: false);
		base._002Ector();
		_referenceNumber = referenceNumber;
		BrokerResult = new CancelReservationResult();
	}

	public void Invoke()
	{
		LogHelper.Instance.Log("BrokerServicesProxy received a remote cancel reservation request: {0}", _referenceNumber);
		if (ReservationServicesProxy.Instance.CancelRequestCallback == null)
		{
			BrokerResult.ErrorMessage = "No cancel reservation request callback registered.";
			BrokerResult.CancellationSucceeded = false;
			ResetEvent.Set();
			LogHelper.Instance.Log("CancelReservationCallbackEntry:Invoke() No cancel reservation request callback registered.");
		}
		else
		{
			ILocalCancelReservationResult localCancelReservationResult = ReservationServicesProxy.Instance.CancelRequestCallback(_referenceNumber);
			BrokerResult.CancellationSucceeded = localCancelReservationResult.Success;
			BrokerResult.ErrorMessage = localCancelReservationResult.ErrorMessage;
			LogHelper.Instance.Log("BrokerServicesProxy Cancel reservation response: {0}", localCancelReservationResult.ToJson());
			ResetEvent.Set();
		}
	}
}
