using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Command;

[Command("configfile")]
internal class ConfigFileCommand
{
	[Usage("configfile activate configfileid:")]
	[CommandForm(Name = "activate")]
	public void ActivateConfigFile(CommandContext context, [CommandKeyValue(IsRequired = true)] long configfileid)
	{
		IConfigFileService service = ServiceLocator.Instance.GetService<IConfigFileService>();
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = service.ActivateConfigFile(configfileid);
		if (errorList.ContainsError())
		{
			context.Errors.AddRange(errorList.ToIPCErrors());
		}
	}
}
