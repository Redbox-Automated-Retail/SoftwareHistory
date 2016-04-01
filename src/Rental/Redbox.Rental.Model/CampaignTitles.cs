using System.Collections.Generic;

namespace Redbox.Rental.Model;

public class CampaignTitles
{
	public bool Include { get; set; }

	public List<long> Titles { get; set; }
}
