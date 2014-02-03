using System;
using System.Reflection;
using Redbox.Core;

namespace Redbox.TicketServices.Proxy.ComponentModel;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
internal class ResponseCodeAttribute : Attribute
{
	public string Name { get; set; }

	public static object GetFieldsForResponseCode(Type type, string code)
	{
		Type nullableTypeIfWrapped = ConversionHelper.GetNullableTypeIfWrapped(type);
		FieldInfo[] fields = nullableTypeIfWrapped.GetFields();
		FieldInfo[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			ResponseCodeAttribute responseCodes = GetResponseCodes(fieldInfo, code);
			if (responseCodes != null)
			{
				return fieldInfo.GetValue(null);
			}
		}
		return null;
	}

	private static ResponseCodeAttribute GetResponseCodes(MemberInfo fieldInfo, string code)
	{
		ResponseCodeAttribute[] array = (ResponseCodeAttribute[])Attribute.GetCustomAttributes(fieldInfo, typeof(ResponseCodeAttribute));
		ResponseCodeAttribute[] array2 = array;
		foreach (ResponseCodeAttribute responseCodeAttribute in array2)
		{
			if (string.Compare(responseCodeAttribute.Name, code, ignoreCase: true) == 0)
			{
				return responseCodeAttribute;
			}
		}
		ResponseCodeAttribute[] array3 = array;
		foreach (ResponseCodeAttribute responseCodeAttribute2 in array3)
		{
			if (responseCodeAttribute2.Name == "*")
			{
				return responseCodeAttribute2;
			}
		}
		return null;
	}
}
