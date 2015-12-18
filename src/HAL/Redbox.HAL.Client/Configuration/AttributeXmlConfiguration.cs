using System;
using System.Reflection;
using System.Xml;
using Redbox.HAL.Core;

namespace Redbox.HAL.Configuration;

internal abstract class AttributeXmlConfiguration : AbstractXmlConfiguration
{
	private readonly Type ThisType;

	protected abstract string FileNodeName { get; }

	protected AttributeXmlConfiguration(string xmlRoot, Type type)
		: base(xmlRoot)
	{
		ThisType = type;
		using ExecutionTimer executionTimer = new ExecutionTimer();
		PropertyInfo[] properties = ThisType.GetProperties();
		PropertyInfo[] array = properties;
		foreach (PropertyInfo propertyInfo in array)
		{
			if (Attribute.GetCustomAttribute(propertyInfo, typeof(XmlConfigurationAttribute)) is XmlConfigurationAttribute xmlConfigurationAttribute)
			{
				try
				{
					object value = ConversionHelper.ChangeType(xmlConfigurationAttribute.DefaultValue, propertyInfo.PropertyType);
					propertyInfo.SetValue(this, value, null);
				}
				catch (Exception e)
				{
					LogHelper.Instance.Log($"[AttributeXmlConfiguration] Ctor caught an exception processing property {propertyInfo.Name}.", e);
				}
			}
		}
		executionTimer.Stop();
		LogHelper.Instance.Log("[AttributeXmlConfiguration] Time to set defaults on type {0}: {1}ms", type.Name, executionTimer.ElapsedMilliseconds);
	}

	protected override void LoadProperties(XmlDocument document, ErrorList errors)
	{
		using (ExecutionTimer executionTimer = new ExecutionTimer())
		{
			XmlNode xmlNode = document.DocumentElement.SelectSingleNode(FileNodeName);
			foreach (XmlNode childNode in xmlNode.ChildNodes)
			{
				string name = childNode.Name;
				try
				{
					PropertyInfo property = ThisType.GetProperty(name);
					if (property == null)
					{
						if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
						{
							LogHelper.Instance.Log("[AttributeXmlConfiguration] Unable to find property '{0}'", name);
						}
					}
					else if (Attribute.GetCustomAttribute(property, typeof(XmlConfigurationAttribute)) is XmlConfigurationAttribute)
					{
						object value = ConversionHelper.ChangeType(childNode.InnerText, property.PropertyType);
						property.SetValue(this, value, null);
					}
				}
				catch (Exception e)
				{
					LogHelper.Instance.Log("[AbstractXmlConfiguration] LoadProperties caught an exception.", e);
				}
			}
			LogHelper.Instance.Log("[AbstractXmlConfiguration] Time to load defaults on type {0}: {1}ms", ThisType.Name, executionTimer.ElapsedMilliseconds);
			executionTimer.Stop();
		}
		LoadPropertiesInner(document, errors);
	}

	protected override void StoreProperties(XmlDocument document, ErrorList errors)
	{
		using (ExecutionTimer executionTimer = new ExecutionTimer())
		{
			PropertyInfo[] properties = ThisType.GetProperties();
			PropertyInfo[] array = properties;
			foreach (PropertyInfo propertyInfo in array)
			{
				if (Attribute.GetCustomAttribute(propertyInfo, typeof(XmlConfigurationAttribute)) is XmlConfigurationAttribute)
				{
					try
					{
						string value = propertyInfo.GetValue(this, null).ToString();
						SetSingleNodeValue(document.DocumentElement, $"{FileNodeName}/{propertyInfo.Name}", value);
					}
					catch (Exception e)
					{
						LogHelper.Instance.Log($"[AbstractXmlConfiguration] StoreProperties caught an exception processing property {propertyInfo.Name}", e);
					}
				}
			}
			executionTimer.Stop();
			LogHelper.Instance.Log("[AbstractXmlConfiguration] Time to save defaults on type {0}: {1}ms", ThisType.Name, executionTimer.ElapsedMilliseconds);
		}
		StorePropertiesInner(document, errors);
	}

	protected abstract void LoadPropertiesInner(XmlDocument document, ErrorList errors);

	protected abstract void StorePropertiesInner(XmlDocument document, ErrorList errors);
}
