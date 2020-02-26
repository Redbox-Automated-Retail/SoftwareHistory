namespace KioskClientService.API.Services.Analytics.KioskDataService;

public class KioskClientEventRequest
{
	public long KioskId { get; set; }

	public string AssemblyVersion { get; set; }

	public KioskClientEvent EventType { get; set; }

	public KioskClientEventRequest(long kioskId, KioskClientEvent eventType)
	{
		KioskId = kioskId;
		AssemblyVersion = GetType().Assembly.GetName().Version.ToString();
		EventType = eventType;
	}
}
