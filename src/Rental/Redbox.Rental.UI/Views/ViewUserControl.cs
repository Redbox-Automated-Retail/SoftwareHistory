using System.Windows;
using System.Windows.Input;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.ControllersLogic;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

public class ViewUserControl : TextToSpeechUserControl, IWPFActor
{
	protected bool NeedKeyboardFocus { get; set; }

	public ViewUserControl()
	{
		base.DataContextChanged += DataContextHasChanged;
	}

	private void DataContextHasChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		DataContextHasChanged();
	}

	protected virtual void DataContextHasChanged()
	{
	}

	protected virtual void VisibleHasChanged()
	{
	}

	protected void InitVisibilityHasChanged()
	{
		base.IsVisibleChanged += delegate
		{
			VisibleHasChanged();
			if (NeedKeyboardFocus)
			{
				base.Focusable = true;
				Keyboard.Focus(this);
			}
		};
	}

	protected void InitKeyboardFocus(KeyEventHandler keyboardAction)
	{
		InitVisibilityHasChanged();
		if (keyboardAction != null)
		{
			base.KeyDown += keyboardAction;
			NeedKeyboardFocus = true;
		}
	}

	public override ISpeechControl GetSpeechControl()
	{
		return ((IBaseModel)base.DataContext)?.ProcessGetSpeechControl();
	}

	protected static void InvokeRoutedCommand(LogicRoutedCommand routeCommand, object parameter = null)
	{
		if (routeCommand.CanExecute(parameter))
		{
			routeCommand.Execute(parameter);
		}
	}

	protected static void InvokeRoutedCommand(RoutedCommand routeCommand, IInputElement target, object parameter = null)
	{
		if (routeCommand.CanExecute(parameter, target))
		{
			routeCommand.Execute(parameter, target);
		}
	}
}
