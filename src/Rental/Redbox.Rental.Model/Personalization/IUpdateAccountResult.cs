using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Proxy.ComponentModel;

namespace Redbox.Rental.Model.Personalization;

public interface IUpdateAccountResult
{
	bool Success { get; set; }

	string MobilePhoneNumber { get; set; }

	UpdateAccountResponseErrors UpdateAccountResponseError { get; set; }

	string ErrorMessage { get; set; }

	ErrorList RemoteServiceErrors { get; set; }
}
