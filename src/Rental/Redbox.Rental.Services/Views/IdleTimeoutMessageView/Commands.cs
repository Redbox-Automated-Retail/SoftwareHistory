using System.Windows.Input;

namespace Redbox.Rental.Services.Views.IdleTimeoutMessageView;

public static class Commands
{
	public static readonly RoutedCommand Yes = new RoutedCommand();

	public static readonly RoutedCommand No = new RoutedCommand();
}
