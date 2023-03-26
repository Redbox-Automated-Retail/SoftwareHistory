namespace Redbox.Rental.Model.KioskClientService.Campaign;

public interface IViewControl
{
	long? ControlId { get; }

	ViewControlType ControlType { get; }

	int? DisplayDuration { get; }
}
