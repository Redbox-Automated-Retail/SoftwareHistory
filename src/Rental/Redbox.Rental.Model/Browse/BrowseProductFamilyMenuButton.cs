using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Model.Browse;

public class BrowseProductFamilyMenuButton : BrowseMenuButton
{
	public TitleFamily FilterValue { get; set; }

	public ArrowDirection ArrowDirection { get; set; }

	public BrowseProductFamilyMenuButton()
	{
		base.Height = 74;
		base.Width = 144;
		base.FixedWidth = true;
		base.TextStyleName = "font_montserrat_bold_13";
	}
}
