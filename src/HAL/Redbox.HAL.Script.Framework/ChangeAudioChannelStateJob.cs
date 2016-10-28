using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "change-audio-channel-state", Operand = "CHANGE-AUDIO-CHANNEL-STATE")]
internal sealed class ChangeAudioChannelStateJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		AudioChannelState audioChannelState = base.Context.PopTop<AudioChannelState>();
		IControlResponse controlResponse = base.ControlSystem.SetAudio(audioChannelState);
		if (controlResponse.Success)
		{
			base.Context.CreateInfoResult("AudioChangeSuccessful", $"Changed audio channel state to {audioChannelState.ToString().ToUpper()}");
			return;
		}
		base.Context.CreateInfoResult("AudioChangeFailure", "Failed to change channel state.");
		AddError("STATE CHANGE FAILURE");
	}

	internal ChangeAudioChannelStateJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
