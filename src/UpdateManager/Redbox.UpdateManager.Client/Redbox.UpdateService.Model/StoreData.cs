using System;

namespace Redbox.UpdateService.Model;

internal class StoreData
{
	public long ID { get; set; }

	public string Number { get; set; }

	public DateTime? LastCheckIn { get; set; }

	public bool? Enabled { get; set; }
}
