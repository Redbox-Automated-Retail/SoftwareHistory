using System;
using System.Collections.Generic;
using System.Security;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Personalization;

public class PersonalizationService : IPersonalizationService
{
	private enum LoginType
	{
		Email,
		Phone
	}

	private Action<ILoginResult> _loginResultCallback;

	private Action<IUpdateAccountResult> _updateAccountResultCallback;

	private LoginType _loginType;

	public bool IsEarlyIdEnabled => Redbox.Rental.Services.Configuration.Configuration.Instance?.EarlyIdEnabled ?? false;

	public bool IsLoyaltyEnabled => Redbox.Rental.Services.Configuration.Configuration.Instance?.LoyaltyEnabled ?? false;

	public void LoginWithEmail(string emailAddress, SecureString password, Action<ILoginResult> loginResultCallback)
	{
		Login(emailAddress, password, null, null, loginResultCallback);
	}

	public void LoginWithPhoneNumber(string phoneNumber, SecureString pin, Action<ILoginResult> loginResultCallback)
	{
		Login(null, null, phoneNumber, pin, loginResultCallback);
	}

	public void UpdateAccount(string customerProfileNumber, string emailAddress, string phoneNumber, SecureString pin, bool punchcardOptIn, bool textClubOptIn, Action<IUpdateAccountResult> resultCallback)
	{
		_updateAccountResultCallback = resultCallback;
		IPersonalizationServices service = ServiceLocator.Instance.GetService<IPersonalizationServices>();
		int kioskId = (int)(ServiceLocator.Instance.GetService<IStoreManager>()?.KioskId ?? 0);
		RemoteServiceCallback completeCallback = ProcessUpdateAccountResult;
		service?.UpdateAccount(kioskId, customerProfileNumber, emailAddress, phoneNumber, pin, punchcardOptIn, textClubOptIn, completeCallback);
	}

	private void Login(string emailAddress, SecureString password, string phoneNumber, SecureString pin, Action<ILoginResult> loginResultCallback)
	{
		if (IsEarlyIdEnabled)
		{
			_loginResultCallback = loginResultCallback;
			_loginType = (string.IsNullOrEmpty(emailAddress) ? LoginType.Phone : LoginType.Email);
			IPersonalizationServices service = ServiceLocator.Instance.GetService<IPersonalizationServices>();
			int kioskId = (int)(ServiceLocator.Instance.GetService<IStoreManager>()?.KioskId ?? 0);
			RemoteServiceCallback completeCallback = ProcessLoginResult;
			service?.Login(kioskId, emailAddress, password, phoneNumber, pin, completeCallback);
		}
		else
		{
			LogHelper.Instance.Log("PersonalizationService login rejected.  Early ID is not enabled.");
			LoginResult obj = new LoginResult
			{
				Success = false
			};
			loginResultCallback?.Invoke(obj);
		}
	}

	private void ProcessLoginResult(IRemoteServiceResult result)
	{
		LoginResult loginResult = new LoginResult
		{
			Success = result.Success,
			KioskLoginResponseError = KioskLoginResponseErrors.None
		};
		if (result.Success)
		{
			PopulatePersonalizationSession(result, loginResult);
			(ServiceLocator.Instance.GetService<IShoppingSessionService>()?.GetCurrentSession())?.ShoppingCart?.SetPropertyBagEntry("EarlyIdIndicator", "true");
		}
		else if (result.Properties.ContainsKey("response") && result.Properties["response"] is KioskLoginResponseErrors)
		{
			switch (loginResult.KioskLoginResponseError = (KioskLoginResponseErrors)result.Properties["response"])
			{
			case KioskLoginResponseErrors.AccountLocked:
				loginResult.ErrorMessage = Resources.personalization_service_error_KLI0001;
				break;
			case KioskLoginResponseErrors.InvalidPasswordOrPin:
				if (_loginType == LoginType.Email)
				{
					loginResult.ErrorMessage = Resources.personalization_service_error_KLI0002_email;
				}
				else
				{
					loginResult.ErrorMessage = Resources.personalization_service_error_KLI0002_phone;
				}
				break;
			case KioskLoginResponseErrors.AccountNotFound:
				if (_loginType == LoginType.Email)
				{
					loginResult.ErrorMessage = Resources.personalization_service_error_KLI0003_email;
				}
				else
				{
					loginResult.ErrorMessage = Resources.personalization_service_error_KLI0003_phone;
				}
				break;
			case KioskLoginResponseErrors.CommunicationError:
				loginResult.ErrorMessage = Resources.personalization_service_error_KLI0004;
				break;
			case KioskLoginResponseErrors.LoyaltyIsDown:
				PopulatePersonalizationSession(result, loginResult);
				loginResult.PersonalizationSession.LoyaltySession.SignInError = true;
				loginResult.ErrorMessage = Resources.personalization_service_error_KLI0005;
				break;
			case KioskLoginResponseErrors.LoyaltyAccountNotFound:
				PopulatePersonalizationSession(result, loginResult);
				loginResult.PersonalizationSession.LoyaltySession = null;
				break;
			default:
				loginResult.ErrorMessage = Resources.personalization_service_error_KLI0004;
				break;
			}
		}
		if (_loginResultCallback != null)
		{
			_loginResultCallback(loginResult);
		}
	}

