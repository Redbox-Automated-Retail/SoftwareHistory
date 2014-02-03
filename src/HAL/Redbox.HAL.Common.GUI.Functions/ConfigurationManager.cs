using System;
using System.IO;
using System.Xml;
using Redbox.HAL.Client;
using Redbox.IPC.Framework;

namespace Redbox.HAL.Common.GUI.Functions;

public class ConfigurationManager
{
	private int m_changeCount;

	public bool ConfigurationChanged => m_changeCount > 0;

	public XmlDocument Document => Doc;

	private XmlNode DecksNode { get; set; }

	private XmlDocument Doc { get; set; }

	private HardwareService Service { get; set; }

	public ConfigurationManager(string file)
	{
		LoadConfigurationFromString(File.ReadAllText(file));
	}

	public ConfigurationManager(HardwareService service)
	{
		if (service == null)
		{
			throw new Exception("Need a valid service.");
		}
		Service = service;
		LoadConfiguration();
	}

	public void Save(string path)
	{
		Doc.Save(path);
	}

	public XmlNode ComputeDecksNode()
	{
		return FindDecksNode();
	}

	public XmlNodeList FindAllDeckNodes()
	{
		return FindDecksNode().ChildNodes;
	}

	public XmlNode MakeEmptyDeckNode()
	{
		return Doc.CreateNode(XmlNodeType.Element, "Deck", string.Empty);
	}

	public XmlNode FindDeckNode(int deckNumber)
	{
		foreach (XmlNode childNode in DecksNode.ChildNodes)
		{
			int attributeValue = childNode.GetAttributeValue<int>("Number");
			if (attributeValue == deckNumber)
			{
				return childNode;
			}
		}
		return null;
	}

	public XmlNode FindNamedPropertyNode(string name)
	{
		XmlNodeList xmlNodeList = Doc.DocumentElement.SelectNodes("property");
		foreach (XmlNode item in xmlNodeList)
		{
			XmlNode namedItem = item.Attributes.GetNamedItem("display-name");
			if (namedItem != null && namedItem.Value.Equals(name))
			{
				return item;
			}
		}
		return null;
	}

	public XmlNode MakeDeckNode()
	{
		return Doc.CreateElement("Deck");
	}

	public void RemoveNode(XmlNode node)
	{
		DecksNode.RemoveChild(node);
	}

	public void AddDeckNode(XmlNode node)
	{
		DecksNode.AppendChild(node);
	}

	public bool Refresh()
	{
		try
		{
			LoadConfiguration();
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public bool FlushChanges(bool refresh)
	{
		m_changeCount++;
		if (Service == null)
		{
			return false;
		}
		HardwareCommandResult val = Service.SetConfiguration("controller", Doc.OuterXml);
		if (((ClientCommandResult)(object)val).Success && refresh)
		{
			LoadConfiguration();
		}
		return ((ClientCommandResult)(object)val).Success;
	}

	public void ResetChangeCount()
	{
		m_changeCount = 0;
	}

	private XmlNode FindDecksNode()
	{
		if (DecksNode != null)
		{
			return DecksNode;
		}
		return DecksNode = Doc.DocumentElement.SelectSingleNode("property[@display-name='Decks']");
	}

	private void LoadConfiguration()
	{
		HardwareCommandResult configuration = Service.GetConfiguration("controller");
		if (!((ClientCommandResult)(object)configuration).Success)
		{
			throw new Exception("Unable to get configuration from HAL.");
		}
		LoadConfigurationFromString(((ClientCommandResult)(object)configuration).CommandMessages[0]);
	}

	private void LoadConfigurationFromString(string raw)
	{
		DecksNode = null;
		Doc = new XmlDocument();
		Doc.LoadXml(raw);
		FindDecksNode();
	}
}
