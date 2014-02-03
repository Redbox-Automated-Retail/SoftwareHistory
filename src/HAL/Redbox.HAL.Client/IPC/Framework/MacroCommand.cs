using Redbox.HAL.Core;
using Redbox.IPC.Framework;

namespace Redbox.HAL.IPC.Framework;

[Command("macro")]
internal class MacroCommand
{
	[Usage("MACRO list")]
	[CommandForm(Name = "list")]
	public void List(CommandContext context)
	{
		foreach (string key in MacroContainer.Instance.Values.Keys)
		{
			string item = $"{key} expands to '{MacroContainer.Instance.Values[key]}'.";
			context.Messages.Add(item);
		}
	}

	[Usage("MACRO create symbol value: 'expansion'")]
	[CommandForm(Name = "create")]
	public void Create(CommandContext context, [CommandKeyValue(IsRequired = true)] string value)
	{
		context.ForEachSymbolDo(delegate(string each)
		{
			MacroContainer.Instance.Values[each] = value;
		}, new string[3] { "list", "create", "delete" });
	}

	[Usage("MACRO delete symbol ...")]
	[CommandForm(Name = "delete")]
	public void Delete(CommandContext context)
	{
		context.ForEachSymbolDo(delegate(string each)
		{
			MacroContainer.Instance.Values.Remove(each);
		}, new string[3] { "list", "create", "delete" });
	}
}
