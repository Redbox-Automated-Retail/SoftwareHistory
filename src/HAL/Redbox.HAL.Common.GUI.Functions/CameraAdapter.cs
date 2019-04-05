using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Redbox.HAL.Client;
using Redbox.HAL.Component.Model;
using Redbox.IPC.Framework;

namespace Redbox.HAL.Common.GUI.Functions;

public sealed class CameraAdapter
{
	private class _SnapResult : ISnapResult
	{
		public bool SnapOk { get; internal set; }

		public string Path { get; internal set; }

		internal _SnapResult()
		{
			SnapOk = false;
			Path = string.Empty;
		}
	}

	private CameraState State;

	private readonly byte[] CameraSnapImmediate;

	private readonly HardwareService Service;

	public bool LegacyCamera { get; private set; }

	public bool CameraInError()
	{
		string text = RunInstruction("CAMERA RETURNERRORCOUNT");
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		int num = 0;
		try
		{
			num = int.Parse(text);
		}
		catch
		{
			num = 0;
		}
		return num > 0;
	}

	public CameraState ToggleState()
	{
		if (State == CameraState.Unknown)
		{
			return State;
		}
		string instruction = ((CameraState.Started == State) ? "CAMERA STOP FORCE=TRUE" : "CAMERA START");
		string value = RunInstruction(instruction);
		if (string.IsNullOrEmpty(value))
		{
			State = CameraState.Unknown;
		}
		else if ("TRUE".Equals(value, StringComparison.CurrentCultureIgnoreCase))
		{
			CameraState state = ((CameraState.Started != State) ? CameraState.Started : CameraState.Stopped);
			State = state;
		}
		return State;
	}

	public CameraState GetCameraStatus()
	{
		if (State == CameraState.Unknown)
		{
			string text = RunInstruction("CAMERA STATUS");
			if (!string.IsNullOrEmpty(text))
			{
				State = ((text == "RUNNING") ? CameraState.Started : CameraState.Stopped);
			}
		}
		return State;
	}

	public void Reset(bool legacy)
	{
		LegacyCamera = legacy;
		State = CameraState.Unknown;
	}

	public ISnapResult Snap()
	{
		HardwareJob val = default(HardwareJob);
		if (!((ClientCommandResult)(object)Service.ExecuteImmediateProgram(CameraSnapImmediate, ref val)).Success)
		{
			return null;
		}
		_SnapResult snapResult = new _SnapResult();
		string topOfStack = HardwareJobExtensions.GetTopOfStack(val);
		if ("SUCCESS" == topOfStack)
		{
			string[] stackEntries = HardwareJobExtensions.GetStackEntries(val, 2);
			snapResult.SnapOk = true;
			snapResult.Path = stackEntries[1];
		}
		return snapResult;
	}

	public bool ResetReturnCounter()
	{
		HardwareJob val = default(HardwareJob);
		return ((ClientCommandResult)(object)Service.ExecuteImmediate("CAMERA RESETRETURNCOUNTER", ref val)).Success;
	}

	public void LaunchAndWaitForTuner()
	{
		string text = "c:\\Program Files\\Redbox\\halservice\\bin\\CameraTuner.exe";
		if (File.Exists(text))
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("CAMERA STOP FORCE=TRUE" + Environment.NewLine);
			stringBuilder.Append("RINGLIGHT ON" + Environment.NewLine);
			HardwareJob val = default(HardwareJob);
			Service.ExecuteImmediateProgram(Encoding.ASCII.GetBytes(stringBuilder.ToString()), ref val);
			ServiceLocator.Instance.GetService<IRuntimeService>().SpinWait(5000);
			Process.Start(text, string.Empty).WaitForExit();
			stringBuilder = new StringBuilder();
			stringBuilder.Append("RINGLIGHT OFF" + Environment.NewLine);
			stringBuilder.Append("CAMERA START" + Environment.NewLine);
			Service.ExecuteImmediateProgram(Encoding.ASCII.GetBytes(stringBuilder.ToString()), ref val);
		}
	}

	public CameraAdapter(HardwareService service)
	{
		Service = service;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("CLEAR");
		stringBuilder.AppendLine(" RINGLIGHT ON");
		stringBuilder.AppendLine(" CAMERA SNAP");
		stringBuilder.AppendLine(" RINGLIGHT OFF");
		CameraSnapImmediate = Encoding.ASCII.GetBytes(stringBuilder.ToString());
		State = CameraState.Unknown;
	}

	private string RunInstruction(string instruction)
	{
		using InstructionHelper instructionHelper = new InstructionHelper(Service);
		return instructionHelper.ExecuteGeneric(instruction, 120000);
	}
}
