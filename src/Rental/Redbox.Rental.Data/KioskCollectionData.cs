using System;

namespace Redbox.Rental.Data;

public class KioskCollectionData
{
	public long Id { get; set; }

	public string Name { get; set; }

	public long[] Titles { get; set; }

	public DateTime? EffectiveStartDate { get; set; }

	public DateTime? EffectiveEndDate { get; set; }

	public string LegalInfo { get; set; }

	public bool IsTop20 { get; set; }
}
