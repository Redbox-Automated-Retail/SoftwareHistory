using System.Collections.Generic;
using System.Text;

namespace Redbox.Rental.Services.KioskClientService.Transactions;

public class LineItemGroup
{
	public TransactionServiceGroupType GroupType { get; set; }

	public List<LineItem> Items { get; set; }

	public Totals Totals { get; set; }

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append($"GroupType: {GroupType}, ");
		if (Items != null)
		{
			stringBuilder.Append(string.Format("Items: [({0})], ", string.Join("), (", Items)));
		}
		stringBuilder.Append($"Totals: ({Totals})");
		return stringBuilder.ToString();
	}
}
