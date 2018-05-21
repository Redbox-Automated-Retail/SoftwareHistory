using System;
using System.Xml;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataMatrix.Framework;

internal sealed class CameraPluginDescriptor
{
	internal string PluginName { get; set; }

	internal string AssemblyName { get; set; }

	internal string PluginNamespace { get; set; }

	internal string PluginServiceProviderMethodName { get; set; }

	internal string Version { get; set; }

	internal string DescriptorPath { get; private set; }

	internal static CameraPluginDescriptor FromXml(string xmlPath)
	{
		try
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(xmlPath);
			if (xmlDocument.DocumentElement == null)
			{
				LogHelper.Instance.Log(LogEntryType.Error, "[Camera Plugin Manager] The descriptor file '{0}' doesn't have a root element.", xmlPath);
				return null;
			}
			XmlNode xmlNode = xmlDocument.SelectSingleNode("Plugin");
			if (xmlNode == null)
			{
				LogHelper.Instance.Log(LogEntryType.Error, "[Camera Plugin Manager] The descriptor file '{0}' is missing the required Plugin node.", xmlPath);
				return null;
			}
			CameraPluginDescriptor cameraPluginDescriptor = new CameraPluginDescriptor();
			cameraPluginDescriptor.DescriptorPath = xmlPath;
			XmlNodeList childNodes = xmlNode.ChildNodes;
			foreach (XmlNode item in childNodes)
			{
				if (item.Name.Equals("PluginType"))
				{
					cameraPluginDescriptor.PluginName = item.Attributes.GetNamedItem("Name").Value;
					cameraPluginDescriptor.Version = item.Attributes.GetNamedItem("Version").Value;
				}
				else if (item.Name.Equals("Assembly"))
				{
					cameraPluginDescriptor.AssemblyName = item.Attributes.GetNamedItem("Name").Value;
				}
				else if (item.Name.Equals("PluginProvider"))
				{
					cameraPluginDescriptor.PluginServiceProviderMethodName = item.Attributes.GetNamedItem("Method").Value;
					cameraPluginDescriptor.PluginNamespace = item.Attributes.GetNamedItem("Namespace").Value;
				}
			}
			return cameraPluginDescriptor;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("[Camera Plugin Manager] Unable to read plugin descriptor.", LogEntryType.Error);
			LogHelper.Instance.Log("[Camera Plugin Manager] Detailed exception: ", e);
			return null;
		}
	}
}
