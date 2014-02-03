using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Command;

[Command("queue")]
internal class QueueCommand
{
	[Usage("queue enqueue label: entry:")]
	[CommandForm(Name = "enqueue")]
	public void Enqueue(CommandContext context, [CommandKeyValue(IsRequired = true)] string label, [CommandKeyValue(IsRequired = true)] string entry)
	{
		IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
		service.Enqueue(label, StringExtensions.ToObject<Dictionary<string, object>>(entry));
	}

	[CommandForm(Name = "dequeue")]
	[Usage("queue dequeue label:")]
	public void Dequeue(CommandContext context, [CommandKeyValue(IsRequired = true)] string label)
	{
		IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
		service.Dequeue(label);
	}

	[CommandForm(Name = "peek")]
	[Usage("queue peek label:")]
	public void Peek(CommandContext context, [CommandKeyValue(IsRequired = true)] string label)
	{
		IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
		string text = service.PeekRaw(label);
		if (text != null)
		{
			context.Messages.Add(text.Replace(System.Environment.NewLine, ""));
		}
	}
}
