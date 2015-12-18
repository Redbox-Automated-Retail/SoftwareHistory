using System;
using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskProduct;

public interface ICollectionProduct : IKioskProduct
{
	long Id { get; }

	string Name { get; }

	int SortOrder { get; }

	List<long> Titles { get; }

	DateTime? EffectiveStartDate { get; }

	DateTime? EffectiveEndDate { get; }

	string LegalInfo { get; }

	bool IsTop20 { get; }

	bool CanShow { get; }
}
