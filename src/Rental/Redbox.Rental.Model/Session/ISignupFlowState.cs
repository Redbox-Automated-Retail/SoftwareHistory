namespace Redbox.Rental.Model.Session;

public interface ISignupFlowState
{
	bool MarketingOptInShown { get; set; }

	bool EmailOptInCampaignShown { get; set; }

	bool WasPerksSignupOfferPresented { get; set; }
}
