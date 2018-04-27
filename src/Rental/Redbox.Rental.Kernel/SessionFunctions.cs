using System;
using System.Collections.Generic;
using System.Reflection;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.Rental.Model;
using Redbox.Rental.Services;
using Redbox.Rental.Services.Session;

namespace Redbox.Rental.Kernel;

public static class SessionFunctions
{
	private static BaseSettingFunctions _baseSettingFunctions = new BaseSettingFunctions();

	[KernelFunction(Name = "kernel.close_open_session_and_start_new_rental_session")]
	public static void CloseOpenSessionAndStartNewRentalSession(string newSessionContext)
	{
		ServiceLocator.Instance.GetService<IRedboxRental>()?.CloseOpenSessionAndStartNewRentalSession(newSessionContext);
	}

	[KernelFunction(Name = "kernel.is_personalization_session_signed_in")]
	public static bool IsPersonalizationSessionSignedIn()
	{
		return OrchestrationService.IsPersonalizationSessionSignedIn();
	}

	[KernelFunction(Name = "kernel.is_rental_session_active")]
	public static bool IsRentalSessionActive()
	{
		return OrchestrationService.IsRentalSessionActive();
	}

	[KernelFunction(Name = "kernel.close_shopping_cart_analytics")]
	public static void CloseShoppingCartAnalytics(string sessionId, string isSessionAbandoned)
	{
		OrchestrationService.CloseShoppingCartAnalytics((sessionId != null) ? new Guid(sessionId) : Guid.NewGuid(), isSessionAbandoned);
	}

	[KernelFunction(Name = "kernel.get_session_value")]
	public static object GetSessionValue(string valueName)
	{
		object obj = null;
		if (valueName != null)
		{
			return _baseSettingFunctions.GetPropertyValue(Session.Instance, valueName);
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
					object propertyValue = _baseSettingFunctions.GetPropertyValue(Session.Instance, obj.ToString());
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
		_baseSettingFunctions.GetProperty(Session.Instance, propertyName, out resultObject, out resultPropertyInfo);
		if (resultPropertyInfo != null && resultObject != null)
		{
			object value2 = _baseSettingFunctions.ConvertValueToPropertyType(resultPropertyInfo, value);
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
		_baseSettingFunctions.GetProperty(Session.Instance, propertyName, out resultObject, out resultPropertyInfo);
		if (valueToAdd != null)
		{
			if (resultPropertyInfo != null && resultObject != null)
			{
				object value = resultPropertyInfo.GetValue(resultObject, null);
				object obj = null;
				if (resultPropertyInfo.PropertyType == typeof(decimal))
				{
					obj = _baseSettingFunctions.AddDecimalValues(value, valueToAdd);
				}
				else if (resultPropertyInfo.PropertyType == typeof(int))
				{
					obj = _baseSettingFunctions.AddIntValues(value, valueToAdd);
				}
				else
				{
					if (!(resultPropertyInfo.PropertyType == typeof(double)))
					{
						throw new Exception($"Error in AddToPropertyValue. Unsupported property data type {resultPropertyInfo.PropertyType}");
					}
					obj = _baseSettingFunctions.AddDoubleValues(value, valueToAdd);
				}
				resultPropertyInfo.SetValue(resultObject, obj, null);
				return obj;
			}
			throw new ArgumentException($"Error in AddToPropertyValue.  Unable to locate property {propertyName}");
		}
		return null;
	}
}
