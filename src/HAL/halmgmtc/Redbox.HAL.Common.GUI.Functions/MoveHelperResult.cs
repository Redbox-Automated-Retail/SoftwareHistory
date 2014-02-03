namespace Redbox.HAL.Common.GUI.Functions;

public class MoveHelperResult
{
	public string MoveResultMessage { get; internal set; }

	public string[] Limits { get; internal set; }

	public bool MoveOk => MoveResultMessage == "SUCCESS";

	public bool Timeout => MoveResultMessage == "TIMEOUT";

	internal MoveHelperResult()
	{
		MoveResultMessage = "SUCCESS";
		Limits = new string[0];
	}
}
