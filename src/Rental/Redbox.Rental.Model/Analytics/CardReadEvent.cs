namespace Redbox.Rental.Model.Analytics;

public class CardReadEvent : AnalyticsEvent
{
	public bool CardReadValid { get; set; }

	public string Action { get; set; }

	public bool? HasName { get; set; }

	public string CardSourceType { get; set; }

	public string FallbackStatusAction { get; set; }

	public bool? IsInTechnicalFallback { get; set; }

	public bool? NextCardReadIsInTechnicalFallback { get; set; }

	public bool? CardHasChip { get; set; }

	public CardReadEvent()
	{
		base.EventType = "CardRead";
	}
}
