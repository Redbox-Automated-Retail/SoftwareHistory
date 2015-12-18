namespace Redbox.Rental.Kernel.Views.Browse;

public class SizeAndPosition : ISizeAndPosition
{
	public int X { get; set; }

	public int Y { get; set; }

	public int Width { get; set; }

	public int Height { get; set; }

	public float FontSize { get; set; }

	public ISizeAndPosition Clone()
	{
		return new SizeAndPosition
		{
			X = X,
			Y = Y,
			Width = Width,
			Height = Height,
			FontSize = FontSize
		};
	}
}
