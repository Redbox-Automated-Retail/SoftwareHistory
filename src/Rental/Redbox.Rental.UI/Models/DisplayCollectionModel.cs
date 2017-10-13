using System.Windows.Media.Imaging;

namespace Redbox.Rental.UI.Models;

public class DisplayCollectionModel : BrowseItemModel
{
	public string Text { get; set; }

	public BitmapImage TopImage { get; set; }

	public BitmapImage MiddleImage { get; set; }

	public BitmapImage BottomImage { get; set; }
}
