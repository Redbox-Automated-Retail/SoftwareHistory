using System;

namespace Redbox.HAL.Component.Model;

internal interface IDumpBinInventoryItem : IXmlConfigurationElement
{
	DateTime PutTime { get; }

	string ID { get; }
}
