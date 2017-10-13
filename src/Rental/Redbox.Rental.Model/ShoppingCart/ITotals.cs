using System.Collections.Generic;

namespace Redbox.Rental.Model.ShoppingCart;

public interface ITotals
{
	decimal SubTotal { get; }

	decimal TaxAmount { get; }

	decimal GrandTotal { get; }

	List<ISubTotalGroup> SubTotalGroups { get; }

	void UpdateSubTotalGroups();
}
