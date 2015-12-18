using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.Rental.Model;
using Redbox.Rental.Services.Session;

namespace Redbox.Rental.Kernel;

public static class SessionFunctions
{
	[KernelFunction(Name = "kernel.initialize_session")]
	public static void InitializeSession()
	{
		Session.Instance = new Session();
	}

	[KernelFunction(Name = "kernel.get_session_value")]
	public static object GetSessionValue(string valueName)
	{
		object obj = null;
		if (valueName != null)
		{
			return Session.Instance.GetPropertyValue(valueName);
		}
		throw new ArgumentNullException("Error in GetSessionValue.  valueName can not be null.");
	}

	[KernelFunction(Name = "kernel.get_session_values")]
	public static LuaTable GetSessionValues(LuaTable propertyNames)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (propertyNames != null)
		{
			foreach (object key in propertyNames.Keys)
			{
				object obj = propertyNames[key];
				if (obj != null)
				{
					object propertyValue = Session.Instance.GetPropertyValue(obj.ToString());
					dictionary.Add(obj.ToString().Replace('.', '_'), propertyValue);
				}
			}
			return dictionary.ToLuaTable();
		}
		throw new ArgumentNullException("Error in GetSessionValues.  propertyNames can not be null.");
	}

	[KernelFunction(Name = "kernel.set_session_value")]
	public static void SetPropertyValue(string propertyName, object value)
	{
		_ = Session.Instance;
		object resultObject = null;
		PropertyInfo resultPropertyInfo = null;
		Session.Instance.GetProperty(propertyName, out resultObject, out resultPropertyInfo);
		if (resultPropertyInfo != null && resultObject != null)
		{
			object value2 = ConvertValueToPropertyType(resultPropertyInfo, value);
			resultPropertyInfo.SetValue(resultObject, value2, null);
			return;
		}
		throw new ArgumentException($"kernel.set_session_value unable to locate property {propertyName}");
	}

	[KernelFunction(Name = "kernel.add_to_session_value")]
	public static object AddToPropertyValue(string propertyName, object valueToAdd)
	{
		_ = Session.Instance;
		object resultObject = null;
		PropertyInfo resultPropertyInfo = null;
		if (propertyName == null)
		{
			throw new ArgumentNullException("Error in AddToPropertyValue.  propertyName can not be null.");
		}
		if (valueToAdd == null)
		{
			throw new ArgumentNullException("Error in AddToPropertyValue.  valueToAdd can not be null.");
		}
		Session.Instance.GetProperty(propertyName, out resultObject, out resultPropertyInfo);
		if (valueToAdd != null)
		{
			if (resultPropertyInfo != null && resultObject != null)
			{
				object value = resultPropertyInfo.GetValue(resultObject, null);
				object obj = null;
				if (resultPropertyInfo.PropertyType == typeof(decimal))
				{
					obj = AddDecimalValues(value, valueToAdd);
				}
				else if (resultPropertyInfo.PropertyType == typeof(int))
				{
					obj = AddIntValues(value, valueToAdd);
				}
				else
				{
					if (resultPropertyInfo.PropertyType != typeof(double))
					{
						throw new Exception($"Error in AddToPropertyValue. Unsupported property data type {resultPropertyInfo.PropertyType}");
					}
					obj = AddDoubleValues(value, valueToAdd);
				}
				resultPropertyInfo.SetValue(resultObject, obj, null);
				return obj;
			}
			throw new ArgumentException($"Error in AddToPropertyValue.  Unable to locate property {propertyName}");
		}
		return null;
	}

	private static double AddDoubleValues(object value1, object value2)
	{
		return Convert.ToDouble(value1) + Convert.ToDouble(value2);
	}

	private static decimal AddDecimalValues(object value1, object value2)
	{
		return Convert.ToDecimal(value1) + Convert.ToDecimal(value2);
	}

	private static int AddIntValues(object value1, object value2)
	{
		return Convert.ToInt32(value1) + Convert.ToInt32(value2);
	}

	private static object ConvertValueToPropertyType(PropertyInfo propertyInfo, object value)
	{
		object result = value;
		if (propertyInfo.PropertyType == typeof(int))
		{
			result = Convert.ToInt32(value);
		}
		else if (propertyInfo.PropertyType == typeof(decimal))
		{
			result = Convert.ToDecimal(value);
		}
		return result;
	}

	private static void GetProperty(this object obj, string name, out object resultObject, out PropertyInfo resultPropertyInfo)
	{
		resultObject = null;
		resultPropertyInfo = null;
		List<string> list = name.Split('.').ToList();
		for (int i = 0; i < list.Count; i++)
		{
			string name2 = list[i];
			if (obj == null)
			{
				continue;
			}
			PropertyInfo property = obj.GetType().GetProperty(name2);
			if (property != null)
			{
				if (i != list.Count - 1)
				{
					obj = property.GetValue(obj, null);
					continue;
				}
				resultPropertyInfo = property;
				resultObject = obj;
			}
		}
	}

	private static object GetPropertyValue(this object obj, string name)
	{
		bool flag = false;
		string[] array = name.Split('.');
		foreach (string name2 in array)
		{
			if (obj == null)
			{
				break;
			}
			flag = false;
			PropertyInfo property = obj.GetType().GetProperty(name2);
			if (property == null)
			{
				break;
			}
			obj = property.GetValue(obj, null);
			flag = true;
		}
		if (!flag)
		{
			throw new ArgumentException($"Error in GetPropertyValue.  Unable to locate property {name}");
		}
		return obj;
	}
}
