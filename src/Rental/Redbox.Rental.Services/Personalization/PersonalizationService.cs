using System;
using System.Security;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Personalization;

public class PersonalizationService : IPersonalizationService
{
	private Action<IUpdateAccountResult> _updateAccountResultCallback;

	public bool IsEarlyIdEnabled => Redbox.Rental.Services.Configuration.Configuration.Instance?.EarlyIdEnabled ?? false;

	public bool IsLoyaltyEnabled => Redbox.Rental.Services.Configuration.Configuration.Instance?.LoyaltyEnabled ?? false;

	public void SetSelectedPromo(string promoCode)
	{
		IPersonalizationSession personalizationSession = Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession;
		if (personalizationSession != null)
		{
			personalizationSession.SelectedPromo = promoCode;
		}
	}

	public void ClearSelectedPromo()
	{
		IPersonalizationSession personalizationSession = Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession;
		if (personalizationSession != null)
		{
			personalizationSession.SelectedPromo = null;
		}
	}

	public void UpdateAccount(string customerProfileNumber, string emailAddress, string phoneNumber, SecureString pin, bool punchcardOptIn, bool textClubOptIn, Action<IUpdateAccountResult> resultCallback, bool queued = false, AccountSourceType source = AccountSourceType.General)
	{
		_updateAccountResultCallback = resultCallback;
		IPersonalizationServices service = ServiceLocator.Instance.GetService<IPersonalizationServices>();
		int kioskId = (int)(ServiceLocator.Instance.GetService<IStoreManager>()?.KioskId ?? 0);
		RemoteServiceCallback completeCallback = ProcessUpdateAccountResult;
		service?.UpdateAccount(kioskId, customerProfileNumber, emailAddress, phoneNumber, pin, punchcardOptIn, textClubOptIn, completeCallback, queued, source);
	}

	private void ProcessUpdateAccountResult(IRemoteServiceResult result)
	{
		UpdateAccountResult updateAccountResult = new UpdateAccountResult
		{
			Success = result.Success,
			UpdateAccountResponseError = UpdateAccountResponseErrors.None,
			RemoteServiceErrors = result.Errors
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
		else
		{
			updateAccountResult.CustomerProfileNumber = result.GetProperty<string>("CustomerProfileNumber");
			updateAccountResult.TempPassword = result.GetProperty<string>("TempPassword");
		}
		_updateAccountResultCallback?.Invoke(updateAccountResult);
	}
}
