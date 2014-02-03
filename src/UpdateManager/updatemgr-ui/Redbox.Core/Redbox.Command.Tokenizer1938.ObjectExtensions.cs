using System.Web.Script.Serialization;

namespace Redbox.Core;

internal static class Redbox_002ECommand_002ETokenizer1938_002EObjectExtensions
{
	public static string ToJson(this object instance)
	{
		JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
		javaScriptSerializer.MaxJsonLength = int.MaxValue;
		JavaScriptSerializer javaScriptSerializer2 = javaScriptSerializer;
		javaScriptSerializer2.RegisterConverters(new JavaScriptConverter[2]
		{
			new Redbox_002ECommand_002ETokenizer1938_002EDateTimeConverter(),
			new Redbox_002ECommand_002ETokenizer1938_002ETimeSpanConverter()
		});
		return javaScriptSerializer2.Serialize(instance);
	}
}
