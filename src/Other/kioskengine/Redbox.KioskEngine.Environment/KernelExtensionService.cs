using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Xml;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class KernelExtensionService : IKernelExtensionService
{
	private string m_applicationPath;

	private readonly object m_syncObject = new object();

	private IDictionary<Guid, IKernelExtension> m_innerExtensions;

	public static IKernelExtensionService Instance => Singleton<KernelExtensionService>.Instance;

	public ReadOnlyCollection<IKernelExtension> Extensions => new List<IKernelExtension>(InnerExtensions.Values).AsReadOnly();

	public string[] SearchPath
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			if (service != null)
			{
				return service.GetValue("Core", "KernelExtensionSearchPath", new string[1] { "..\\kext" });
			}
			return new string[1] { "..\\kext" };
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>()?.SetValue("Core", "KernelExtensionSearchPath", value);
		}
	}

	internal IDictionary<Guid, IKernelExtension> InnerExtensions
	{
		get
		{
			if (m_innerExtensions == null)
			{
				m_innerExtensions = new Dictionary<Guid, IKernelExtension>();
			}
			return m_innerExtensions;
		}
	}

	public IKernelExtension GetExtension(Guid id)
	{
		lock (m_syncObject)
		{
			return InnerExtensions.ContainsKey(id) ? InnerExtensions[id] : null;
		}
	}

	public ErrorList Initialize(string applicationPath)
	{
		LogHelper.Instance.Log("Initialize kernel extension service:");
		ErrorList errorList = new ErrorList();
		m_applicationPath = applicationPath;
		ServiceLocator.Instance.AddService(typeof(IKernelExtensionService), Instance);
		List<string> names = new List<string>();
		errorList.AddRange(EnumerateExtensions(names));
		return errorList;
	}

	public ErrorList EnumerateExtensions(List<string> names)
	{
		ErrorList errorList = new ErrorList();
		List<string> list = new List<string>();
		string[] searchPath = SearchPath;
		foreach (string text in searchPath)
		{
			if (!Path.IsPathRooted(text))
			{
				list.Add(Path.Combine(Path.GetDirectoryName(m_applicationPath), text));
			}
			else
			{
				list.Add(text);
			}
		}
		foreach (string item in list)
		{
			LogHelper.Instance.Log("...Search path: {0}", item);
			if (!Directory.Exists(item))
			{
				continue;
			}
			string[] files = Directory.GetFiles(item, "*.kext", SearchOption.AllDirectories);
			string[] array = files;
			foreach (string text2 in array)
			{
				if (!Path.GetDirectoryName(text2).EndsWith(".staged_"))
				{
					errorList.AddRange(LoadExtension(text2, out var name));
					if (name != null)
					{
						LogHelper.Instance.Log("...Loaded kernel extension '{0}' from file: {1}", name, text2);
						names.Add(name);
					}
				}
			}
		}
		return errorList;
	}

	public ErrorList LoadExtension(string path, out string name)
	{
		name = null;
		ErrorList errorList = new ErrorList();
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(path);
		if (xmlDocument.DocumentElement == null)
		{
			return errorList;
		}
		name = XmlNodeExtensions.GetNodeValue<string>(xmlDocument.DocumentElement.SelectSingleNode("Name"), null);
		Guid? nodeValue = XmlNodeExtensions.GetNodeValue<Guid?>(xmlDocument.DocumentElement.SelectSingleNode("ID"), null);
		string nodeValue2 = XmlNodeExtensions.GetNodeValue<string>(xmlDocument.DocumentElement.SelectSingleNode("Title"), null);
		string nodeValue3 = XmlNodeExtensions.GetNodeValue<string>(xmlDocument.DocumentElement.SelectSingleNode("Type"), null);
		string nodeValue4 = XmlNodeExtensions.GetNodeValue<string>(xmlDocument.DocumentElement.SelectSingleNode("Author"), null);
		string nodeValue5 = XmlNodeExtensions.GetNodeValue<string>(xmlDocument.DocumentElement.SelectSingleNode("Version"), null);
		string nodeValue6 = XmlNodeExtensions.GetNodeValue<string>(xmlDocument.DocumentElement.SelectSingleNode("Category"), null);
		string nodeValue7 = XmlNodeExtensions.GetNodeValue<string>(xmlDocument.DocumentElement.SelectSingleNode("Copyright"), null);
		string nodeValue8 = XmlNodeExtensions.GetNodeValue<string>(xmlDocument.DocumentElement.SelectSingleNode("Trademark"), null);
		Bitmap icon = null;
		string nodeValue9 = XmlNodeExtensions.GetNodeValue<string>(xmlDocument.DocumentElement.SelectSingleNode("Icon"), null);
		if (!string.IsNullOrEmpty(nodeValue9))
		{
			using MemoryStream stream = new MemoryStream(StringExtensions.Base64ToBytes(nodeValue9));
			icon = (Bitmap)Image.FromStream(stream);
		}
		string nodeValue10 = XmlNodeExtensions.GetNodeValue<string>(xmlDocument.DocumentElement.SelectSingleNode("Description"), null);
		try
		{
			string[] array = nodeValue3.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			if (array.Length == 2)
			{
				string assemblyFile = Path.Combine(Path.GetDirectoryName(path), array[0]) + ".dll";
				Assembly assembly = Assembly.LoadFrom(assemblyFile);
				if (assembly != null)
				{
					Type type = assembly.GetType(array[1]);
					if (type != null && Activator.CreateInstance(type) is IKernelExtensionHost kernelExtensionHost)
					{
						KernelExtension kernelExtension = new KernelExtension();
						kernelExtension.Author = nodeValue4;
						kernelExtension.Category = nodeValue6;
						kernelExtension.Copyright = nodeValue7;
						kernelExtension.Description = nodeValue10;
						kernelExtension.Host = kernelExtensionHost;
						kernelExtension.Icon = icon;
						kernelExtension.ID = nodeValue.Value;
						kernelExtension.IsUnloadSupported = false;
						kernelExtension.Name = name;
						kernelExtension.Path = path;
						kernelExtension.Title = nodeValue2;
						kernelExtension.Trademark = nodeValue8;
						kernelExtension.Version = nodeValue5;
						KernelExtension kernelExtension2 = kernelExtension;
						XmlNodeList xmlNodeList = xmlDocument.DocumentElement.SelectNodes("Dependency");
						if (xmlNodeList != null)
						{
							foreach (XmlNode item in xmlNodeList)
							{
								kernelExtension2.InnerDependencies.Add(XmlNodeExtensions.GetNodeValue<Guid>(item));
							}
						}
						kernelExtensionHost.Extension = kernelExtension2;
						errorList.AddRange(kernelExtensionHost.PreLoad());
						InnerExtensions[nodeValue.Value] = kernelExtension2;
					}
				}
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("P999", $"An unhandled exception was raised while loading kernel extension: {path}.   See the details of this error for more information on the exception.", e));
		}
		return errorList;
	}

	public void NotifyOfHostCrash(Exception e)
	{
		lock (m_syncObject)
		{
			foreach (IKernelExtension value in InnerExtensions.Values)
			{
				if (value.Host != null)
				{
					value.Host.HandleHostCrash(e);
				}
			}
		}
	}

	public ErrorList LoadAllExtensions()
	{
		lock (m_syncObject)
		{
			ErrorList errorList = new ErrorList();
			foreach (IKernelExtension value in InnerExtensions.Values)
			{
				if (value.Host != null)
				{
					errorList.AddRange(value.Host.Load());
				}
			}
			return errorList;
		}
	}

	public ErrorList ResetAllExtensions()
	{
		lock (m_syncObject)
		{
			ErrorList errorList = new ErrorList();
			foreach (IKernelExtension value in InnerExtensions.Values)
			{
				if (value.Host != null)
				{
					errorList.AddRange(value.Host.Reset());
				}
			}
			return errorList;
		}
	}

	public ErrorList ActivateAllExtensions()
	{
		lock (m_syncObject)
		{
			ErrorList errorList = new ErrorList();
			foreach (IKernelExtension value in InnerExtensions.Values)
			{
				if (value.Host != null)
				{
					errorList.AddRange(value.Host.Activate());
				}
			}
			return errorList;
		}
	}

	public ErrorList DeactivateAllExtensions()
	{
		lock (m_syncObject)
		{
			ErrorList errorList = new ErrorList();
			foreach (IKernelExtension value in InnerExtensions.Values)
			{
				if (value.Host != null)
				{
					errorList.AddRange(value.Host.Deactivate());
				}
			}
			return errorList;
		}
	}

	public bool CanSwitch(out ErrorList errors)
	{
		errors = new ErrorList();
		bool result = true;
		lock (m_syncObject)
		{
			foreach (IKernelExtension value in InnerExtensions.Values)
			{
				if (value.Host != null && !value.Host.CanSwitch(out var errors2))
				{
					result = false;
					errors.AddRange(errors2);
				}
			}
			return result;
		}
	}

	private KernelExtensionService()
	{
	}
}
