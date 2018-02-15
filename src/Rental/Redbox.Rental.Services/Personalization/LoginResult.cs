using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Rental.Model.Personalization;

namespace Redbox.Rental.Services.Personalization;

public class LoginResult : ILoginResult
{
	public bool Success { get; set; }

	public IPersonalizationSession PersonalizationSession { get; set; }

	public KioskLoginResponseErrors KioskLoginResponseError { get; set; }

	public string ErrorMessage { get; set; }
}
