using Redbox.Core;

namespace Redbox.IPC.Framework;

[Redbox.IPC.Framework.Command("macro")]
internal class MacroCommand
{
	[Redbox.IPC.Framework.CommandForm(Name = "list")]
	[Usage("MACRO list")]
	public void List(Redbox.IPC.Framework.CommandContext context)
	{
		foreach (string key in Redbox.IPC.Framework.CommandService.Instance.Properties.Keys)
		{
			string item = $"{key} expands to '{Redbox.IPC.Framework.CommandService.Instance.Properties[key]}'.";
			context.Messages.Add(item);
		}
	}

	[Redbox.IPC.Framework.CommandForm(Name = "create")]
	[Usage("MACRO create symbol value: 'expansion'")]
	public void Create(Redbox.IPC.Framework.CommandContext context, [Redbox.IPC.Framework.CommandKeyValue(IsRequired = true)] string value)
	{
		context.ForEachSymbolDo(delegate(string each)
		{
			Redbox.IPC.Framework.CommandService.Instance.Properties[each] = value;
		}, new string[3] { "list", "create", "delete" });
	}

	[Redbox.IPC.Framework.CommandForm(Name = "delete")]
	[Usage("MACRO delete symbol ...")]
	public void Delete(Redbox.IPC.Framework.CommandContext context)
	{
		context.ForEachSymbolDo(delegate(string each)
		{
			Redbox.IPC.Framework.CommandService.Instance.Properties.Remove(each);
		}, new string[3] { "list", "create", "delete" });
	}
}
