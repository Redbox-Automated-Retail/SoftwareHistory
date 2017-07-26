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
		for (int i = 0; i < boards.Length; i++)
		{
			ControlBoards board = boards[i];
			string boardVersion = base.ControlSystem.GetBoardVersion(board);
			string arg = "responsive";
			if (boardVersion == "TIMEOUT")
			{
				num++;
				arg = "not responsive";
			}
			base.Context.CreateInfoResult("VersionResult", $"{board.ToString().ToUpper()} board {arg}");
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
