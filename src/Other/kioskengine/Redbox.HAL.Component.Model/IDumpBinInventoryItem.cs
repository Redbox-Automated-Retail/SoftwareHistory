using System;

namespace Redbox.HAL.Component.Model;

internal interface IDumpBinInventoryItem
{
	DateTime PutTime { get; }

	string ID { get; }
}
