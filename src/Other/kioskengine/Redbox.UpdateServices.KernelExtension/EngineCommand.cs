using System;
using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.UpdateServices.KernelExtension;

[Redbox_002EIPC_002EFramework1769326_002ECommand("engine")]
public class EngineCommand
{
	[Redbox_002EIPC_002EFramework1769326_002ECommandForm(Name = "ping")]
	[Usage("engine ping")]
	public void Ping(CommandContext context)
	{
		try
		{
			LogHelper.Instance.Log("Engine Command: Ping");
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Error in EngineCommand Ping.", e);
		}
	}

	[Redbox_002EIPC_002EFramework1769326_002ECommandForm(Name = "shutdown")]
	[Usage("engine shutdown")]
	public void ShutDown(CommandContext context)
	{
		try
		{
			LogHelper.Instance.Log("Engine Command: ShutDownEngine");
			IEngineApplication service = ServiceLocator.Instance.GetService<IEngineApplication>();
			if (service != null)
			{
				if (service.CanShutDown())
				{
					service.ShutDown();
				}
				else
				{
					service.FlagForSafeShutdown();
				}
			}
			else
			{
				LogHelper.Instance.Log("Unable to locate EngineApplication service");
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Error in EngineCommand ShutDownEngine.", e);
		}
	}

	[Redbox_002EIPC_002EFramework1769326_002ECommandForm(Name = "reset-bundle")]
	[Usage("engine reset-bundle")]
	internal static void ResetBundle(string[] parms)
	{
		try
		{
			LogHelper.Instance.Log("Engine Command: reset-bundle");
			IEngineApplication service = ServiceLocator.Instance.GetService<IEngineApplication>();
			if (service != null)
			{
				if (service.CanRestart())
				{
					LogHelper.Instance.Log("Resetting Bundle...");
					IResourceBundleService service2 = ServiceLocator.Instance.GetService<IResourceBundleService>();
					if (service2 != null)
					{
						service2.Restart();
					}
					else
					{
						LogHelper.Instance.Log("Unable to locate ResourceBundle service");
					}
				}
				else
				{
					LogHelper.Instance.Log("CanRestart() Returned False; Not Restarting Bundle.");
				}
			}
			else
			{
				LogHelper.Instance.Log("Unable to locate EngineApplication service");
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Error in EngineCommand ResetBundle.", e);
		}
	}
}
