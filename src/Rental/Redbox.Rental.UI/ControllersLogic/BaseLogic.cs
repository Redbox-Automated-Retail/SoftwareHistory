using System;
using System.Security;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.KioskHealth;

namespace Redbox.Rental.UI.ControllersLogic;

public class BaseLogic
{
	protected const string routeCommandError = "The routeCommand can not be null";

	protected LogicRoutedCommand RegisterRoutedCommand(LogicRoutedCommand routeCommand, Action execute, Func<bool> canExecute = null)
	{
		if (routeCommand == null)
		{
			throw new NullReferenceException("The routeCommand can not be null");
		}
		routeCommand.initRoutedCommand(execute, canExecute);
		return routeCommand;
	}

	protected LogicRoutedCommand RegisterRoutedCommand(LogicRoutedCommand routeCommand, Action<int> execute, Predicate<int> canExecute = null)
	{
		if (routeCommand == null)
		{
			throw new NullReferenceException("The routeCommand can not be null");
		}
		routeCommand.initRoutedCommand(execute, canExecute);
		return routeCommand;
	}

	protected LogicRoutedCommand RegisterRoutedCommand(LogicRoutedCommand routeCommand, Action<bool> execute, Predicate<bool> canExecute = null)
	{
		if (routeCommand == null)
		{
			throw new NullReferenceException("The routeCommand can not be null");
		}
		routeCommand.initRoutedCommand(execute, canExecute);
		return routeCommand;
	}

	protected LogicRoutedCommand RegisterRoutedCommand(LogicRoutedCommand routeCommand, Action<double> execute, Predicate<double> canExecute = null)
	{
		if (routeCommand == null)
		{
			throw new NullReferenceException("The routeCommand can not be null");
		}
		routeCommand.initRoutedCommand(execute, canExecute);
		return routeCommand;
	}

	protected LogicRoutedCommand RegisterRoutedCommand(LogicRoutedCommand routeCommand, Action<string> execute, Predicate<string> canExecute = null)
	{
		if (routeCommand == null)
		{
			throw new NullReferenceException("The routeCommand can not be null");
		}
		routeCommand.initRoutedCommand(execute, canExecute);
		return routeCommand;
	}

	protected LogicRoutedCommand RegisterRoutedCommand(LogicRoutedCommand routeCommand, Action<object> execute, Predicate<object> canExecute = null)
	{
		if (routeCommand == null)
		{
			throw new NullReferenceException("The routeCommand can not be null");
		}
		routeCommand.initRoutedCommand(execute, canExecute);
		return routeCommand;
	}

	protected LogicRoutedCommand RegisterRoutedCommand<T>(LogicRoutedCommand routeCommand, Action<T> execute, Predicate<object> canExecute = null)
	{
		if (routeCommand == null)
		{
			throw new NullReferenceException("The routeCommand can not be null");
		}
		routeCommand.initRoutedCommand(execute, canExecute);
		return routeCommand;
	}

	public static SecureString CreateSecureString(string source)
	{
		if (string.IsNullOrWhiteSpace(source))
		{
			return null;
		}
		SecureString secureString = new SecureString();
		char[] array = source.ToCharArray();
		foreach (char c in array)
		{
			secureString.AppendChar(c);
		}
		return secureString;
	}

	protected static void PostTouchScreenHealthActivity()
	{
		ServiceLocator.Instance.GetService<ITouchScreenHealth>()?.PostActivity();
	}

	public void HandleWPFHit()
	{
		ServiceLocator.Instance.GetService<IRenderingService>()?.ActiveScene.PrcoessWPFHit();
		PostTouchScreenHealthActivity();
	}
}
