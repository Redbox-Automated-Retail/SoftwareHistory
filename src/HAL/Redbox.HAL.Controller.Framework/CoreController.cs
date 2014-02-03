using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

internal class CoreController : IDisposable
{
	public void Dispose()
	{
	}

	internal static CoreResponse ExecuteSingleControllerCommand(CommandType type)
	{
		using CoreController coreController = new CoreController();
		return coreController.SendCommand(new CoreCommand(type, waitForCompletion: false, null, SerialBoard.Instance.Port, null));
	}

	internal static CoreResponse ExecuteSingleWaitableCommand(CommandType type, int timeout)
	{
		using CoreController coreController = new CoreController();
		return coreController.SendCommand(new CoreCommand(type, waitForCompletion: true, timeout, SerialBoard.Instance.Port, null));
	}

	internal static CoreResponse ExecuteCoreCommand(CoreCommand command)
	{
		using CoreController coreController = new CoreController();
		return coreController.SendCommand(command);
	}

	private CoreResponse SendCommand(CoreCommand command)
	{
		if (command.Port == null)
		{
			return CoreResponse.MakeErrorResponse();
		}
		if (!command.Port.Open())
		{
			LogHelper.Instance.Log($"CoreController: unable to open port {command.Port.Name}.", LogEntryType.Error);
			CoreResponse coreResponse = CoreResponse.MakeErrorResponse();
			if (command.Port.Name == ControllerConfiguration.Instance.ControllerPortName)
			{
				coreResponse.CommErrorDiagnostic = "SER board not responsive.";
			}
			return coreResponse;
		}
		try
		{
			return command.Execute();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"CoreController.SendCommands on port {command.Port.Name} caught an exception.", e);
			return CoreResponse.MakeErrorResponse();
		}
	}

	private CoreController()
	{
	}
}
