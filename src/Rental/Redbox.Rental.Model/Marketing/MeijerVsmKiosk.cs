namespace Redbox.Rental.Model.Marketing;

public class MeijerVsmKiosk
{
	public long KioskId { get; set; }

	public string DateText { get; set; }

	public MeijerVsmNearestKiosk NearestKiosk { get; set; }

	public MeijerVsmKiosk(long kioskId, string dateText, MeijerVsmNearestKiosk nearestKiosk)
	{
		KioskId = kioskId;
		DateText = dateText;
		NearestKiosk = nearestKiosk;
	}
}
