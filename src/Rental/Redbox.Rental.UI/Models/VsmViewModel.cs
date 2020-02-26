using System.Windows.Media.Imaging;
using Redbox.Rental.Model.Ads;

namespace Redbox.Rental.UI.Models;

public class VsmViewModel : BaseModel<VsmViewModel>
{
	public BitmapImage AdImage { get; set; }

	public IAdImpression AdImpression { get; set; }

	public string TitleText { get; set; }

	public float HeaderHeight { get; set; }

	public int MinimumDisplayTime { get; set; }
}
