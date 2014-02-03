using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Redbox.Command.Tokenizer;
using Redbox.Core;

namespace Redbox.IPC.Framework;

internal class CommandInstance
{
	public readonly IDictionary<string, FormMethod> FormMethodCache = new Dictionary<string, FormMethod>();

	public Type CommandType { get; set; }

	public string CommandDescription { get; set; }

	public static CommandInstance GetCommandInstance(Type commandType)
	{
		CommandInstance commandInstance = new CommandInstance();
		commandInstance.CommandType = commandType;
		CommandInstance commandInstance2 = commandInstance;
		if (Attribute.GetCustomAttribute(commandType, typeof(DescriptionAttribute)) is DescriptionAttribute descriptionAttribute)
		{
			commandInstance2.CommandDescription = descriptionAttribute.Description;
		}
		MethodInfo[] methods = commandType.GetMethods();
		foreach (MethodInfo methodInfo in methods)
		{
			NotLoggableAttribute notLoggableAttribute = Attribute.GetCustomAttribute(methodInfo, typeof(NotLoggableAttribute)) as NotLoggableAttribute;
			if (!(Attribute.GetCustomAttribute(methodInfo, typeof(CommandFormAttribute)) is CommandFormAttribute commandFormAttribute))
			{
				continue;
			}
			ParameterInfo[] parameters = methodInfo.GetParameters();
			if (parameters.Length == 0 || parameters[0].ParameterType != typeof(CommandContext))
			{
				continue;
			}
			UsageAttribute usageAttribute = Attribute.GetCustomAttribute(methodInfo, typeof(UsageAttribute)) as UsageAttribute;
			DescriptionAttribute descriptionAttribute2 = Attribute.GetCustomAttribute(methodInfo, typeof(DescriptionAttribute)) as DescriptionAttribute;
			FormMethod formMethod = new FormMethod();
			formMethod.Method = methodInfo;
			formMethod.Usage = ((usageAttribute != null) ? usageAttribute.Template : string.Empty);
			formMethod.Description = ((descriptionAttribute2 != null) ? descriptionAttribute2.Description : string.Empty);
			formMethod.Loggable = notLoggableAttribute == null;
			FormMethod formMethod2 = formMethod;
			int num = 0;
			ParameterInfo[] array = parameters;
			foreach (ParameterInfo parameterInfo in array)
			{
				if (parameterInfo.ParameterType == typeof(CommandContext))
				{
					continue;
				}
				FormMethodParameter formMethodParameter = new FormMethodParameter();
				formMethodParameter.Parameter = parameterInfo;
				formMethodParameter.Index = num++;
				FormMethodParameter formMethodParameter2 = formMethodParameter;
				CommandKeyValueAttribute commandKeyValueAttribute = Attribute.GetCustomAttribute(parameterInfo, typeof(CommandKeyValueAttribute)) as CommandKeyValueAttribute;
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
		MethodInfo method = commandType.GetMethod("Default", new Type[1] { typeof(CommandContext) });
		if (method != null)
		{
			FormMethod formMethod3 = new FormMethod();
			formMethod3.Method = method;
			FormMethod formMethod4 = formMethod3;
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

	public void InvokeDefault(CommandResult result, CommandContext context, CommandTokenizer tokenizer)
	{
		if (HasDefault())
		{
			FormMethodCache["Default"].Invoke(result, context, tokenizer, GetInstance());
		}
	}

	public FormMethod GetMethod(string formName)
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
