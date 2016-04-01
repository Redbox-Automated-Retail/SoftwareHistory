using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "test-boards-job", Operand = "TEST-BOARDS-JOB")]
internal class TestBoardsJob : NativeJobAdapter
{
	private readonly ControlBoards[] Boards = new ControlBoards[3]
	{
		ControlBoards.Serial,
		ControlBoards.Aux,
		ControlBoards.Picker
	};

	protected override void ExecuteInner()
	{
		int num = 0;
		ControlBoards[] boards = Boards;
		foreach (ControlBoards controlBoards in boards)
		{
			string boardVersion = base.ControlSystem.GetBoardVersion(controlBoards);
			string arg = "responsive";
			if (boardVersion == "TIMEOUT")
			{
				num++;
				arg = "not responsive";
			}
			base.Context.CreateInfoResult("VersionResult", $"{controlBoards.ToString().ToUpper()} board {arg}");
		}
		if (num > 0)
		{
			AddError("One or more comm failures.");
		}
	}

	internal TestBoardsJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
