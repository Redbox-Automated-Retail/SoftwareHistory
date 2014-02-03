using Redbox.HAL.Client;

namespace Redbox.HAL.MSHALTester;

internal class JobResult
{
	internal HardwareJobStatus EndStatus { get; set; }

	internal ProgramResult[] Results { get; set; }

	internal HardwareCommandResult Result { get; set; }
}
