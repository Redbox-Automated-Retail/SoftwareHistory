using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Redbox.KioskEngine.ComponentModel;

public static class EnumExtensions
{
	public static string GetDisplayName(this Enum val)
	{
		return val.GetType().GetMember(val.ToString()).FirstOrDefault()?.GetCustomAttribute<DisplayAttribute>(inherit: false)?.Name ?? val.ToString();
	}

	public static string GetDisplayDescription(this Enum val)
	{
		return val.GetType().GetMember(val.ToString()).FirstOrDefault()?.GetCustomAttribute<DisplayAttribute>(inherit: false)?.Description ?? val.ToString();
	}
}
