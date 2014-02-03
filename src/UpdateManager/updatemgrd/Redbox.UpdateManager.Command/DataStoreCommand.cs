using System;
using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Command;

[Command("data")]
internal class DataStoreCommand
{
	[CommandForm(Name = "set")]
	[Usage("data set id: o:")]
	public void Set(CommandContext context, [CommandKeyValue(IsRequired = true)] string id, [CommandKeyValue(IsRequired = true)] string o)
	{
		IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
		service.SetRaw(id, o);
	}

	[CommandForm(Name = "get")]
	[Usage("data get id:")]
	public void Get(CommandContext context, [CommandKeyValue(IsRequired = true)] string id)
	{
		IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
		string raw = service.GetRaw(id);
		if (raw != null)
		{
			context.Messages.Add(raw.Replace(System.Environment.NewLine, ""));
		}
	}

	[CommandForm(Name = "delete")]
	[Usage("data delete id:")]
	public void Delete(CommandContext context, [CommandKeyValue(IsRequired = true)] string id)
	{
		IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
		service.Delete(id);
	}

	[CommandForm(Name = "reset")]
	[Usage("data reset")]
	public void Reset(CommandContext context)
	{
		IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
		service.Reset();
	}

	[Usage("data cleanup [extension:]")]
	[CommandForm(Name = "cleanup")]
	public void CleanUp(CommandContext context, [CommandKeyValue(IsRequired = false)] string extension)
	{
		IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
		if (string.IsNullOrEmpty(extension))
		{
			service.CleanUp();
		}
		else
		{
			service.CleanUp(extension);
		}
	}
}
