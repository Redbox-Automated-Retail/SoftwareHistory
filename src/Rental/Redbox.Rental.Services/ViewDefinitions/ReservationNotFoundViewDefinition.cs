using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "reservation_not_found_view")]
public class ReservationNotFoundViewDefinition : BaseViewDefinition, IViewDefinition
{
	public ReservationNotFoundViewDefinition()
	{
		base.ViewName = "reservation_not_found_view";
		base.ControlName = "ReservationNotFoundView";
		base.OnEnter = delegate
		{
			ReservationNotFoundViewController.ConfigureView();
		};
		base.OnLeave = delegate
		{
			ReservationNotFoundViewController.LeaveView();
		};
	}
}
