using System.Windows.Input;
using Redbox.Rental.UI.ControllersLogic;

namespace Redbox.Rental.UI;

public class Commands
{
	public static readonly RoutedCommand GeneralClickCommand = new RoutedCommand();

	public static readonly RoutedCommand RentOrBuyMoviesCommand = new RoutedCommand();

	public static readonly RoutedCommand RentOrBuyGamesCommand = new RoutedCommand();

	public static readonly RoutedCommand BuyMoviesCommand = new RoutedCommand();

	public static readonly RoutedCommand BuyGamesCommand = new RoutedCommand();

	public static readonly RoutedCommand ReturnProductCommand = new RoutedCommand();

	public static readonly RoutedCommand PickupProductCommand = new RoutedCommand();

	public static readonly RoutedCommand ToggleADAModeCommand = new RoutedCommand();

	public static readonly RoutedCommand ToggleLanguageModeCommand = new RoutedCommand();

	public static readonly RoutedCommand HelpCommand = new RoutedCommand();

	public static readonly RoutedCommand SignInCommand = new RoutedCommand();

	public static readonly RoutedCommand SignOutCommand = new RoutedCommand();

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

	public static readonly RoutedCommand MyPerksCommand = new RoutedCommand();

	public static readonly RoutedCommand HomeCommand = new RoutedCommand();

	public static readonly RoutedCommand TermsAndPrivacyCommand = new RoutedCommand();

	public static readonly RoutedCommand PromoCodeCommand = new RoutedCommand();

	public static readonly RoutedCommand PayCommand = new RoutedCommand();

	public static readonly RoutedCommand UseCreditsCommand = new RoutedCommand();

	public static readonly RoutedCommand UsePointsCommand = new RoutedCommand();

	public static readonly RoutedCommand AddMovieCommand = new RoutedCommand();

	public static readonly RoutedCommand AddGameCommand = new RoutedCommand();

	public static readonly RoutedCommand UpdateBagCommand = new RoutedCommand();

	public static readonly RoutedCommand Button1Command = new RoutedCommand();

	public static readonly RoutedCommand Button2Command = new RoutedCommand();

	public static readonly RoutedCommand StartOverCommand = new RoutedCommand();

	public static readonly RoutedCommand GoBackCommand = new RoutedCommand();

	public static readonly RoutedCommand MoreCollectionsCommand = new RoutedCommand();

	public static readonly RoutedCommand CancelDialogCommand = new RoutedCommand();

	public static readonly LogicRoutedCommand KeyButtonCommand = new LogicRoutedCommand();

	public static readonly LogicRoutedCommand BackButtonCommand = new LogicRoutedCommand();

	public static readonly LogicRoutedCommand ClearButtonCommand = new LogicRoutedCommand();

	public static readonly LogicRoutedCommand DomainButtonCommand = new LogicRoutedCommand();

	public static readonly LogicRoutedCommand DotComButtonCommand = new LogicRoutedCommand();

	public static readonly LogicRoutedCommand SpaceButtonCommand = new LogicRoutedCommand();

	public static readonly LogicRoutedCommand CancelButtonCommand = new LogicRoutedCommand();

	public static readonly LogicRoutedCommand ContinueButtonCommand = new LogicRoutedCommand();

	public static readonly LogicRoutedCommand CapButtonCommand = new LogicRoutedCommand();

	public static readonly LogicRoutedCommand EmailSelectedCommand = new LogicRoutedCommand();

	public static readonly LogicRoutedCommand SnapshotButtonCommand = new LogicRoutedCommand();

	public static readonly LogicRoutedCommand PerksButtonCommand = new LogicRoutedCommand();

	public static readonly LogicRoutedCommand PromoButtonCommand = new LogicRoutedCommand();

	public static readonly LogicRoutedCommand PhonePinButtonCommand = new LogicRoutedCommand();

	public static readonly LogicRoutedCommand EmailCodeButtonCommand = new LogicRoutedCommand();

	public static readonly LogicRoutedCommand PointsIncludeCommand = new LogicRoutedCommand();

	public static readonly LogicRoutedCommand PointsExcludeCommand = new LogicRoutedCommand();
}
