namespace Redbox.Rental.Kernel.Views.Browse;

public interface ISizeAndPosition
{
	int X { get; set; }

	int Y { get; set; }

	int Width { get; set; }

	int Height { get; set; }

	float FontSize { get; set; }

	ISizeAndPosition Clone();
}
