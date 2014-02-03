namespace Redbox.UpdateService.Model;

internal class StoreScheduleInfo
{
	public byte EndOfScheduleInHoursFromMidnight;

	public byte StartOfScheduleInHoursFromMidnight;

	public int MaxBandwidthWhileOutsideOfSchedule;

	public int MaxBandwidthWhileWithInSchedule;

	public uint NoProgressTimeout;
}
