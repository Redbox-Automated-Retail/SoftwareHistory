using System.Collections.Generic;
using System.Xml;
using Redbox.HAL.Client;
using Redbox.IPC.Framework;

namespace Redbox.HAL.Common.GUI.Functions;

public class ExternalDeviceConfiguration
{
	private List<ExternalDevice> m_devices;

	public HardwareService Service { get; private set; }

	private XmlNode InstalledDevicesNode { get; set; }

	private XmlDocument Document { get; set; }

	public ExternalDevice FindByName(string name)
	{
		if (m_devices == null)
		{
			InitDevices();
		}
		foreach (ExternalDevice device in m_devices)
		{
			if (device.Name == name)
			{
				return device;
			}
		}
		return null;
	}

	public bool AddDevice(ExternalDevice device)
	{
		XmlNode xmlNode = Document.CreateNode(XmlNodeType.Element, "Device", string.Empty);
		InstalledDevicesNode.AppendChild(xmlNode);
		xmlNode.SetAttributeValue("Name", device.Name);
		foreach (ExternalDeviceProperty property in device.Properties)
		{
			XmlNode xmlNode2 = Document.CreateNode(XmlNodeType.Element, "Property", string.Empty);
			xmlNode.AppendChild(xmlNode2);
			xmlNode2.SetAttributeValue("name", property.Key);
			xmlNode2.SetAttributeValue("value", property.Value);
			if (!string.IsNullOrEmpty(property.ValueType))
			{
				xmlNode2.SetAttributeValue("type", property.ValueType);
			}
		}
		return true;
	}

	public bool RemoveDevice(ExternalDevice device)
	{
		XmlNode xmlNode = null;
		foreach (XmlNode childNode in InstalledDevicesNode.ChildNodes)
		{
			string value = childNode.Attributes["Name"].Value;
			if (device.Name.Equals(value))
			{
				xmlNode = childNode;
			}
		}
		if (xmlNode != null)
		{
			InstalledDevicesNode.RemoveChild(xmlNode);
			return true;
		}
		return false;
	}

	public bool SaveConfiguration(bool saveToFile)
	{
		if (saveToFile)
		{
			Document.Save("c:\\config-save.xml");
		}
		return ((ClientCommandResult)(object)Service.SetConfiguration("controller", Document.OuterXml)).Success;
	}

	public ExternalDeviceConfiguration()
	{
		Service = CommonFunctions.ConfigureCommunication("rcp://127.0.0.1:7001");
		InitInner();
	}

	public ExternalDeviceConfiguration(HardwareService service, bool loadDevices)
	{
		Service = service;
		InitInner();
		if (loadDevices)
		{
			InitDevices();
		}
	}

	private bool GetConfigFromHAL()
	{
		HardwareCommandResult configuration = Service.GetConfiguration("controller");
		if (((ClientCommandResult)(object)configuration).Success)
		{
			string xml = ((ClientCommandResult)(object)configuration).CommandMessages[0];
			Document = new XmlDocument();
			Document.LoadXml(xml);
			return true;
		}
		return false;
	}

	private void WriteToOutput(string msg)
	{
	}

	private void InitInner()
	{
		GetConfigFromHAL();
	}

	private void InitDevices()
	{
		m_devices = new List<ExternalDevice>();
		XmlNodeList xmlNodeList = Document.DocumentElement.SelectNodes("property");
		foreach (XmlNode item2 in xmlNodeList)
		{
			XmlNode namedItem = item2.Attributes.GetNamedItem("display-name");
			if (namedItem == null || !namedItem.Value.Equals("Installed Devices"))
			{
				continue;
			}
			InstalledDevicesNode = item2;
			foreach (XmlNode childNode in item2.ChildNodes)
			{
				ExternalDevice externalDevice = new ExternalDevice(childNode.Attributes["Name"].Value);
				externalDevice.IsInstalled = true;
				foreach (XmlNode childNode2 in childNode.ChildNodes)
				{
					if (childNode2.Name.Equals("Property"))
					{
						_ = childNode2.Attributes["name"].Value;
						string value = childNode2.Attributes["value"].Value;
						string valueType = null;
						XmlAttribute xmlAttribute = childNode2.Attributes["type"];
						if (xmlAttribute != null)
						{
							valueType = xmlAttribute.Value;
						}
						ExternalDeviceProperty externalDeviceProperty = new ExternalDeviceProperty();
						externalDeviceProperty.Key = childNode2.Attributes["name"].Value;
						externalDeviceProperty.Value = value;
						externalDeviceProperty.ValueType = valueType;
						ExternalDeviceProperty item = externalDeviceProperty;
						externalDevice.Properties.Add(item);
					}
				}
				m_devices.Add(externalDevice);
			}
		}
		if (InstalledDevicesNode == null)
		{
			InstalledDevicesNode = Document.CreateNode(XmlNodeType.Element, "InstalledDevices", string.Empty);
			Document.DocumentElement.AppendChild(InstalledDevicesNode);
		}
	}
}
