using System;
using Redbox.HAL.Configuration;
using Redbox.HAL.Core;
using Redbox.IPC.Framework;

namespace Redbox.HAL.IPC.Framework;

[Command("script")]
public class ScriptCommand
{
	[Usage("SCRIPT execute path: 'C:\\temp\\my-script.txt'")]
	[CommandForm(Name = "execute")]
	public void Execute(CommandContext context, [CommandKeyValue(IsRequired = true)] string path)
	{
		try
		{
			ErrorList errorList = new ErrorList();
			BatchCommandRunner.ExecuteFile(path, errorList);
		}
		catch (Exception e)
		{
			context.Errors.Add(Error.NewError("S999", "An unhandled exception was raised in ScriptCommmand.Execute.", e));
		}
	}
}
