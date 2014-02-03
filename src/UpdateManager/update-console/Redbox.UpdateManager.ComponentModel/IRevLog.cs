using System.Collections.Generic;

namespace Redbox.UpdateManager.ComponentModel;

internal interface IRevLog
{
	string Hash { get; set; }

	List<ChangeItem> Changes { get; set; }

	string Label { get; set; }

	string HashFileName(string name);
}
