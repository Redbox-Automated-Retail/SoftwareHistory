using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using Redbox.Core;

namespace Redbox.Macros;

internal sealed class FunctionFactory
{
	private static readonly Hashtable m_methodInfoCollection = new Hashtable();

	[ReflectionPermission(SecurityAction.Demand, Flags = ReflectionPermissionFlag.NoFlags)]
	public static bool ScanAssembly(string assemblyFile)
	{
		Assembly assembly = Assembly.LoadFrom(assemblyFile);
		return ScanAssembly(assembly);
	}

	[ReflectionPermission(SecurityAction.Demand, Flags = ReflectionPermissionFlag.NoFlags)]
	public static bool ScanAssembly(Assembly assembly)
	{
		bool flag = false;
		Type[] types = assembly.GetTypes();
		foreach (Type type in types)
		{
			bool flag2 = ScanTypeForFunctions(type);
			flag = flag || flag2;
		}
		return flag;
	}

	[ReflectionPermission(SecurityAction.Demand, Flags = ReflectionPermissionFlag.NoFlags)]
	public static void ScanDir(string path, bool failOnError)
	{
		if (string.IsNullOrEmpty(path))
		{
			return;
		}
		DirectoryInfo directoryInfo = new DirectoryInfo(path);
		FileInfo[] files = directoryInfo.GetFiles("*.dll");
		FileInfo[] array = files;
		foreach (FileInfo fileInfo in array)
		{
			try
			{
				ScanAssembly(fileInfo.FullName);
			}
			catch (Exception innerException)
			{
				string text = string.Format(CultureInfo.InvariantCulture, "Failure scanning \"{0}\" for extensions", fileInfo);
				if (failOnError)
				{
					throw new EvaluatorException(text + ".", Location.UnknownLocation, innerException);
				}
			}
		}
	}

	internal static MethodInfo LookupFunction(string functionName, FunctionArgument[] args)
	{
		object obj = m_methodInfoCollection[functionName];
		if (obj == null)
		{
			throw new EvaluatorException(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1052"), functionName));
		}
		if (obj is MethodInfo methodInfo)
		{
			if (methodInfo.GetParameters().Length == args.Length)
			{
				CheckDeprecation(functionName, methodInfo);
				return methodInfo;
			}
		}
		else
		{
			ArrayList arrayList = (ArrayList)obj;
			for (int i = 0; i < arrayList.Count; i++)
			{
				MethodInfo methodInfo2 = (MethodInfo)arrayList[i];
				if (methodInfo2.GetParameters().Length == args.Length)
				{
					CheckDeprecation(functionName, methodInfo2);
					return methodInfo2;
				}
			}
		}
		throw new EvaluatorException(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1044"), functionName, args.Length));
	}

	private static void CheckDeprecation(string functionName, MemberInfo function)
	{
		ObsoleteAttribute obsoleteAttribute = ((ObsoleteAttribute)Attribute.GetCustomAttribute(function, typeof(ObsoleteAttribute), inherit: true)) ?? ((ObsoleteAttribute)Attribute.GetCustomAttribute(function.DeclaringType, typeof(ObsoleteAttribute), inherit: true));
		if (obsoleteAttribute != null)
		{
			string text = string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1087"), functionName, obsoleteAttribute.Message);
			if (obsoleteAttribute.IsError)
			{
				throw new EvaluatorException(text, Location.UnknownLocation);
			}
			Redbox_002EMacros1870_002ELogHelper.Instance.Log($"{text}", Redbox_002EMacros1870_002ELogEntryType.Info);
		}
	}

	private static bool ScanTypeForFunctions(Type type)
	{
		try
		{
			FunctionSetAttribute functionSetAttribute = (FunctionSetAttribute)Attribute.GetCustomAttribute(type, typeof(FunctionSetAttribute));
			if (functionSetAttribute == null)
			{
				return false;
			}
			bool flag = type == typeof(ExpressionEvaluator);
			if (type.IsSubclassOf(typeof(FunctionSetBase)) && !type.IsAbstract)
			{
				flag = true;
			}
			if (flag)
			{
				string prefix = functionSetAttribute.Prefix;
				if (!string.IsNullOrEmpty(prefix))
				{
					prefix += "::";
					MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
					foreach (MethodInfo methodInfo in methods)
					{
						FunctionAttribute functionAttribute = (FunctionAttribute)Attribute.GetCustomAttribute(methodInfo, typeof(FunctionAttribute));
						if (functionAttribute != null)
						{
							RegisterFunction(prefix + functionAttribute.Name, methodInfo);
						}
					}
					return true;
				}
				Redbox_002EMacros1870_002ELogHelper.Instance.Log($"Ignoring functions in type \"{type.AssemblyQualifiedName}\": no prefix was set.", Redbox_002EMacros1870_002ELogEntryType.Info);
				return false;
			}
			return false;
		}
		catch
		{
			Redbox_002EMacros1870_002ELogHelper.Instance.Log($"Failure scanning \"{type.AssemblyQualifiedName}\" for functions.", Redbox_002EMacros1870_002ELogEntryType.Error);
			throw;
		}
	}

	private static void RegisterFunction(string key, MethodInfo info)
	{
		object obj = m_methodInfoCollection[key];
		if (obj == null)
		{
			m_methodInfoCollection.Add(key, info);
			return;
		}
		ArrayList arrayList;
		if (!(obj is MethodInfo value))
		{
			arrayList = (ArrayList)obj;
			arrayList.Add(info);
			return;
		}
		ArrayList arrayList2 = new ArrayList(3);
		arrayList2.Add(value);
		arrayList2.Add(info);
		arrayList = arrayList2;
		m_methodInfoCollection[key] = arrayList;
	}
}
