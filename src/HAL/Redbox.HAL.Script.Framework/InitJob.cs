using System;
using System.Diagnostics;
using System.IO;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "init", Operand = "INIT")]
internal sealed class InitJob : NativeJobAdapter
{
	private bool CameraStarted;

	private static int? m_emulatedInitWaitTime = null;

	private static int EmulatedInitWaitTime
	{
		get
		{
			if (m_emulatedInitWaitTime.HasValue)
			{
				return m_emulatedInitWaitTime.Value;
			}
			IPersistentMap map = ServiceLocator.Instance.GetService<IPersistentMapService>().GetMap();
			m_emulatedInitWaitTime = map.GetValue("EmuInitWait", 10000);
			return m_emulatedInitWaitTime.Value;
		}
	}

	protected override void ExecuteInner()
	{
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: false, "init", flushOnWrite: true, null);
		_ = ControllerService.Instance;
		applicationLog.Write("Init start.");
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			PerfFunctions.Wait(EmulatedInitWaitTime);
			if (base.Context.ID == ExecutionEngine.Instance.InitJob.ID)
			{
				string text = service.RuntimePath("CameraTuner.exe");
				if (File.Exists(text))
				{
					Process cameraTuner = Process.Start(text, "-snaptest");
					PerfFunctions.Wait(60000);
					HandleCameraStartup(cameraTuner);
				}
			}
			IScannerDevice configuredDevice = ServiceLocator.Instance.GetService<IScannerDeviceService>().GetConfiguredDevice();
			bool flag = configuredDevice.Start();
			string text2 = $"CAMERA START returned {flag}";
			applicationLog.Write(text2);
			LogHelper.Instance.Log(text2, LogEntryType.Info);
			return;
		}
		applicationLog.Write("RESET controller boards.");
		if (base.Context.ID == ExecutionEngine.Instance.InitJob.ID)
		{
			PerfFunctions.Wait(ControllerConfiguration.Instance.MinInitWakeupTime);
			LogHelper.Instance.Log("Wait for COM ports complete.", LogEntryType.Info);
		}
		if (base.ControlSystem.IsInitialized)
		{
			base.ControlSystem.Shutdown();
			PerfFunctions.Wait(2000);
		}
		IControlResponse controlResponse = base.ControlSystem.Initialize();
		if (!controlResponse.Success)
		{
			applicationLog.Write("Reset of the boards timed out.");
			LogHelper.Instance.Log("RESET instruction returned timeout; maybe communication error?", LogEntryType.Error);
			base.Context.Send("RESET instruction returned timeout.");
			base.Context.CreateInfoResult("RESET", "RESET instruction timed out.");
			AddError("RESET instruction returned timeout.");
			return;
		}
		IMotionControlService service2 = ServiceLocator.Instance.GetService<IMotionControlService>();
		service2.Initialize();
		try
		{
			Process cameraTuner2 = null;
			if (base.Context.ID == ExecutionEngine.Instance.InitJob.ID && TryToFixCamera())
			{
				LogHelper.Instance.Log($"[Init, {base.Context.ID}]:Start camera check process.");
				string text3 = service.RuntimePath("CameraTuner.exe");
				if (File.Exists(text3))
				{
					cameraTuner2 = Process.Start(text3, "-snaptest");
				}
				else
				{
					LogHelper.Instance.Log("The CameraTuner program doesn't exist.", LogEntryType.Error);
				}
			}
			applicationLog.Write("RESET completed successfully; get controller board VERSION stamps.");
			base.Context.Send("GET VERSION INFO");
			string[] boardVersions = base.ControlSystem.GetBoardVersions();
			if (boardVersions[boardVersions.Length - 1] == "TIMEOUT")
			{
				applicationLog.Write("The Version instruction timed out.");
				LogHelper.Instance.Log("VERSION instruction returned timeout.", LogEntryType.Error);
				base.Context.Send("VERSION instruction returned timeout.");
				base.Context.CreateInfoResult("VERSION", "VERSION instruction timed out.");
				AddError("VERSION instruction returned timeout.");
				return;
			}
			for (int num = boardVersions.Length - 1; num >= 0; num--)
			{
				base.Context.CreateInfoResult("VERSION INFO", boardVersions[num].Trim());
			}
			base.Context.CreateInfoResult("VERSION INFO", "A");
			base.Context.Send("TEST TRACK");
			base.ControlSystem.TrackOpen();
			PerfFunctions.Wait(200);
			base.ControlSystem.TrackClose();
			PerfFunctions.Wait(200);
			if (!base.ControlSystem.TrackOpen().Success)
			{
				applicationLog.Write("TRACK OPEN timed out.");
				base.Context.CreateInfoResult("TRACK OPEN FAILED", "The TRACK OPEN instruction timed out.");
				AddError("TRACK OPEN instruction failed.");
				return;
			}
			PerfFunctions.Wait(200);
			if (!base.ControlSystem.TrackClose().Success)
			{
				applicationLog.Write("TRACK CLOSE timed out.");
				base.Context.CreateInfoResult("TRACK CLOSE FAILED", "The TRACK CLOSE instruction timed out.");
				AddError("TRACK CLOSE instruction failed.");
				return;
			}
			PerfFunctions.Wait(200);
			applicationLog.Write("Ensure the gripper is not obstructed.");
			base.Context.Send("CHECK OBSTRUCTIONS");
			using (ClearGripperOperation clearGripperOperation = new ClearGripperOperation())
			{
				ErrorCodes errorCodes = clearGripperOperation.Execute();
				if (errorCodes != ErrorCodes.Success)
				{
					if (base.MotionService.CurrentLocation != null)
					{
						base.Context.CreateResult(StackEnd.Top, "PICKER OBSTRUCTED", "The picker is obstructed and cannot be moved.", base.MotionService.CurrentLocation.Deck, base.MotionService.CurrentLocation.Slot, null, null, null);
					}
					string text4 = $"The picker is obstructed and cannot be moved; clear gripper returned {errorCodes.ToString()}.";
					applicationLog.Write(text4);
					LogHelper.Instance.Log(text4, LogEntryType.Error);
					AddError(text4);
					return;
				}
			}
			ErrorCodes errorCodes2 = base.MotionService.InitAxes(applicationLog);
			if (errorCodes2 != ErrorCodes.Success)
			{
				string text5 = $"Initializing the axes failed with code {errorCodes2.ToString()}";
				base.Result.Errors.Add(Error.NewError("E999", "Init axes error.", text5));
				applicationLog.WriteFormatted(text5, LogEntryType.Error);
				base.Context.CreateInfoResult("InitError", text5);
				return;
			}
			HandleCameraStartup(cameraTuner2);
			StartCamera();
			ErrorCodes errorCodes3 = HelperFunctions.CheckPicker(base.Result, base.Context, null);
			if (errorCodes3 != ErrorCodes.PickerEmpty)
			{
				AddError("Failed to put disk away.");
				return;
			}
			applicationLog.Write("Move to the vend position.");
			base.Context.Send("MOVE TO VEND");
			ErrorCodes errorCodes4 = base.MotionService.MoveVend(MoveMode.Get, base.AppLog);
			if (errorCodes4 != ErrorCodes.Success)
			{
				base.Context.CreateInfoResult(StackEnd.Top, errorCodes4.ToString().ToUpper(), "There was an error executing the MOVEVEND instruction.");
				AddError("The MOVEVEND instruction failed.");
			}
		}
		finally
		{
			StartCamera();
			applicationLog.Write("Initialization complete.");
			base.Context.Send("INIT STOP");
		}
	}

	private void StartCamera()
	{
		if (!CameraStarted)
		{
			CameraStarted = true;
			IScannerDevice configuredDevice = ServiceLocator.Instance.GetService<IScannerDeviceService>().GetConfiguredDevice();
			bool flag = configuredDevice.Start();
			string text = $"CAMERA START returned {flag}";
			base.AppLog.Write(text);
			LogHelper.Instance.Log(text, LogEntryType.Info);
		}
	}

	private void HandleCameraStartup(Process cameraTuner)
	{
		if (cameraTuner == null)
		{
			return;
		}
		int num = 0;
		if (!cameraTuner.WaitForExit(10000))
		{
			cameraTuner.Kill();
			Process[] processesByName = Process.GetProcessesByName("CameraTuner");
			if (processesByName.Length > 0)
			{
				Process[] array = processesByName;
				foreach (Process process in array)
				{
					try
					{
						process.Kill();
					}
					catch
					{
					}
				}
			}
			num = -1;
		}
		else
		{
			LogHelper.Instance.Log($"Camera tuner exited with code {cameraTuner.ExitCode}", LogEntryType.Info);
			num = cameraTuner.ExitCode;
		}
		try
		{
			if (num != 0)
			{
				LogHelper.Instance.Log("No snapshot was generated from tuner; reset camera.", LogEntryType.Info);
				IScannerDeviceService service = ServiceLocator.Instance.GetService<IScannerDeviceService>();
				IScannerDevice configuredDevice = service.GetConfiguredDevice();
				configuredDevice.Restart();
			}
		}
		finally
		{
			cameraTuner.Dispose();
		}
	}

	internal InitJob(ExecutionResult r, ExecutionContext c)
		: base(r, c)
	{
	}

	private bool TryToFixCamera()
	{
		IScannerDeviceService service = ServiceLocator.Instance.GetService<IScannerDeviceService>();
		IScannerDevice configuredDevice = service.GetConfiguredDevice();
		if (!configuredDevice.Name.Equals("legacy", StringComparison.CurrentCultureIgnoreCase))
		{
			return false;
		}
		IPersistentMap map = ServiceLocator.Instance.GetService<IPersistentMapService>().GetMap();
		if (map.GetValue("FixCamOnWES7", def: false))
		{
			return ControllerConfiguration.Instance.FixCameraOnInit2;
		}
		return false;
	}
}
