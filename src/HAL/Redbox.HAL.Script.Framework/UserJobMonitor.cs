using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

public class UserJobMonitor
{
	public static UserJobMonitor Instance => Singleton<UserJobMonitor>.Instance;

	public DateTime? LastJob { get; private set; }

	internal void RegisterLastJob()
	{
		LastJob = DateTime.Now;
	}

	private UserJobMonitor()
	{
	}
}
