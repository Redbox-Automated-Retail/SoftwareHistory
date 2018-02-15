using Redbox.KioskServices.Proxy.ComponentModel;

namespace Redbox.Rental.Model.Personalization;

public interface ILoginResult
{
	bool Success { get; set; }

	IPersonalizationSession PersonalizationSession { get; set; }

	KioskLoginResponseErrors KioskLoginResponseError { get; set; }

	string ErrorMessage { get; set; }
}
