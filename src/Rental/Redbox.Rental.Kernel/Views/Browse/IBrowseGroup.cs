namespace Redbox.Rental.Kernel.Views.Browse;

public interface IBrowseGroup
{
	string Name { get; set; }

	int Width { get; set; }

	int Height { get; set; }

	bool Enabled { get; set; }

	bool IsSelected { get; set; }
}
