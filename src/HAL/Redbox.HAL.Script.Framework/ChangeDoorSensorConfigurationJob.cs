using System;
using System.IO;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "door-sensor-software-override")]
internal sealed class ChangeDoorSensorConfigurationJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		if (!ControllerConfiguration.Instance.IsVMZMachine)
		{
			AddError("Unsupported door switch configuration change on non-VMZ kiosk.");
			return;
		}
		IDoorSensorService service = ServiceLocator.Instance.GetService<IDoorSensorService>();
		string text = base.Context.PopTop<string>();
		service.SoftwareOverride = text.Equals("off", StringComparison.CurrentCultureIgnoreCase);
		IFormattedLogFactoryService service2 = ServiceLocator.Instance.GetService<IFormattedLogFactoryService>();
		string path = service2.CreateSubpath("Service");
		string path2 = Path.Combine(path, "DoorSensorConfiguration.log");
		using StreamWriter streamWriter = new StreamWriter(path2, append: true);
		string arg = $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}";
		streamWriter.WriteLine(service.SoftwareOverride ? $"{arg} : Door sensors have a software override in place: they are not active - *UNCHECKED MOVEMENT IS POSSIBLE*." : $"{arg} : Door sensors do not have a software override; they are active.");
	}

	internal ChangeDoorSensorConfigurationJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
