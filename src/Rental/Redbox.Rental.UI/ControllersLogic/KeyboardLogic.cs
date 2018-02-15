using System;
using System.Security;
using System.Windows.Input;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.UI.ControllersLogic;

public class KeyboardLogic : BaseLogic
{
	public KeyboardModel Model { get; set; }

	public KeyboardInfo KeyboardInfo { get; set; }

	public bool Instructions { get; set; }

	public int KeyboardTextMaxLength { get; set; }

	public Action CancelAction { get; set; }

	public Action<string> ContinueAction { get; set; }

	public Action<string, SecureString> ContinueCodeAction { get; set; }

	public Predicate<string> IsValidEntry { get; set; }

	public Func<string, string, bool> IsValidEntries { get; set; }

	public static Action StopIdleTimerAction { get; set; }

	public KeyboardMode KeyboardMode { get; set; }

	public KeyboardLogic(KeyboardModel model)
	{
		Model = model;
		Model.KeyboardError = "";
		Model.KeyButtonCommand = RegisterRoutedCommand(Commands.KeyButtonCommand, KeyButtonCommand_Execute);
		Model.BackButtonCommand = RegisterRoutedCommand(Commands.BackButtonCommand, BackButtonCommand_Execute);
		Model.ClearButtonCommand = RegisterRoutedCommand(Commands.ClearButtonCommand, ClearButtonCommand_Execute);
		Model.DomainButtonCommand = RegisterRoutedCommand(Commands.DomainButtonCommand, DomainButtonCommand_Execute);
		Model.DotComButtonCommand = RegisterRoutedCommand(Commands.DotComButtonCommand, DotComButtonCommand_Execute);
		Model.SpaceButtonCommand = RegisterRoutedCommand(Commands.SpaceButtonCommand, SpaceButtonCommand_Execute);
		Model.CancelButtonCommand = RegisterRoutedCommand(Commands.CancelButtonCommand, CancelButtonCommand_Execute);
		Model.ContinueButtonCommand = RegisterRoutedCommand(Commands.ContinueButtonCommand, ContinueButtonCommand_Execute);
		Model.EmailSelectedCommand = RegisterRoutedCommand(Commands.EmailSelectedCommand, EmailSelectedCommand_Execute);
	}

	public bool InitControlTextMode()
	{
		bool keyEmailMode = Model.KeyEmailMode;
		Model.SetControlTextMode?.Invoke(keyEmailMode);
		return keyEmailMode;
	}

	public void SetControlTextMode(bool mode)
	{
		Model.SetControlTextMode?.Invoke(mode);
	}

	public bool GetControlTextMode()
	{
		return Model.KeyEmailMode;
	}

	private string GetErrorMessage()
	{
		switch (KeyboardInfo.KeyboardMode)
		{
		case KeyboardMode.EMAIL_COMING_SOON:
		case KeyboardMode.ACCOUNT_SIGN_IN:
			return Resources.email_error_message;
		case KeyboardMode.EMAIL_RECEIPTS_PROMO:
			return Resources.receipt_email_error;
		case KeyboardMode.PROMOTIONS_CODE:
			return Resources.promo_code_error_message;
		default:
			return "";
		}
	}

	public void CancelButton_Execute()
	{
		if (CancelAction != null)
		{
			CancelAction();
			Model.KeyboardText = "";
		}
	}

	public bool ValidateKeyboardText()
	{
		Model.KeyboardText = Model.KeyboardText ?? "";
		Model.KeyboardCode = Model.KeyboardCode ?? "";
		Model.KeyboardError = "";
		bool flag;
		switch (KeyboardMode)
		{
		case KeyboardMode.ACCOUNT_SIGN_IN:
			flag = IsValidEntries != null && IsValidEntries(Model.KeyboardText, "\a\f\v");
			if (!flag)
			{
				Model.KeyboardError = GetErrorMessage();
			}
			break;
		case KeyboardMode.ACCOUNT_EMAIL_CHANGED:
			flag = IsValidEntries != null && IsValidEntries(Model.KeyboardText, Model.KeyboardCode);
			if (!flag)
			{
				Model.KeyboardError = GetErrorMessage();
			}
			break;
		default:
			flag = IsValidEntry != null && IsValidEntry(Model.KeyboardText);
			if (!flag)
			{
				Model.KeyboardError = GetErrorMessage();
			}
			break;
		}
		return flag;
	}

