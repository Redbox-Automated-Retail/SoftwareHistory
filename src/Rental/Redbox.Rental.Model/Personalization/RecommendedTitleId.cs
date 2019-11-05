using System;
using System.Collections.Generic;
using System.Linq;

namespace Redbox.Rental.Model.Personalization;

public class RecommendedTitleId : IRecommendedTitleId
{
	private List<string> _tags;

	public long Id { get; set; }

	public RecommendedTitlesIdType IdType { get; set; }

	public bool IsWishList { get; private set; }

	public List<string> Tags
	{
		get
		{
			return _tags;
		}
		set
		{
			_tags = value;
			IsWishList = _tags.Any((string x) => x.Equals("wishlist", StringComparison.CurrentCultureIgnoreCase));
		}
	}
}
