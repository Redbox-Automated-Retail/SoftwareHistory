using System.Reflection;
using Redbox.Core;

namespace Redbox.IPC.Framework;

[Command("statistics")]
internal class StatisticsCommand
{
	[CommandForm(Name = "show")]
	[Usage("STATISTICS show")]
	public void Show(CommandContext context)
	{
		FieldInfo[] fields = typeof(Statistics).GetFields(BindingFlags.Instance | BindingFlags.Public);
		FieldInfo[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			string item = $"{fieldInfo.Name} = {fieldInfo.GetValue(Statistics.Instance)}";
			context.Messages.Add(item);
		}
		PropertyInfo[] properties = typeof(Statistics).GetProperties(BindingFlags.Instance | BindingFlags.Public);
		PropertyInfo[] array2 = properties;
		foreach (PropertyInfo propertyInfo in array2)
		{
			string item2 = $"{propertyInfo.Name} = {propertyInfo.GetValue(Statistics.Instance, null)}";
			context.Messages.Add(item2);
		}
	}
}
