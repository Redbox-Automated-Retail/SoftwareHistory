using System;

namespace Redbox.Rental.Model;

public interface IRebalance
{
	DateTime? GetRebalanceVersion();

	void CheckNewRebalance();
}
