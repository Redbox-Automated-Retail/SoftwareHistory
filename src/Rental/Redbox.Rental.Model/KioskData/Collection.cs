using System;
using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskData;

public class Collection
{
	public long Id { get; set; }

	public string Name { get; set; }

	public int SortOrder { get; set; }

	public List<long> Titles { get; set; }

	public DateTime? EffectiveStartDate { get; set; }

	public DateTime? EffectiveEndDate { get; set; }

	public string LegalInfo { get; set; }

	public bool IsTop20 { get; set; }

	public Collection()
	{
		Titles = new List<long>();
	}
}
