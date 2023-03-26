namespace Redbox.Rental.Model.KioskClientService.Campaign;

public class ViewControl : IViewControl
{
	public long? ControlId { get; set; }

	public ViewControlType ControlType { get; internal set; }

	public int? DisplayDuration { get; internal set; }
}
