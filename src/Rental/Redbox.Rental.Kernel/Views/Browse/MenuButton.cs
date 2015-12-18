namespace Redbox.Rental.Kernel.Views.Browse;

public class MenuButton
{
	private string _fontName = "font_akzidenz_bold_tiny";

	private int _y;

	private int _height = 69;

	public string Name { get; set; }

	public string Text { get; set; }

	public int Width { get; set; }

	public int Height
	{
		get
		{
			return _height;
		}
		set
		{
			_height = value;
		}
	}

	public int X { get; set; }

	public int Y
	{
		get
		{
			return _y;
		}
		set
		{
			_y = value;
		}
	}

	public bool IsSelected { get; set; }

	public bool FixedWidth { get; set; }

	public bool FixedPosition { get; set; }

	public string FontName
	{
		get
		{
			return _fontName;
		}
		set
		{
			_fontName = value;
		}
	}
}
