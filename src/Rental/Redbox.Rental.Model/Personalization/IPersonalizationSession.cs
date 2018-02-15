namespace Redbox.Rental.Model.Personalization;

public interface IPersonalizationSession
{
	string CustomerProfileNumber { get; set; }

	string LoginEmailAddress { get; set; }

	string FirstName { get; set; }

	bool IsLoggedIn { get; set; }

	ILoyaltySession LoyaltySession { get; set; }

	void LogOut();

	void LogOutLoyaltySession();
}
