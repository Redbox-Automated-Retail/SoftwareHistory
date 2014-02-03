using System.Reflection;
using Redbox.Core;

namespace Redbox.IPC.Framework;

[Redbox.IPC.Framework.Command("statistics")]
internal class StatisticsCommand
{
	[Redbox.IPC.Framework.CommandForm(Name = "show")]
	[Usage("STATISTICS show")]
	public void Show(Redbox.IPC.Framework.CommandContext context)
	{
		FieldInfo[] fields = typeof(Redbox.IPC.Framework.Statistics).GetFields(BindingFlags.Instance | BindingFlags.Public);
		FieldInfo[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			string item = $"{fieldInfo.Name} = {fieldInfo.GetValue(Redbox.IPC.Framework.Statistics.Instance)}";
			context.Messages.Add(item);
		}
		PropertyInfo[] properties = typeof(Redbox.IPC.Framework.Statistics).GetProperties(BindingFlags.Instance | BindingFlags.Public);
		PropertyInfo[] array2 = properties;
		foreach (PropertyInfo propertyInfo in array2)
		{
			string item2 = $"{propertyInfo.Name} = {propertyInfo.GetValue(Redbox.IPC.Framework.Statistics.Instance, null)}";
			context.Messages.Add(item2);
		}
	}
}
