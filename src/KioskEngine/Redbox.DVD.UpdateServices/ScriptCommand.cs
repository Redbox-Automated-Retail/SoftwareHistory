using System;
using Redbox.Core;

namespace Redbox.IPC.Framework;

[Redbox.IPC.Framework.Command("script")]
internal class ScriptCommand
{
	[Redbox.IPC.Framework.CommandForm(Name = "execute")]
	[Usage("SCRIPT execute path: 'C:\\temp\\my-script.txt'")]
	public void Execute(Redbox.IPC.Framework.CommandContext context, [Redbox.IPC.Framework.CommandKeyValue(IsRequired = true)] string path)
	{
		try
		{
			Redbox.IPC.Framework.BatchCommandRunner.ExecuteFile(path);
		}
		catch (Exception e)
		{
			context.Errors.Add(Error.NewError("S999", "An unhandled exception was raised in ScriptCommmand.Execute.", e));
		}
	}
}
