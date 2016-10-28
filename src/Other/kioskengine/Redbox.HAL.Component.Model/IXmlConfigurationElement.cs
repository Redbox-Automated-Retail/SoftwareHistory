using System.Xml;

namespace Redbox.HAL.Component.Model;

internal interface IXmlConfigurationElement
{
	void WriteElement(XmlWriter writer);
}
