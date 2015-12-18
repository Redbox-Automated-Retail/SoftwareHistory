namespace Redbox.Rental.Kernel.Views.Browse;

public interface IBrowseItemConfigurationInfo
{
	IBrowseControl BrowseControl { get; set; }

	ISizeAndPosition SizeAndPosition { get; set; }

	int BrowseItemActorIndex { get; set; }

	int RowIndex { get; set; }

	int ColumnIndex { get; set; }

	bool IsBorderItem { get; set; }

	IBrowseItemConfigurationInfo Clone();
}
