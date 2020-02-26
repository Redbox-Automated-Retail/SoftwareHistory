namespace Redbox.Rental.UI.Views;

public static class SwipeViewTypeExtensions
{
	public static bool IsReservation(this SwipeViewType source)
	{
		if (source != SwipeViewType.ReservationPickup && source != SwipeViewType.EncryptedSwipeReservationPickup && source != SwipeViewType.EMVInsertTapReservationPickup && source != SwipeViewType.EMVInsertReservationPickup)
		{
			return source == SwipeViewType.EMVTapReservationPickup;
		}
		return true;
	}
}
