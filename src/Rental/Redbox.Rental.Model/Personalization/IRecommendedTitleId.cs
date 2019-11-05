using System.Collections.Generic;

namespace Redbox.Rental.Model.Personalization;

public interface IRecommendedTitleId
{
	long Id { get; set; }

	RecommendedTitlesIdType IdType { get; set; }

	List<string> Tags { get; set; }

	bool IsWishList { get; }
}
