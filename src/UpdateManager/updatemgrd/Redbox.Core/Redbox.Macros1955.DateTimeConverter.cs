using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace Redbox.Core;

internal class Redbox_002EMacros1955_002EDateTimeConverter : JavaScriptConverter
{
	private const string DateKey = "theDate";

	public override IEnumerable<Type> SupportedTypes
	{
		get
		{
			List<Type> list = new List<Type>();
			list.Add(typeof(DateTime));
			list.Add(typeof(DateTime?));
			return list;
		}
	}

	public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (obj.GetType() == typeof(DateTime))
		{
			dictionary["theDate"] = ((DateTime)obj).Ticks;
			return dictionary;
		}
		if (obj.GetType() == typeof(DateTime?))
		{
			DateTime? dateTime = (DateTime?)obj;
			if (dateTime.HasValue)
			{
				dictionary["theDate"] = dateTime.Value.Ticks;
			}
		}
		return dictionary;
	}

	public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
	{
		if (dictionary.ContainsKey("theDate"))
		{
			return new DateTime(long.Parse(dictionary["theDate"].ToString()), DateTimeKind.Unspecified);
		}
		return null;
	}
}
