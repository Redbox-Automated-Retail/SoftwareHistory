using System.Windows.Input;

namespace Redbox.Rental.Services.Views.HelpView;

public static class Commands
{
	public static readonly RoutedCommand GoBack = new RoutedCommand();

	public static readonly RoutedCommand ShowDocument = new RoutedCommand();

	public static readonly RoutedCommand RedboxLogoCommand = new RoutedCommand();

	public static readonly RoutedCommand PageLeft = new RoutedCommand();

	public static readonly RoutedCommand PageRight = new RoutedCommand();
}
