namespace Redbox.Rental.Kernel.Views.Browse;

public class BrowseItemConfigurationInfo : IBrowseItemConfigurationInfo
{
	public IBrowseControl BrowseControl { get; set; }

	public ISizeAndPosition SizeAndPosition { get; set; }

	public int BrowseItemActorIndex { get; set; }

	public int RowIndex { get; set; }

	public int ColumnIndex { get; set; }

	public bool IsBorderItem { get; set; }

	public IBrowseItemConfigurationInfo Clone()
	{
		return new BrowseItemConfigurationInfo
		{
			BrowseControl = BrowseControl,
			SizeAndPosition = SizeAndPosition.Clone(),
			BrowseItemActorIndex = BrowseItemActorIndex,
			ColumnIndex = ColumnIndex,
			RowIndex = RowIndex,
			IsBorderItem = IsBorderItem
		};
	}
}
