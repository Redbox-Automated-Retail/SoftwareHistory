using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace Redbox.KioskEngine.ComponentModel;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class KernelFunctionAttribute : Attribute
{
	public string Name { get; set; }

	public string ExtensionName { get; set; }

	public static ReadOnlyCollection<KernelFunctionInfo> GetKernelFunctionInfos(Assembly assembly, string extension)
	{
		List<KernelFunctionInfo> list = new List<KernelFunctionInfo>();
		Type[] types = assembly.GetTypes();
		foreach (Type type in types)
		{
			MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (MethodInfo methodInfo in methods)
			{
				DescriptionAttribute descriptionAttribute = (DescriptionAttribute)Attribute.GetCustomAttribute(methodInfo, typeof(DescriptionAttribute));
				ObsoleteAttribute obsoleteAttribute = (ObsoleteAttribute)Attribute.GetCustomAttribute(methodInfo, typeof(ObsoleteAttribute));
				KernelFunctionAttribute[] array = (KernelFunctionAttribute[])Attribute.GetCustomAttributes(methodInfo, typeof(KernelFunctionAttribute));
				if (array != null && array.Length > 0)
				{
					KernelFunctionInfo kernelFunctionInfo = new KernelFunctionInfo();
					kernelFunctionInfo.Method = methodInfo;
					kernelFunctionInfo.Extension = extension;
					kernelFunctionInfo.Description = descriptionAttribute?.Description;
					kernelFunctionInfo.DeprecationWarning = obsoleteAttribute?.Message;
					KernelFunctionInfo kernelFunctionInfo2 = kernelFunctionInfo;
					kernelFunctionInfo2.InnerAttributes.AddRange(array);
					list.Add(kernelFunctionInfo2);
				}
			}
		}
		return list.AsReadOnly();
	}

	public string GetNamespace()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (!string.IsNullOrEmpty(Name))
		{
			string[] array = Name.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < array.Length - 1; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append(".");
				}
				stringBuilder.Append(array[i]);
			}
		}
		return stringBuilder.ToString();
	}
}
