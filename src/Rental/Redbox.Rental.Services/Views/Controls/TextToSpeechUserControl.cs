using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.Services.Views.Controls;

public class TextToSpeechUserControl : UserControl, ITextToSpeechControl
{
	private Dictionary<string, RoutedCommand> m_routedCommands = new Dictionary<string, RoutedCommand>();

	protected Dictionary<string, RoutedCommand> RoutedCommands => m_routedCommands;

	public virtual string ControlText(string controlName)
	{
		return null;
	}

	public virtual void ExecuteCommand(string commandName, string parameter)
	{
		RoutedCommand value = null;
		if (m_routedCommands.TryGetValue(commandName, out value))
		{
			value.Execute(parameter, this);
		}
	}

	public virtual bool IsControlEnabled(string controlName)
	{
		return GetUIElement(controlName)?.IsEnabled ?? false;
	}

	public virtual bool IsControlVisible(string controlName)
	{
		return GetUIElement(controlName)?.IsVisible ?? false;
	}

	protected void RegisterRoutedCommand(string commandName, RoutedCommand command)
	{
		if (!m_routedCommands.Keys.Contains(commandName))
		{
			m_routedCommands.Add(commandName, command);
		}
	}

	private UIElement GetUIElement(string controlName)
	{
		UIElement result = null;
		object obj = FindName(controlName);
		if (obj != null && obj is UIElement uIElement)
		{
			result = uIElement;
		}
		return result;
	}
}
