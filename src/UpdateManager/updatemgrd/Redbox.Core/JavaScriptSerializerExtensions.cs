using System;
using System.Web.Script.Serialization;

namespace Redbox.Core;

internal static class JavaScriptSerializerExtensions
{
	public static object Deserialize(this JavaScriptSerializer serializer, Type type, string json)
	{
		return serializer.GetType().GetMethod("Deserialize").MakeGenericMethod(type)
			.Invoke(serializer, new string[1] { json });
	}
}
