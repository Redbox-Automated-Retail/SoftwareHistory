using System;
using Redbox.HAL.Client;
using Redbox.HAL.Component.Model;

namespace HALUtilities;

internal sealed class LoopyExecutor : JobExecutor
{
	private readonly int Iterations;

	protected override string JobName => "loopy";

	internal static void Run(int iterations, HardwareService s)
	{
		new ConsoleLogger(enableDebug: true);
		using LoopyExecutor loopyExecutor = new LoopyExecutor(s, iterations);
		loopyExecutor.Job.EventRaised += delegate(HardwareJob job, DateTime eventTime, string eventMessage)
		{
			LogHelper.Instance.Log("<EventReceived> Job = {0} Msg = {1}", job.ID, eventMessage);
		};
		loopyExecutor.Run();
	}

	protected override void SetupJob(HardwareJob job)
	{
		job.Push(Iterations);
	}

	internal LoopyExecutor(HardwareService s, int iter)
		: base(s)
	{
		Iterations = iter;
	}
}
