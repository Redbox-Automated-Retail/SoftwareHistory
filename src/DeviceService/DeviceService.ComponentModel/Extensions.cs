using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace DeviceService.ComponentModel;

public static class Extensions
{
	public static string GetDescription(this Enum value)
	{
		return value.GetType().GetMember(value.ToString()).FirstOrDefault()?.GetCustomAttribute<DescriptionAttribute>()?.Description;
	}
}
