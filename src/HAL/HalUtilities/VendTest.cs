using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using Redbox.HAL.Client;
using Redbox.HAL.Core;

namespace HALUtilities;

internal class VendTest
{
	private readonly AutoResetEvent Waiter = new AutoResetEvent(initialState: false);

	private readonly HardwareService Service;

	private readonly ClientHelper Helper;

	private readonly int Iterations;

	private readonly List<IInventoryLocation> m_locs = new List<IInventoryLocation>();

	internal VendTest(int iter)
	{
		Iterations = iter;
		Helper = new ClientHelper();
		Service = Helper.AutoConnect();
	}

	internal void Execute(IConsole console, int? startAt)
	{
		if (startAt.HasValue && startAt.Value != -1)
		{
			RedboxTimer redboxTimer = new RedboxTimer("Vend Test", TimerFired);
			redboxTimer.ScheduleAtNext(startAt.Value, 0);
			Waiter.WaitOne();
		}
		console.WriteLine("Starting test at {0} on {1}", DateTime.Now.ToShortTimeString(), DateTime.Now.ToShortDateString());
		List<Location> list = new List<Location>();
		for (int i = 0; i < Iterations; i++)
		{
			console.WriteLine("Starting iteration {0}", i);
			GenerateList(console);
			IInventoryLocation inventoryLocation = null;
			do
			{
				inventoryLocation = SelectTargetLocation();
				if (inventoryLocation == null)
				{
					break;
				}
				list.Add(inventoryLocation.Location);
				if (!Service.Vend(list, new HardwareJobSchedule
				{
					Priority = HardwareJobPriority.High
				}, out var job).Success)
				{
					Console.WriteLine("Failed to schedule vend job.");
					return;
				}
				job.Pend();
				Helper.WaitForJob(job, out var endStatus);
				list.Clear();
				if (endStatus != HardwareJobStatus.Completed)
				{
					console.WriteLine("Vend job {0} failed with status {1}.", job.ID, endStatus.ToString());
					return;
				}
				if (!job.GetResults(out var _).Success)
				{
					console.WriteLine("Failed to get program results.");
					return;
				}
			}
			while (inventoryLocation != null);
		}
	}

	private void TimerFired(object source, ElapsedEventArgs e)
	{
		Waiter.Set();
	}

	private void GenerateList(IConsole console)
	{
		using KioskInventory kioskInventory = new KioskInventory(Service);
		if (kioskInventory.DeckInventory.Count == 0)
		{
			console.WriteLine("Unable to get inventory from service.");
			Environment.Exit(-1);
		}
		foreach (IInventoryLocation item in kioskInventory.DeckInventory)
		{
			if (!(item.Matrix == "EMPTY"))
			{
				m_locs.Add(item);
			}
		}
	}

	private IInventoryLocation SelectTargetLocation()
	{
		if (0 >= m_locs.Count)
		{
			return null;
		}
		Random random = new Random(DateTime.Now.Millisecond);
		int index = random.Next(0, m_locs.Count - 1);
		IInventoryLocation result = m_locs[index];
		m_locs.RemoveAt(index);
		return result;
	}
}
