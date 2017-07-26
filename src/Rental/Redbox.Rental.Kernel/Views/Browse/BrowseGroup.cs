namespace Redbox.Rental.Kernel.Views.Browse;

public class BrowseGroup : IBrowseGroup
{
	public string Name { get; set; }

	public int Width { get; set; }

	public int Height { get; set; }

	public bool Enabled { get; set; }

	public bool IsSelected { get; set; }
}
