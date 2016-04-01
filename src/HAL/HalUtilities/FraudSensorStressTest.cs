using System;
using Redbox.HAL.Client;

namespace HALUtilities;

internal sealed class FraudSensorStressTest
{
	private readonly HardwareService Service;

	private readonly ClientHelper Helper;

	private readonly int Iterations;

	private readonly int ReadPause;

	private readonly int PauseBetweenReads;

	internal void RunJob()
	{
		HardwareJobSchedule hardwareJobSchedule = new HardwareJobSchedule();
		hardwareJobSchedule.Priority = HardwareJobPriority.High;
		HardwareJobSchedule schedule = hardwareJobSchedule;
		HardwareCommandResult hardwareCommandResult = Service.ScheduleJob("fraud-sensor-stress-test", string.Empty, enableDebugging: false, schedule, out var job);
		if (!hardwareCommandResult.Success)
		{
			Console.WriteLine("Unable to schedule job.");
			return;
		}
		job.Push(PauseBetweenReads);
		job.Push(ReadPause);
		job.Push(Iterations);
		Helper.WaitForJob(job, out var endStatus);
		Console.WriteLine("Job {0} ended with status {1}", job.ID, endStatus.ToString());
	}

	internal FraudSensorStressTest(int iterations, int readPause, int pauseBetweenReads)
	{
		Iterations = iterations;
		ReadPause = readPause;
		PauseBetweenReads = pauseBetweenReads;
		Helper = new ClientHelper();
		Service = Helper.AutoConnect();
	}
}
