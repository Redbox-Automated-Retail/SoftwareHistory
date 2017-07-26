using System.Windows.Input;

namespace Redbox.Rental.UI;

public static class Commands
{
	public static readonly RoutedCommand RentOrBuyMoviesCommand = new RoutedCommand();

	public static readonly RoutedCommand RentOrBuyGamesCommand = new RoutedCommand();

	public static readonly RoutedCommand BuyMoviesOrGamesCommand = new RoutedCommand();

	public static readonly RoutedCommand ReturnProductCommand = new RoutedCommand();

	public static readonly RoutedCommand PickupProductCommand = new RoutedCommand();

	public static readonly RoutedCommand ToggleADAModeCommand = new RoutedCommand();

	public static readonly RoutedCommand ToggleLanguageModeCommand = new RoutedCommand();

	public static readonly RoutedCommand HelpCommand = new RoutedCommand();

	public static readonly RoutedCommand ExitBurnInViewCommand = new RoutedCommand();

	public static readonly RoutedCommand BannerClickCommand = new RoutedCommand();

	public static readonly RoutedCommand DisplayProductSelectedCommand = new RoutedCommand();

	public static readonly RoutedCommand DisplayProductAddCommand = new RoutedCommand();

	public static readonly RoutedCommand DisplayProductCancelCommand = new RoutedCommand();

	public static readonly RoutedCommand AddDVDCommand = new RoutedCommand();

	public static readonly RoutedCommand AddBluRayCommand = new RoutedCommand();

	public static readonly RoutedCommand CancelTitleRollupOverlayCommand = new RoutedCommand();

	public static readonly RoutedCommand TitleRollupOverlayTimeoutCommand = new RoutedCommand();

	public static readonly RoutedCommand StartOverCommand = new RoutedCommand();
}
