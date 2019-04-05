namespace Redbox.Rental.Model.Personalization;

public interface IRecommendedTitleId
{
	long Id { get; set; }

	RecommendedTitlesIdType IdType { get; set; }
}
