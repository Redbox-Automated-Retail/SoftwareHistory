namespace KioskClientService.API.Services.Campaigns;

public interface IViewControl
{
	ViewControlType ControlType { get; }

	int? DisplayDuration { get; }
}
