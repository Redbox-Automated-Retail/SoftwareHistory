using System;

namespace Redbox.HAL.Component.Model;

public interface IDumpBinInventoryItem : IXmlConfigurationElement
{
	DateTime PutTime { get; }

	string ID { get; }
}
