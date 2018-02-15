using Redbox.Rental.Model.Personalization;

namespace Redbox.Rental.Services.Personalization;

public class PersonalizationSession : IPersonalizationSession
{
	public string CustomerProfileNumber { get; set; }

	public string LoginEmailAddress { get; set; }

	public string FirstName { get; set; }

	public bool IsLoggedIn { get; set; }

	public ILoyaltySession LoyaltySession { get; set; }

	public void LogOut()
	{
		IsLoggedIn = false;
		LogOutLoyaltySession();
	}

	public void LogOutLoyaltySession()
	{
		LoyaltySession = null;
	}
}
