namespace KioskClientService.API.Services.Campaigns;

public class ViewControl : IViewControl
{
	public ViewControlType ControlType { get; internal set; }

	public int? DisplayDuration { get; internal set; }
}
