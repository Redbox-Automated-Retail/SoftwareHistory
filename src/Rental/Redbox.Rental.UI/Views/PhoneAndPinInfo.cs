using System;
using System.Security;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

public class PhoneAndPinInfo
{
	public PhoneAndPinViewModel PhoneAndPinViewModel { get; set; }

	public SignInCancelButtonTypes SignInCancelButtonType { get; set; }

	public Action<string, SecureString> SignInAction { get; set; }
}
