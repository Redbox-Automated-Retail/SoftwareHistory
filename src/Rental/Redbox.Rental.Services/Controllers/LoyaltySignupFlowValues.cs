using System;
using System.Security;
using Redbox.Rental.Model.Personalization;

namespace Redbox.Rental.Services.Controllers;

public class LoyaltySignupFlowValues : ILoginEmailProvider
{
	public Action ContinueAction { get; set; }

	public Action TimeoutAction { get; set; }

	public bool IsAfterSwipe { get; set; }

	public string EmailAddress { get; set; }

	public string LoginEmailAddress => EmailAddress;

	public string CustomerProfileNumber { get; set; }

	public string MobilePhoneNumber { get; set; }

	public bool PromptForPerks { get; set; }

	public bool PromptForEarlyId { get; set; }

	public bool TextClubEnabled { get; set; }

	public bool SignUpForPerks { get; set; }

	public bool SignUpForEarlyID { get; set; }

	public SecureString Pin { get; set; }
}