	public string ContinueButtonExecute()
	{
		if (ValidateKeyboardText())
		{
			if (KeyboardMode == KeyboardMode.ACCOUNT_SIGN_IN)
			{
				if (ContinueCodeAction != null)
				{
					StopIdleTimerAction?.Invoke();
					ContinueCodeAction(Model.KeyboardText, BaseLogic.CreateSecureString(Model.KeyboardCode));
					Model.KeyboardText = string.Empty;
					Model.KeyboardCode = string.Empty;
				}
			}
			else if (ContinueAction != null)
			{
				StopIdleTimerAction?.Invoke();
				ContinueAction(Model.KeyboardText);
			}
		}
		return Model.KeyboardError;
	}

	private void UpdateContinue(bool clearError = true)
	{
		if (Model.KeyEmailMode)
		{
			if ((Model.KeyboardText?.Length ?? 0) > KeyboardTextMaxLength)
			{
				Model.KeyboardText = Model.KeyboardText.Remove(KeyboardTextMaxLength);
			}
		}
		else if ((Model.KeyboardCode?.Length ?? 0) > KeyboardTextMaxLength)
		{
			Model.KeyboardCode = Model.KeyboardCode.Remove(KeyboardTextMaxLength);
		}
		if (clearError)
		{
			Model.KeyboardError = string.Empty;
		}
		HandleWPFHit();
		CommandManager.InvalidateRequerySuggested();
	}

	public void KeyButtonCommand_Execute(string text)
	{
		if (Model.KeyEmailMode)
		{
			Model.KeyboardText += text;
		}
		else
		{
			Model.KeyboardCode += text;
		}
		UpdateContinue();
	}

	public void BackButtonCommand_Execute()
	{
		if (Model.KeyEmailMode)
		{
			int num = Model.KeyboardText.Length - 1;
			if (num > -1)
			{
				Model.KeyboardText = Model.KeyboardText.Remove(num);
			}
		}
		else
		{
			int num = Model.KeyboardCode.Length - 1;
			if (num > -1)
			{
				Model.KeyboardCode = Model.KeyboardCode.Remove(num);
			}
		}
		UpdateContinue();
	}

	public void ClearButtonCommand_Execute()
	{
		if (Model.KeyEmailMode)
		{
			Model.KeyboardText = string.Empty;
		}
		else
		{
			Model.KeyboardCode = string.Empty;
		}
		UpdateContinue();
	}

	public void DomainButtonCommand_Execute(string text)
	{
		int num = Model.KeyboardText.IndexOf('@');
		if (num > -1)
		{
			Model.KeyboardText = Model.KeyboardText.Remove(num);
		}
		if (Model.KeyboardText.Length > 0)
		{
			Model.KeyboardText += text;
		}
		UpdateContinue();
	}

	public void DotComButtonCommand_Execute(string text)
	{
		int num = Model.KeyboardText.IndexOf('@');
		if (num == -1)
		{
			Model.KeyboardError = Resources.email_error_message;
		}
		string text2 = Model.KeyboardText.Substring(num + 1);
		if (text2.Length == 0)
		{
			Model.KeyboardError = Resources.email_error_message;
		}
		else
		{
			int num2 = text2.IndexOf('.') + 1;
			if (num2 > 0)
			{
				Model.KeyboardText = Model.KeyboardText.Remove(num + num2);
			}
			Model.KeyboardText += text;
		}
		UpdateContinue();
	}

	public void SpaceButtonCommand_Execute()
	{
		Model.KeyboardText += " ";
		UpdateContinue();
	}

	public void CancelButtonCommand_Execute()
	{
		CancelButton_Execute();
		HandleWPFHit();
	}

	public void ContinueButtonCommand_Execute()
	{
		Model.KeyboardError = ContinueButtonExecute();
		if (!string.IsNullOrEmpty(Model.KeyboardError))
		{
			UpdateContinue(clearError: false);
		}
		else
		{
			Model.KeyboardText = "";
		}
	}

	public void EmailSelectedCommand_Execute(bool emailMode)
	{
		Model.KeyEmailMode = emailMode;
	}
}
