using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskProduct;

public static class RatingsProperties
{
	private static Dictionary<Ratings, RatingsProperty> _ratingsProperties;

	public static Dictionary<Ratings, RatingsProperty> Properties
	{
		get
		{
			if (_ratingsProperties == null)
			{
				_ratingsProperties = new Dictionary<Ratings, RatingsProperty>
				{
					{
						Ratings.None,
						new RatingsProperty
						{
							Name = "None",
							Code = string.Empty,
							Description = string.Empty
						}
					},
					{
						Ratings.G,
						new RatingsProperty
						{
							Name = "G",
							Code = "G",
							Description = "General Audiences. All Ages Admitted."
						}
					},
					{
						Ratings.PG,
						new RatingsProperty
						{
							Name = "PG",
							Code = "PG",
							Description = "Parental Guidance Suggested. Some Material May Not Be Suitable For Children."
						}
					},
					{
						Ratings.PG13,
						new RatingsProperty
						{
							Name = "PG-13",
							Code = "PG13",
							Description = "Parents Strongly Cautioned. Some Material May Be Inappropriate For Children Under 13."
						}
					},
					{
						Ratings.R,
						new RatingsProperty
						{
							Name = "R",
							Code = "R",
							Description = "Restricted. Children Under 17 Require Accompanying Parent or Adult Guardian."
						}
					},
					{
						Ratings.NR,
						new RatingsProperty
						{
							Name = "NR",
							Code = "NR",
							Description = "Not Rated"
						}
					},
					{
						Ratings.ALLAGES,
						new RatingsProperty
						{
							Name = "ALL AGES",
							Code = "ALLAGES",
							Description = "ALL AGES"
						}
					},
					{
						Ratings.EC,
						new RatingsProperty
						{
							Name = "EC",
							Code = "EC",
							Description = "Early Childhood, 6 and older",
							CultureStringName = "esrb_earlychildhood"
						}
					},
					{
						Ratings.E,
						new RatingsProperty
						{
							Name = "E",
							Code = "E",
							Description = "Everyone",
							CultureStringName = "esrb_everyone"
						}
					},
					{
						Ratings.E10PLUS,
						new RatingsProperty
						{
							Name = "E 10+",
							Code = "E10+",
							Description = "Everyone 10 and older",
							CultureStringName = "esrb_everyone_10_plus"
						}
					},
					{
						Ratings.T,
						new RatingsProperty
						{
							Name = "T",
							Code = "T",
							Description = "Teen, ages 13 and older",
							CultureStringName = "esrb_teen"
						}
					},
					{
						Ratings.M17PLUS,
						new RatingsProperty
						{
							Name = "M (17+)",
							Code = "M",
							Description = "Mature, 17 and older",
							CultureStringName = "esrb_mature"
						}
					},
					{
						Ratings.RP,
						new RatingsProperty
						{
							Name = "RP",
							Code = "RP",
							Description = "Rating Pending",
							CultureStringName = "esrb_rating_pending"
						}
					},
					{
						Ratings.AO,
						new RatingsProperty
						{
							Name = "AO (18+)",
							Code = "AO",
							Description = "Adult Only",
							CultureStringName = "esrb_adults_only"
						}
					},
					{
						Ratings.TVMA,
						new RatingsProperty
						{
							Name = "TVMA",
							Code = "TVMA",
							Description = "Mature Audience Only"
						}
					},
					{
						Ratings.TVPG,
						new RatingsProperty
						{
							Name = "TVPG",
							Code = "TVPG",
							Description = "Parental Guidance Suggested"
						}
					},
					{
						Ratings.TV14,
						new RatingsProperty
						{
							Name = "TV14",
							Code = "TV14",
							Description = "Parents Strongly Cautioned"
						}
					},
					{
						Ratings.TVG,
						new RatingsProperty
						{
							Name = "TVG",
							Code = "TVG",
							Description = "General Audience"
						}
					},
					{
						Ratings.TVY,
						new RatingsProperty
						{
							Name = "TVY",
							Code = "TVY",
							Description = "All Children"
						}
					},
					{
						Ratings.TVY7,
						new RatingsProperty
						{
							Name = "TVY7",
							Code = "TVY7",
							Description = "Directed to Older Children"
						}
					}
				};
			}
			return _ratingsProperties;
		}
	}
}
