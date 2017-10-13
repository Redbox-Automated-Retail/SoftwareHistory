using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class BoardVersionResponse : IBoardVersionResponse
{
	public bool ReadSuccess { get; private set; }

	public string Version { get; private set; }

	public string BoardName { get; private set; }

	internal BoardVersionResponse(CoreResponse response)
	{
		ReadSuccess = response.Success;
		Version = (ReadSuccess ? response.OpCodeResponse.Trim() : string.Empty);
	}

	internal BoardVersionResponse(ControlBoards board)
	{
		ReadSuccess = true;
		BoardName = board.ToString();
		Version = $"Emulated {board.ToString()} version.";
	}
}
