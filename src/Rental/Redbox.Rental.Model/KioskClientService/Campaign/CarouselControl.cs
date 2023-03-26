namespace Redbox.Rental.Model.KioskClientService.Campaign;

internal class CarouselControl : ViewControl, ICarouselControl, IViewControl
{
	public int? MaxTitles { get; internal set; }

	public CarouselControl()
	{
		base.ControlType = ViewControlType.Carousel;
	}
}
