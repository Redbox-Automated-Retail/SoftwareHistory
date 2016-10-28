using System;
using Redbox.Core;

namespace Redbox.IPC.Framework;

[Redbox_002EIPC_002EFramework1802119_002ECommand("script", Filter = "System|Script")]
public class ScriptCommand
{
	[Redbox_002EIPC_002EFramework1802119_002ECommandForm(Name = "execute")]
	[Usage("SCRIPT execute path: 'C:\\temp\\my-script.txt'")]
	public void Execute(CommandContext context, [CommandKeyValue(IsRequired = true)] string path)
	{
		try
		{
			BatchCommandRunner.ExecuteFile(path);
		}
		catch (Exception e)
		{
			context.Errors.Add(Error.NewError("S999", "An unhandled exception was raised in ScriptCommmand.Execute.", e));
		}
	}
}
