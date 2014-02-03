using System;
using System.Reflection;

namespace Skybound.VisualTips.External;

internal class VisualTipService
{
	private static MethodInfo SetExtenderMethod;

	private static bool SetExtenderChecked;

	private VisualTipService()
	{
	}

	public static void SetExtender(Type controlType, IVisualTipExtender extender)
	{
		if (SetExtenderMethod == null && !SetExtenderChecked)
		{
			SetExtenderChecked = true;
			Type type = Type.GetType("Skybound.VisualTips.VisualTipProvider, Skybound.VisualTips");
			if (type != null)
			{
				SetExtenderMethod = type.GetMethod("SetExtender", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[2]
				{
					typeof(Type),
					typeof(object)
				}, null);
			}
		}
		if (SetExtenderMethod != null)
		{
			SetExtenderMethod.Invoke(null, new object[2] { controlType, extender });
		}
	}
}
