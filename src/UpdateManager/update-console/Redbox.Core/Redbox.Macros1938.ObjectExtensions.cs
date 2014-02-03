using System.Web.Script.Serialization;

namespace Redbox.Core;

internal static class Redbox_002EMacros1938_002EObjectExtensions
{
	public static string ToJson(this object instance)
	{
		JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
		javaScriptSerializer.MaxJsonLength = int.MaxValue;
		JavaScriptSerializer javaScriptSerializer2 = javaScriptSerializer;
		javaScriptSerializer2.RegisterConverters(new Redbox_002EMacros1938_002EDateTimeConverter[1]
		{
			new Redbox_002EMacros1938_002EDateTimeConverter()
		});
		return javaScriptSerializer2.Serialize(instance);
	}
}
