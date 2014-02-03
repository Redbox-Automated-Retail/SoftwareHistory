using System.Diagnostics;
using System.IO;
using Redbox.HAL.Camera.Framework;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal class InitJob : NativeJobAdapter
{
	private const int MinPortWakeupTime = 60000;

	private static int? m_emulatedInitWaitTime = null;

	private static int EmulatedInitWaitTime
	{
		get
		{
			if (m_emulatedInitWaitTime.HasValue)
			{
				return m_emulatedInitWaitTime.Value;
			}
			m_emulatedInitWaitTime = RegistryOptions.Instance.GetSecretValue<int?>("SOFTWARE\\Redbox\\HAL", "EmuInitWait");
			if (!m_emulatedInitWaitTime.HasValue)
			{
				m_emulatedInitWaitTime = 10000;
			}
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
			return;
		}
		applicationLog.Write("RESET controller boards.");
		if (base.Context.ID == ExecutionEngine.Instance.InitJob.ID)
		{
			int num = ControllerConfiguration.Instance.COMPortInitSleepTime;
			if (num < 60000)
			{
				num = 60000;
			}
			PerfFunctions.Wait(num);
			LogHelper.Instance.Log("Wait for COM ports complete.", LogEntryType.Info);
		}
		if (instance.Reset())
		{
			applicationLog.Write("Reset of the boards timed out.");
			LogHelper.Instance.Log("RESET instruction returned timeout; maybe communication error?", LogEntryType.Error);
			SendMessage("RESET instruction returned timeout.");
			ResultHelper.Instance.CreateInfoResult("RESET", "RESET instruction timed out.");
			AddError("RESET instruction returned timeout.");
			return;
		}
		try
		{
			Platform platform = RuntimeService.Instance.GetPlatform();
			Process cameraTuner = null;
			if (platform != Platform.Windows7 && base.Context.ID == ExecutionEngine.Instance.InitJob.ID && ControllerConfiguration.Instance.FixCameraOnInit2)
			{
				string fileName = Path.Combine(RuntimeService.Instance.AssemblyDirectory, "CameraTuner.exe");
				cameraTuner = Process.Start(fileName, "-snaptest");
			}
			applicationLog.Write("RESET completed successfully; get controller board VERSION stamps.");
			SendMessage("GET VERSION INFO");
			string machineRevision = instance.GetMachineRevision(out var versions);
			if (versions[versions.Length - 1] == "TIMEOUT")
			{
				applicationLog.Write("The Version instruction timed out.");
				LogHelper.Instance.Log("VERSION instruction returned timeout.", LogEntryType.Error);
				SendMessage("VERSION instruction returned timeout.");
				ResultHelper.Instance.CreateInfoResult("VERSION", "VERSION instruction timed out.");
				AddError("VERSION instruction returned timeout.");
				return;
			}
			for (int num2 = versions.Length - 1; num2 >= 0; num2--)
			{
				if (versions[num2] != null)
				{
					ResultHelper.Instance.CreateInfoResult("VERSION INFO", versions[num2].Trim());
				}
			}
			ResultHelper.Instance.CreateInfoResult("VERSION INFO", machineRevision);
			applicationLog.Write("Deactivate audio, close track, close vend door");
			SendMessage("SET START STATE");
			instance.SetAudio(on: false);
			SendMessage("TEST TRACK");
			Track.Instance.Open();
			PerfFunctions.Wait(200);
			Track.Instance.Close();
			PerfFunctions.Wait(200);
			if (base.Track.Open())
			{
				applicationLog.Write("TRACK OPEN timed out.");
				ResultHelper.Instance.CreateInfoResult("TRACK OPEN FAILED", "The TRACK OPEN instruction timed out.");
				AddError("TRACK OPEN instruction failed.");
				return;
			}
			PerfFunctions.Wait(200);
			if (base.Track.Close())
			{
				applicationLog.Write("TRACK CLOSE timed out.");
				ResultHelper.Instance.CreateInfoResult("TRACK CLOSE FAILED", "The TRACK CLOSE instruction timed out.");
				AddError("TRACK CLOSE instruction failed.");
				return;
			}
			PerfFunctions.Wait(200);
			applicationLog.Write("Ensure the gripper is not obstructed.");
			SendMessage("CHECK OBSTRUCTIONS");
			if (!instance.IsGripperClear())
			{
				if (instance.Location.IsValid)
				{
					ResultHelper.Instance.CreateResult(StackEnd.Top, "PICKER OBSTRUCTED", "The picker is obstructed and cannot be moved.", instance.Location.Deck, instance.Location.Slot, null, null, null);
				}
				string text = "The picker is obstructed and cannot be moved.";
				applicationLog.Write(text);
				LogHelper.Instance.Log(text, LogEntryType.Error);
				AddError(text);
				return;
			}
			using (MotionController motionController = MotionController.GetController())
			{
				using HomeAxesResult homeAxesResult = motionController.InitAxes(applicationLog);
				if (!homeAxesResult.Succeded)
				{
					foreach (Error error in homeAxesResult.ErrorList)
					{
						base.Result.Errors.Add(error);
						ResultHelper.Instance.CreateInfoResult("InitError", error.Details);
					}
					return;
				}
			}
			HandleCameraStartup(cameraTuner);
			bool flag = CameraService.Instance.Start();
			string text2 = $"CAMERA START returned {flag}";
			applicationLog.Write(text2);
			LogHelper.Instance.Log(text2, LogEntryType.Info);
			PerfFunctions.Wait(500);
			PickerStatus pickerStatus = HelperFunctions.CheckPicker(base.Result, null);
			if (pickerStatus != PickerStatus.Empty)
			{
				AddError("Failed to put disk away.");
				return;
			}
			applicationLog.Write("Move to the vend position.");
			SendMessage("MOVE TO VEND");
			MotionResult motionResult = instance.MoveToVend(MoveMode.Get, base.AppLog);
			if (motionResult != MotionResult.Success)
			{
				ResultHelper.Instance.CreateInfoResult(StackEnd.Top, motionResult.ToString().ToUpper(), "There was an error executing the MOVEVEND instruction.");
				AddError("The MOVEVEND instruction failed.");
			}
		}
		finally
		{
			CameraService.Instance.Start();
			applicationLog.Write("Initialization complete.");
			SendMessage("INIT STOP");
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
}
