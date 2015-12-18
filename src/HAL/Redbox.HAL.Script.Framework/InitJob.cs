using System.Diagnostics;
using System.IO;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;
using Redbox.HAL.DataMatrix.Framework;
using Redbox.HAL.DataStorage;

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
			IPersistentMap dataStore = StoreManager.Instance.GetDataStore();
			m_emulatedInitWaitTime = dataStore.GetValue("EmuInitWait", 10000);
			return m_emulatedInitWaitTime.Value;
		}
	}

	protected override void ExecuteInner()
	{
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: false, "init", flushOnWrite: true, null);
		ControllerService instance = ControllerService.Instance;
		applicationLog.Write("Init start.");
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			PerfFunctions.Wait(EmulatedInitWaitTime);
			if (base.Context.ID == ExecutionEngine.Instance.InitJob.ID)
			{
				string text = Path.Combine(RuntimeService.Instance.AssemblyDirectory, "CameraTuner.exe");
				if (File.Exists(text))
				{
					Process cameraTuner = Process.Start(text, "-snaptest");
					PerfFunctions.Wait(60000);
					HandleCameraStartup(cameraTuner);
				}
			}
			IScannerDevice configuredDevice = ScannerDeviceFactory.GetConfiguredDevice();
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
		if (instance.Reset())
		{
			applicationLog.Write("Reset of the boards timed out.");
			LogHelper.Instance.Log("RESET instruction returned timeout; maybe communication error?", LogEntryType.Error);
			base.Context.Send("RESET instruction returned timeout.");
			base.Context.CreateInfoResult("RESET", "RESET instruction timed out.");
			AddError("RESET instruction returned timeout.");
			return;
		}
		try
		{
			Process cameraTuner2 = null;
			if (base.Context.ID == ExecutionEngine.Instance.InitJob.ID && TryToFixCamera())
			{
				LogHelper.Instance.Log($"[Init, {base.Context.ID}]:Start camera check process.");
				string text3 = Path.Combine(RuntimeService.Instance.AssemblyDirectory, "CameraTuner.exe");
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
			string boardVersions = ControlSystem.Instance.GetBoardVersions(out var versions);
			if (versions[versions.Length - 1] == "TIMEOUT")
			{
				applicationLog.Write("The Version instruction timed out.");
				LogHelper.Instance.Log("VERSION instruction returned timeout.", LogEntryType.Error);
				base.Context.Send("VERSION instruction returned timeout.");
				base.Context.CreateInfoResult("VERSION", "VERSION instruction timed out.");
				AddError("VERSION instruction returned timeout.");
				return;
			}
			for (int num = versions.Length - 1; num >= 0; num--)
			{
				base.Context.CreateInfoResult("VERSION INFO", versions[num].Trim());
			}
			base.Context.CreateInfoResult("VERSION INFO", boardVersions);
			applicationLog.Write("Deactivate audio, close track, close vend door");
			base.Context.Send("SET START STATE");
			ControlSystem.Instance.ExecuteControllerCommand(CommandType.AudioOff);
			base.Context.Send("TEST TRACK");
			Track.Open();
			PerfFunctions.Wait(200);
			Track.Close();
			PerfFunctions.Wait(200);
			if (Track.Open())
			{
				applicationLog.Write("TRACK OPEN timed out.");
				base.Context.CreateInfoResult("TRACK OPEN FAILED", "The TRACK OPEN instruction timed out.");
				AddError("TRACK OPEN instruction failed.");
				return;
			}
			PerfFunctions.Wait(200);
			if (Track.Close())
			{
				applicationLog.Write("TRACK CLOSE timed out.");
				base.Context.CreateInfoResult("TRACK CLOSE FAILED", "The TRACK CLOSE instruction timed out.");
				AddError("TRACK CLOSE instruction failed.");
				return;
			}
			PerfFunctions.Wait(200);
			applicationLog.Write("Ensure the gripper is not obstructed.");
			base.Context.Send("CHECK OBSTRUCTIONS");
			if (!Gripper.IsClear())
			{
				if (instance.Location.IsValid)
				{
					base.Context.CreateResult(StackEnd.Top, "PICKER OBSTRUCTED", "The picker is obstructed and cannot be moved.", instance.Location.Deck, instance.Location.Slot, null, null, null);
				}
				string text4 = "The picker is obstructed and cannot be moved.";
				applicationLog.Write(text4);
				LogHelper.Instance.Log(text4, LogEntryType.Error);
				AddError(text4);
				return;
			}
			MotionController controller = MotionController.GetController();
			using (HomeAxesResult homeAxesResult = controller.InitAxes(applicationLog))
			{
				if (!homeAxesResult.Succeded)
				{
					foreach (Error error in homeAxesResult.ErrorList)
					{
						base.Result.Errors.Add(error);
						base.Context.CreateInfoResult("InitError", error.Details);
					}
					return;
				}
			}
			HandleCameraStartup(cameraTuner2);
			StartCamera();
			PickerStatus pickerStatus = HelperFunctions.CheckPicker(base.Result, base.Context, null);
			if (pickerStatus != PickerStatus.Empty)
			{
				AddError("Failed to put disk away.");
				return;
			}
			applicationLog.Write("Move to the vend position.");
			base.Context.Send("MOVE TO VEND");
			MotionResult motionResult = instance.MoveToVend(MoveMode.Get, base.AppLog);
			if (motionResult != MotionResult.Success)
			{
				base.Context.CreateInfoResult(StackEnd.Top, motionResult.ToString().ToUpper(), "There was an error executing the MOVEVEND instruction.");
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
			IScannerDevice configuredDevice = ScannerDeviceFactory.GetConfiguredDevice();
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
			ProcessHelper.KillProcess("CameraTuner");
			num = -1;
		}
		else
		{
			LogHelper.Instance.Log($"Camera tuner exited with code {cameraTuner.ExitCode}", LogEntryType.Info);
			num = cameraTuner.ExitCode;
		}
		try
		{
			if (num == 0)
			{
				return;
			}
			LogHelper.Instance.Log("No snapshot was generated from tuner; reset camera.", LogEntryType.Info);
			using DeviceHelper helper = new DeviceHelper(debug: false);
			CameraInfo cameraInfo = new CameraInfo(helper);
			if (cameraInfo.FindActiveCamera())
			{
				LogHelper.Instance.Log("Found camera {0}", cameraInfo.Revision.ToString());
				if (cameraInfo.Revision == KnownCameras.ThirdGen || cameraInfo.Revision == KnownCameras.FourthGen)
				{
					LogHelper.Instance.Log("The running driver for the camera {0}", cameraInfo.CorrectDriverRunning() ? "is correct" : "is not correct.");
				}
				if (DeviceStatus.Enabled == cameraInfo.Status)
				{
					bool flag = cameraInfo.ResetDriver();
					LogHelper.Instance.Log(" RESET camera returned {0}", flag ? "OK" : "FAILURE");
				}
				else if (DeviceStatus.Disabled == cameraInfo.Status && ControllerConfiguration.Instance.CheckGen4CameraOnInit)
				{
					LogHelper.Instance.Log("The camera appears not to be enabled.");
					bool flag2 = true;
					if (KnownCameras.FourthGen == cameraInfo.Revision)
					{
						flag2 = cameraInfo.CorrectDriverRunning();
						LogHelper.Instance.Log("Driver {0} for camera {1}", flag2 ? "matched" : "didn't match", cameraInfo.Revision.ToString());
					}
					if (flag2)
					{
						cameraInfo.SetCameraState(DeviceState.Enable);
						PerfFunctions.SpinWait(3000);
					}
				}
			}
			else
			{
				LogHelper.Instance.Log("Unable to find any active cameras in the device list.", LogEntryType.Error);
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
		if (BarcodeConfiguration.Instance.ScannerService == ScannerServices.Cortex)
		{
			return false;
		}
		IPersistentMap dataStore = StoreManager.Instance.GetDataStore();
		if (dataStore.GetValue("FixCamOnWES7", def: false))
		{
			return ControllerConfiguration.Instance.FixCameraOnInit2;
		}
		return false;
	}
}
