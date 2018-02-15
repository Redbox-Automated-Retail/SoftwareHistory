using System;
using System.Windows.Input;

namespace Redbox.Rental.UI.ControllersLogic;

public class LogicRoutedCommand : ICommand
{
	private dynamic _doExecute;

	private dynamic _canExecute;

	public event EventHandler CanExecuteChanged
	{
		add
		{
			CommandManager.RequerySuggested += value;
		}
		remove
		{
			CommandManager.RequerySuggested -= value;
		}
	}

	public void initRoutedCommand(dynamic doExecute, dynamic canExecute)
	{
		if (doExecute == null)
		{
			throw new NullReferenceException("The doExecute cannot be null.");
		}
		_doExecute = doExecute;
		_canExecute = canExecute;
	}

	public bool CanExecute(dynamic parameter)
	{
		if (_canExecute == null)
		{
			return true;
		}
		if (!((parameter == null && _canExecute.GetType() == typeof(Func<bool>)) ? true : false))
		{
			return _canExecute.Invoke(parameter);
		}
		return _canExecute.Invoke();
	}

	public void Execute(dynamic parameter)
	{
		if (parameter == null && _doExecute.GetType() == typeof(Action))
		{
			_doExecute.Invoke();
		}
		else
		{
			_doExecute.Invoke(parameter);
		}
	}
}
