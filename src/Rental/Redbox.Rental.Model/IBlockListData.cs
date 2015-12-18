using System;
using System.Collections.Generic;

namespace Redbox.Rental.Model;

public interface IBlockListData
{
	DateTime? GetVersion();

	void CheckForNewFile();

	IEnumerable<BlackList> GetBlackList(string pattern1, string pattern2);
}
