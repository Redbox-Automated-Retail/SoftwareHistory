using System.Windows.Input;

namespace Redbox.Rental.UI;

public static class Commands
{
	public static readonly RoutedCommand RentOrBuyMoviesCommand = new RoutedCommand();

	public static readonly RoutedCommand RentOrBuyGamesCommand = new RoutedCommand();

	public static readonly RoutedCommand BuyMoviesCommand = new RoutedCommand();

	public static readonly RoutedCommand BuyGamesCommand = new RoutedCommand();

	public static readonly RoutedCommand ReturnProductCommand = new RoutedCommand();

	public static readonly RoutedCommand PickupProductCommand = new RoutedCommand();

	public static readonly RoutedCommand ToggleADAModeCommand = new RoutedCommand();

	public static readonly RoutedCommand ToggleLanguageModeCommand = new RoutedCommand();

	public static readonly RoutedCommand HelpCommand = new RoutedCommand();

	public static readonly RoutedCommand ExitBurnInViewCommand = new RoutedCommand();

	public static readonly RoutedCommand BannerClickCommand = new RoutedCommand();

	public static readonly RoutedCommand CollectionCommand = new RoutedCommand();

	public static readonly RoutedCommand MoreInfoCommand = new RoutedCommand();

	public static readonly RoutedCommand BuyCommand = new RoutedCommand();

	public static readonly RoutedCommand PrimaryCommand = new RoutedCommand();

	public static readonly RoutedCommand BluRayCommand = new RoutedCommand();

	public static readonly RoutedCommand BrowseItemSelectedCommand = new RoutedCommand();

	public static readonly RoutedCommand BrowseItemAddCommand = new RoutedCommand();

	public static readonly RoutedCommand BrowseItemCancelCommand = new RoutedCommand();

	public static readonly RoutedCommand AddDVDCommand = new RoutedCommand();

	public static readonly RoutedCommand AddBluRayCommand = new RoutedCommand();

	public static readonly RoutedCommand AddDigitalCodeCommand = new RoutedCommand();

	public static readonly RoutedCommand DigitalCodeInfoCommand = new RoutedCommand();

	public static readonly RoutedCommand CancelTitleRollupOverlayCommand = new RoutedCommand();

	public static readonly RoutedCommand TitleRollupOverlayTimeoutCommand = new RoutedCommand();

	public static readonly RoutedCommand StatisticsPopupCommand = new RoutedCommand();

	public static readonly RoutedCommand CheckOutCommand = new RoutedCommand();

	public static readonly RoutedCommand BrowseFilterCommand = new RoutedCommand();

	public static readonly RoutedCommand BrowseTopMenuCommand = new RoutedCommand();

	public static readonly RoutedCommand ResetIdleTimerCommand = new RoutedCommand();

	public static readonly RoutedCommand PageLeftCommand = new RoutedCommand();

	public static readonly RoutedCommand PageRightCommand = new RoutedCommand();

	public static readonly RoutedCommand BrowseGroupSelectedCommand = new RoutedCommand();

	public static readonly RoutedCommand ToggleTitleFamilyCommand = new RoutedCommand();

	public static readonly RoutedCommand StartOverCommand = new RoutedCommand();

	public static readonly RoutedCommand GoBackCommand = new RoutedCommand();

	public static readonly RoutedCommand MoreCollectionsCommand = new RoutedCommand();

	public static readonly RoutedCommand CancelDialogCommand = new RoutedCommand();
}
