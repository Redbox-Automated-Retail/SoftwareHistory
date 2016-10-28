using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Configuration;
using Redbox.IPC.Framework;

namespace Redbox.HAL.IPC.Framework;

[Command("script")]
internal class ScriptCommand
{
	[CommandForm(Name = "execute")]
	[Usage("SCRIPT execute path: 'C:\\temp\\my-script.txt'")]
	public void Execute(CommandContext context, [CommandKeyValue(IsRequired = true)] string path)
	{
		try
		{
			Redbox.HAL.Component.Model.ErrorList errorList = new Redbox.HAL.Component.Model.ErrorList();
			BatchCommandRunner.ExecuteFile(path, errorList);
		}
		catch (Exception e)
		{
			context.Errors.Add(Redbox.HAL.Component.Model.Error.NewError("S999", "An unhandled exception was raised in ScriptCommmand.Execute.", e));
		}
	}
}
