using System.Windows;
using Redbox.Rental.UI.ControllersLogic;

namespace Redbox.Rental.UI.Models;

public class PromptSignInModel : BaseModel<PromptSignInModel>
{
	public static readonly DependencyProperty SignInLeftTitleProperty = BaseModel<PromptSignInModel>.CreateDependencyProperty("SignInLeftTitle", TYPES.STRING);

	public static readonly DependencyProperty SignInRightTitleProperty = BaseModel<PromptSignInModel>.CreateDependencyProperty("SignInRightTitle", TYPES.STRING);

	public static readonly DependencyProperty SignInMessageProperty = BaseModel<PromptSignInModel>.CreateDependencyProperty("SignInMessage", TYPES.STRING);

	public static readonly DependencyProperty EmailCodeButtonTextProperty = BaseModel<PromptSignInModel>.CreateDependencyProperty("EmailCodeButtonText", TYPES.STRING);

	public static readonly DependencyProperty PhonePinButtonTextProperty = BaseModel<PromptSignInModel>.CreateDependencyProperty("PhonePinButtonText", TYPES.STRING);

	public static readonly DependencyProperty ContinueButtonTextProperty = BaseModel<PromptSignInModel>.CreateDependencyProperty("ContinueButtonText", TYPES.STRING);

	public PromptSignInLogic Logic { get; set; }

	public LogicRoutedCommand EmailCodeButtonCommand { get; set; }

	public LogicRoutedCommand PhonePinButtonCommand { get; set; }

	public LogicRoutedCommand ContinueButtonCommand { get; set; }

	public string SignInLeftTitle
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(SignInLeftTitleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SignInLeftTitleProperty, value);
			});
		}
	}

	public string SignInRightTitle
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(SignInRightTitleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SignInRightTitleProperty, value);
			});
		}
	}

	public string SignInMessage
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(SignInMessageProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SignInMessageProperty, value);
			});
		}
	}

	public string EmailCodeButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(EmailCodeButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(EmailCodeButtonTextProperty, value);
			});
		}
	}

	public string PhonePinButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(PhonePinButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PhonePinButtonTextProperty, value);
			});
		}
	}

	public string ContinueButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(ContinueButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ContinueButtonTextProperty, value);
			});
		}
	}
}
