namespace Redbox.Rental.Kernel.Views.Browse;

public class PageBrowseItem
{
	public int RowIndex { get; set; }

	public int ColumnIndex { get; set; }

	public IBrowseItem BrowseItem { get; set; }
}
