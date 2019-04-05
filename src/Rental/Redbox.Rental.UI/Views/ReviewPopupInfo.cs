using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.UI.Views;

public class ReviewPopupInfo : IViewAnalyticsName
{
	public string ViewAnalyticsName { get; set; }

	public ITitleProduct TitleProduct { get; set; }
}
