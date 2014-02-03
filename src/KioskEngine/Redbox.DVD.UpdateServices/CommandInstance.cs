using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Redbox.Command.Tokenizer;
using Redbox.Core;

namespace Redbox.IPC.Framework;

internal class CommandInstance
{
	public readonly IDictionary<string, Redbox.IPC.Framework.FormMethod> FormMethodCache = new Dictionary<string, Redbox.IPC.Framework.FormMethod>();

	public Type CommandType { get; set; }

	public string CommandDescription { get; set; }

	public static Redbox.IPC.Framework.CommandInstance GetCommandInstance(Type commandType)
	{
		Redbox.IPC.Framework.CommandInstance commandInstance = new Redbox.IPC.Framework.CommandInstance();
		commandInstance.CommandType = commandType;
		Redbox.IPC.Framework.CommandInstance commandInstance2 = commandInstance;
		if (Attribute.GetCustomAttribute(commandType, typeof(DescriptionAttribute)) is DescriptionAttribute descriptionAttribute)
		{
			commandInstance2.CommandDescription = descriptionAttribute.Description;
		}
		MethodInfo[] methods = commandType.GetMethods();
		foreach (MethodInfo methodInfo in methods)
		{
			Redbox.IPC.Framework.NotLoggableAttribute notLoggableAttribute = Attribute.GetCustomAttribute(methodInfo, typeof(Redbox.IPC.Framework.NotLoggableAttribute)) as Redbox.IPC.Framework.NotLoggableAttribute;
			if (!(Attribute.GetCustomAttribute(methodInfo, typeof(Redbox.IPC.Framework.CommandFormAttribute)) is Redbox.IPC.Framework.CommandFormAttribute commandFormAttribute))
			{
				continue;
			}
			ParameterInfo[] parameters = methodInfo.GetParameters();
			if (parameters.Length == 0 || parameters[0].ParameterType != typeof(Redbox.IPC.Framework.CommandContext))
			{
				continue;
			}
			UsageAttribute usageAttribute = Attribute.GetCustomAttribute(methodInfo, typeof(UsageAttribute)) as UsageAttribute;
			DescriptionAttribute descriptionAttribute2 = Attribute.GetCustomAttribute(methodInfo, typeof(DescriptionAttribute)) as DescriptionAttribute;
			Redbox.IPC.Framework.FormMethod formMethod = new Redbox.IPC.Framework.FormMethod();
			formMethod.Method = methodInfo;
			formMethod.Usage = ((usageAttribute != null) ? usageAttribute.Template : string.Empty);
			formMethod.Description = ((descriptionAttribute2 != null) ? descriptionAttribute2.Description : string.Empty);
			formMethod.Loggable = notLoggableAttribute == null;
			Redbox.IPC.Framework.FormMethod formMethod2 = formMethod;
			int num = 0;
			ParameterInfo[] array = parameters;
			foreach (ParameterInfo parameterInfo in array)
			{
				if (parameterInfo.ParameterType == typeof(Redbox.IPC.Framework.CommandContext))
				{
					continue;
				}
				Redbox.IPC.Framework.FormMethodParameter formMethodParameter = new Redbox.IPC.Framework.FormMethodParameter();
				formMethodParameter.Parameter = parameterInfo;
				formMethodParameter.Index = num++;
				Redbox.IPC.Framework.FormMethodParameter formMethodParameter2 = formMethodParameter;
				Redbox.IPC.Framework.CommandKeyValueAttribute commandKeyValueAttribute = Attribute.GetCustomAttribute(parameterInfo, typeof(Redbox.IPC.Framework.CommandKeyValueAttribute)) as Redbox.IPC.Framework.CommandKeyValueAttribute;
				formMethodParameter2.KeyName = parameterInfo.Name.ToUpper();
				if (commandKeyValueAttribute != null)
				{
					formMethodParameter2.MetaData = commandKeyValueAttribute;
					if (commandKeyValueAttribute.KeyName != null)
					{
						formMethodParameter2.KeyName = commandKeyValueAttribute.KeyName.ToUpper();
					}
				}
				formMethod2.ParameterCache[formMethodParameter2.KeyName] = formMethodParameter2;
			}
			formMethod2.Compile(commandInstance2.CommandType);
			commandInstance2.FormMethodCache[commandFormAttribute.Name.ToUpper()] = formMethod2;
		}
		MethodInfo method = commandType.GetMethod("Default", new Type[1] { typeof(Redbox.IPC.Framework.CommandContext) });
		if (method != null)
		{
			Redbox.IPC.Framework.FormMethod formMethod3 = new Redbox.IPC.Framework.FormMethod();
			formMethod3.Method = method;
			Redbox.IPC.Framework.FormMethod formMethod4 = formMethod3;
			formMethod4.Compile(commandInstance2.CommandType);
			commandInstance2.FormMethodCache["Default"] = formMethod4;
		}
		return commandInstance2;
	}

	public object GetInstance()
	{
		return Activator.CreateInstance(CommandType);
	}

	public bool HasDefault()
	{
		return FormMethodCache.ContainsKey("Default");
	}

	public bool HasOnlyDefault()
	{
		if (FormMethodCache.Count == 1)
		{
			return HasDefault();
		}
		return false;
	}

	public void InvokeDefault(Redbox.IPC.Framework.CommandResult result, Redbox.IPC.Framework.CommandContext context, CommandTokenizer tokenizer)
	{
		if (HasDefault())
		{
			FormMethodCache["Default"].Invoke(result, context, tokenizer, GetInstance());
		}
	}

	public Redbox.IPC.Framework.FormMethod GetMethod(string formName)
	{
		if (string.IsNullOrEmpty(formName))
		{
			return null;
		}
		string key = formName.ToUpper();
		if (FormMethodCache.ContainsKey(key))
		{
			return FormMethodCache[key];
		}
		return null;
	}
}
