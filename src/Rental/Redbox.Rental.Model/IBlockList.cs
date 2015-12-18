using System;

namespace Redbox.Rental.Model;

public interface IBlockList
{
	bool IsNameInBlockList(string firstName, string lastName, int market_id, string zipCode);

	void CheckForNewFile();

	DateTime? GetVersion();
}
