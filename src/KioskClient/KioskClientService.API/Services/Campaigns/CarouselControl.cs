namespace KioskClientService.API.Services.Campaigns;

internal class CarouselControl : ViewControl, ICarouselControl, IViewControl
{
	public int? MaxTitles { get; internal set; }

	public CarouselControl()
	{
		base.ControlType = ViewControlType.Carousel;
	}
}
