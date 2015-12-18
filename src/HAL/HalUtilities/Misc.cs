using System;
using System.Collections.Generic;
using Redbox.HAL.Client;
using Redbox.HAL.Core;

namespace HALUtilities;

internal static class Misc
{
	private class D
	{
		internal readonly int Slot;

		internal D(int _s)
		{
			Slot = _s;
		}
	}

	private class C
	{
		internal readonly int ID;

		internal readonly DateTime CreatedOn;

		internal C(int id, DateTime co)
		{
			ID = id;
			CreatedOn = co;
		}
	}

	private static void RunTest()
	{
		TimeSort();
		ClientHelper clientHelper = new ClientHelper();
		HardwareService hardwareService = clientHelper.AutoConnect();
		if (hardwareService != null)
		{
			TestAndSet(hardwareService, "FixCameraAfterReturn", "TRUE");
			TestAndSet(hardwareService, "ReturnCameraAlertThreshold", "2");
			hardwareService.ExecuteImmediate("CLEAR", out var _);
		}
	}

	private static void SegmentTest()
	{
		List<Range> list = new List<Range>();
		list.Add(new Range(1, 15));
		list.Add(new Range(16, 30));
		list.Add(new Range(31, 45));
		list.Add(new Range(46, 60));
		list.Add(new Range(61, 75));
		list.Add(new Range(76, 90));
		int num = 6;
		int num2 = 90;
		int num3 = 5;
		for (int i = 0; i < 10; i++)
		{
			if (i >= num)
			{
				Console.WriteLine("Quadrants exceeded.");
				continue;
			}
			int num4 = ((i > 0) ? (num - i) : 0);
			if (i == 0)
			{
				num4 = num;
			}
			if (--num4 == 0)
			{
				Console.WriteLine(" Drum fully rotated.");
				continue;
			}
			Range range = list[num4];
			int num5 = range.Start - num3;
			if (num5 <= 0)
			{
				num5 = num2 - Math.Abs(num5);
			}
			Console.WriteLine("On rotation = {0} Slot = {1}", i, num5);
		}
	}

	private static void DeckSort()
	{
		Array.Sort(new D[5]
		{
			new D(20),
			new D(1),
			new D(2),
			new D(21),
			new D(19)
		}, (D x, D y) => x.Slot.CompareTo(y.Slot));
		Console.WriteLine("f");
	}

	private static void TimeSort()
	{
		DateTime now = DateTime.Now;
		int num = 0;
		C[] array = new C[8]
		{
			new C(num++, now),
			new C(num++, now.Subtract(new TimeSpan(1, 2, 3, 0))),
			new C(num++, now.AddDays(1.0)),
			null,
			null,
			null,
			null,
			null
		};
		for (int i = 3; i < array.Length; i++)
		{
			C c = null;
			c = ((i % 2 != 0) ? new C(num++, now.Subtract(new TimeSpan(0, 5 * i, 0))) : new C(num++, now.AddMinutes(5 * i)));
			array[i] = c;
		}
		Array.Sort(array, (C left, C right) => left.CreatedOn.CompareTo(right.CreatedOn));
		C[] array2 = array;
		foreach (C c2 in array2)
		{
			Console.WriteLine("Ctx {0} at {1} {2}", c2.ID, c2.CreatedOn.ToShortDateString(), c2.CreatedOn.ToShortTimeString());
		}
		Console.WriteLine("foo");
	}

	private static void TestOptions()
	{
		ClientHelper clientHelper = new ClientHelper();
		HardwareService hardwareService = clientHelper.AutoConnect();
		if (hardwareService == null)
		{
			Console.WriteLine("Can't connect.");
			return;
		}
		HardwareCommandResult hardwareCommandResult = hardwareService.ExecuteImmediate("GETCFG \"ReturnCameraAlertThreshold\" TYPE=CONTROLLER", out var job);
		if (!hardwareCommandResult.Success)
		{
			Console.WriteLine("Call failed.");
			{
				foreach (Error error in hardwareCommandResult.Errors)
				{
					Console.WriteLine("{0} {1}", error.Description, error.Details);
				}
				return;
			}
		}
		if (job.GetStack(out var stack).Success)
		{
			Console.WriteLine("Option ReturnCameraAlertThreshold is {0}", stack.Peek());
		}
	}

	private static void Log(string msg)
	{
		Console.WriteLine(msg);
	}

	private static void Log(string fmt, params object[] stuff)
	{
		Log(string.Format(fmt, stuff));
	}

	private static void TestAndSet(HardwareService service, string option, string should)
	{
		string statement = $"GETCFG \"{option}\" TYPE=CONTROLLER";
		HardwareCommandResult hardwareCommandResult = service.ExecuteImmediate(statement, out var job);
		if (!hardwareCommandResult.Success)
		{
			Log("GETCFG call failed. Errors:");
			{
				foreach (Error error in hardwareCommandResult.Errors)
				{
					Log("  {0} {1}", error.Description, error.Details);
				}
				return;
			}
		}
		if (!job.GetStack(out var stack).Success)
		{
			return;
		}
		string text = stack.Peek();
		Log("Option {0} is {1}", option, stack.Peek());
		if (!text.Equals(should, StringComparison.CurrentCultureIgnoreCase))
		{
			statement = $"SETCFG \"{option}\" \"{should}\" TYPE=CONTROLLER";
			if (service.ExecuteImmediate(statement, out job).Success)
			{
				Log("Successfully set option {0} -> {1}", option, should);
			}
			else
			{
				Log("Call failed.");
			}
		}
	}
}
