namespace Redbox.Rental.Kernel.Views.Browse;

public class ProductFamilyMenuButton : MenuButton
{
	public string FilterValue { get; set; }

	public ArrowDirection ArrowDirection { get; set; }

	public ProductFamilyMenuButton()
	{
		base.Height = 74;
		base.Width = 144;
		base.FixedWidth = true;
		base.FontName = "font_akzidenz_bold_smaller";
	}
}