	private void ProcessUpdateAccountResult(IRemoteServiceResult result)
	{
		UpdateAccountResult updateAccountResult = new UpdateAccountResult
		{
			Success = result.Success,
			UpdateAccountResponseError = UpdateAccountResponseErrors.None
		};
		if (!result.Success && result.Properties.ContainsKey("response") && result.Properties["response"] is UpdateAccountResponseErrors)
		{
			switch (updateAccountResult.UpdateAccountResponseError = (UpdateAccountResponseErrors)result.Properties["response"])
			{
			case UpdateAccountResponseErrors.PhoneNumberConflict:
				updateAccountResult.ErrorMessage = Resources.personalization_service_error_UPA0001;
				break;
			case UpdateAccountResponseErrors.CommunicationError:
				updateAccountResult.ErrorMessage = Resources.personalization_service_error_UPA0002;
				break;
			default:
				updateAccountResult.ErrorMessage = Resources.personalization_service_error_UPA0002;
				break;
			}
		}
		_updateAccountResultCallback?.Invoke(updateAccountResult);
	}

	private static int GetInt(object obj)
	{
		int result = 0;
		Type type = obj.GetType();
		if (type == typeof(int))
		{
			result = (int)obj;
		}
		else if (type == typeof(string))
		{
			int.TryParse((string)obj, out result);
		}
		return result;
	}

	private static T GetProperty<T>(IDictionary<string, object> dictionary, string keyName)
	{
		object obj = (dictionary.ContainsKey(keyName) ? dictionary[keyName] : null);
		if (typeof(T) == typeof(string))
		{
			obj = ((obj != null) ? obj.ToString() : string.Empty);
		}
		else if (typeof(T) == typeof(int))
		{
			obj = GetInt(obj);
		}
		return (T)obj;
	}

	private static void PopulatePersonalizationSession(IRemoteServiceResult result, LoginResult loginResult)
	{
		loginResult.PersonalizationSession = new PersonalizationSession();
		loginResult.PersonalizationSession.FirstName = GetProperty<string>(result.Properties, "FirstName");
		loginResult.PersonalizationSession.IsLoggedIn = true;
		loginResult.PersonalizationSession.CustomerProfileNumber = GetProperty<string>(result.Properties, "CustomerProfileNumber");
		loginResult.PersonalizationSession.LoginEmailAddress = GetProperty<string>(result.Properties, "LoginEmail");
		loginResult.PersonalizationSession.LoyaltySession = new LoyaltySession();
		loginResult.PersonalizationSession.LoyaltySession.PointsBalance = GetProperty<int>(result.Properties, "LoyaltyPointBalance");
		LoyaltyTiers result2 = LoyaltyTiers.None;
		string property = GetProperty<string>(result.Properties, "LoyaltyTier");
		if (property != string.Empty)
		{
			Enum.TryParse<LoyaltyTiers>(property, ignoreCase: true, out result2);
		}
		loginResult.PersonalizationSession.LoyaltySession.LoyaltyTier = result2;
		loginResult.PersonalizationSession.LoyaltySession.LoyaltyTierName = property;
		loginResult.PersonalizationSession.LoyaltySession.LoyaltyTierCounter = GetProperty<int>(result.Properties, "LoyaltyTierCounter");
		loginResult.PersonalizationSession.LoyaltySession.PointsExpiring = GetProperty<int>(result.Properties, "PointsExpiring");
		loginResult.PersonalizationSession.LoyaltySession.PointsExpirationDate = GetProperty<DateTime?>(result.Properties, "PointsExpirationDate");
		loginResult.PersonalizationSession.LoyaltySession.BirthDate = GetProperty<DateTime?>(result.Properties, "BirthDate");
		loginResult.PersonalizationSession.LoyaltySession.PostalCode = GetProperty<string>(result.Properties, "PostalCode");
		loginResult.PersonalizationSession.LoyaltySession.PhoneNumber = GetProperty<string>(result.Properties, "PhoneNumber");
	}
}
