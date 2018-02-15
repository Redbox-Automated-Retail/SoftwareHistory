using System.Windows.Input;

namespace Redbox.Rental.UI.Views;

public static class SignInViewUserControlCommands
{
	public static readonly RoutedCommand EmailAndPasswordCommand = new RoutedCommand();

	public static readonly RoutedCommand PhoneAndPinCommand = new RoutedCommand();

	public static readonly RoutedCommand CancelCommand = new RoutedCommand();
}
