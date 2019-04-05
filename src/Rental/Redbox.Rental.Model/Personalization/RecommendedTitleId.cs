namespace Redbox.Rental.Model.Personalization;

public class RecommendedTitleId : IRecommendedTitleId
{
	public long Id { get; set; }

	public RecommendedTitlesIdType IdType { get; set; }
}
