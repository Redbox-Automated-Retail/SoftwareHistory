namespace Redbox.Core;

internal static class ObjectExtensions
{
	public static string ToJson(this object instance)
	{
		return JavaScriptConverterRegistry.Instance.GetSerializer().Serialize(instance);
	}
}
