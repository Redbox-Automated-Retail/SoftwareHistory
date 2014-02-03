using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using Redbox.HAL.Client;
using Redbox.HAL.Core;

namespace HALUtilities;

internal class VendTest
{
	private AutoResetEvent Waiter = new AutoResetEvent(initialState: false);

	private List<InventoryLocation> m_locs = new List<InventoryLocation>();

	private HardwareService Service { get; set; }

	private ClientHelper Helper { get; set; }

	private int Iterations { get; set; }

	internal VendTest(int iter)
	{
		Iterations = iter;
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
		ClientHelper clientHelper = new ClientHelper();
		Service = clientHelper.AutoConnect();
		for (int i = 0; i < Iterations; i++)
		{
			console.WriteLine("Starting iteration {0}", i);
			GenerateList(clientHelper, console);
			InventoryLocation inventoryLocation = null;
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
					StartTime = DateTime.Now,
					Priority = HardwareJobPriority.High
				}, out var job).Success)
				{
					Console.WriteLine("Failed to schedule vend job.");
					return;
				}
				job.Pend();
				clientHelper.WaitForJob(job, out var endStatus);
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

	private void GenerateList(ClientHelper helper, IConsole console)
	{
		List<InventoryLocation> list = KioskInventory.Load(Service, console);
		if (list.Count == 0)
		{
			console.WriteLine("Unable to get inventory from service.");
			Environment.Exit(-1);
		}
		foreach (InventoryLocation item in list)
		{
			if (!(item.Matrix == "EMPTY"))
			{
				m_locs.Add(item);
			}
		}
	}

	private InventoryLocation SelectTargetLocation()
	{
		if (0 >= m_locs.Count)
		{
			return null;
		}
		Random random = new Random(DateTime.Now.Millisecond);
		int index = random.Next(0, m_locs.Count - 1);
		InventoryLocation result = m_locs[index];
		m_locs.RemoveAt(index);
		return result;
	}
}
