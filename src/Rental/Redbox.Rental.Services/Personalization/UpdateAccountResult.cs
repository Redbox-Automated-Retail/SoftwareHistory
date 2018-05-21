using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Rental.Model.Personalization;

namespace Redbox.Rental.Services.Personalization;

public class UpdateAccountResult : IUpdateAccountResult
{
	public bool Success { get; set; }

	public string MobilePhoneNumber { get; set; }

	public UpdateAccountResponseErrors UpdateAccountResponseError { get; set; }

	public string ErrorMessage { get; set; }
}
