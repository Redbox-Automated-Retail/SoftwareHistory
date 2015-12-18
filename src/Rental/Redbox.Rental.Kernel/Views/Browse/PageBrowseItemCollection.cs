using System.Collections.Generic;

namespace Redbox.Rental.Kernel.Views.Browse;

public class PageBrowseItemCollection
{
	private Dictionary<int, Dictionary<int, IBrowseItem>> _dictionaryCollection = new Dictionary<int, Dictionary<int, IBrowseItem>>();

	public void Add(int row, int column, IBrowseItem pageBrowseItem)
	{
		Dictionary<int, IBrowseItem> value = null;
		if (!_dictionaryCollection.TryGetValue(row, out value))
		{
			value = new Dictionary<int, IBrowseItem>();
			_dictionaryCollection.Add(row, value);
		}
		value[column] = pageBrowseItem;
	}

	public IBrowseItem Get(int row, int column)
	{
		IBrowseItem value = null;
		Dictionary<int, IBrowseItem> value2 = null;
		if (_dictionaryCollection.TryGetValue(row, out value2))
		{
			value2.TryGetValue(column, out value);
		}
		return value;
	}
}
