using System;
using System.Threading;
using Redbox.HAL.Client;
using Redbox.HAL.Client.Executors;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Common.GUI.Functions;

public sealed class KioskFunctionTest : JobExecutor, IKioskFunctionCheckData
{
	private TestState TSDriverStatus;

	private TestState CameraDriverStatus;

	private TestState SnapDecodeStatus;

	private TestState VendDoorStatus;

	private TestState InitStatus;

	private TestState VerticalSlotTestStatus;

	private readonly bool HasCortex;

	private readonly OutputBox Box;

	public string VerticalSlotTestResult => VerticalSlotTestStatus.ToString();

	public string InitTestResult => InitStatus.ToString();

	public string VendDoorTestResult => VendDoorStatus.ToString();

	public string TrackTestResult => InitStatus.ToString();

	public string SnapDecodeTestResult => SnapDecodeStatus.ToString();

	public string TouchscreenDriverTestResult => TSDriverStatus.ToString();

	public string CameraDriverTestResult => CameraDriverStatus.ToString();

	public DateTime Timestamp { get; private set; }

	public string UserIdentifier { get; private set; }

	protected override string JobName => "kiosk-function-test-data";

	protected override void SetupJob(HardwareJob job)
	{
		job.Push(new object[1] { UserIdentifier });
		job.Push(new object[1] { Timestamp.ToString() });
		job.Push(new object[1] { TouchscreenDriverTestResult.ToString() });
		job.Push(new object[1] { CameraDriverTestResult.ToString() });
		job.Push(new object[1] { SnapDecodeTestResult.ToString() });
		job.Push(new object[1] { TrackTestResult.ToString() });
		job.Push(new object[1] { VendDoorTestResult.ToString() });
		job.Push(new object[1] { InitTestResult.ToString() });
		job.Push(new object[1] { VerticalSlotTestResult.ToString() });
	}

	internal void SendData(string username)
	{
		UserIdentifier = username;
		((JobExecutor)this).Run();
		Box.Write("Report queued.");
	}

	internal TestState TestDrivers()
	{
		TestState testState = MatchTS();
		TestState testState2 = MatchCamera();
		if (TestState.Success != testState || TestState.Success != testState2)
		{
			return TestState.Failure;
		}
		return TestState.Success;
	}

	internal TestState TestCameraSnap(int deck, int slot)
	{
		SnapDecodeStatus = TestState.Failure;
		if (!CompositeFunctions.GetItem(deck, slot, Box, base.Service))
		{
			SnapDecodeStatus = TestState.Failure;
		}
		else
		{
			int num = (HasCortex ? 1 : 3);
			ReadBarcodeResult readBarcodeResult = ReadBarcodeResult.ReadBarcodeOfDiskInPicker(base.Service);
			Box.Write(string.Format("Found {0} barcodes ({1}), time={2} {3}", readBarcodeResult.Count, readBarcodeResult.Barcode, readBarcodeResult.ScanTime, readBarcodeResult.IsDuplicate ? "DUPLICATE" : string.Empty));
			int num2 = int.Parse(readBarcodeResult.ScanTime.Split('.')[0]);
			int num3 = int.Parse(readBarcodeResult.Count);
			SnapDecodeStatus = ((num3 >= num && num2 <= 0) ? TestState.Success : TestState.Failure);
			CompositeFunctions.PutItem(base.Service, deck, slot, Box);
		}
		return SnapDecodeStatus;
	}

	internal TestState TestVendDoor()
	{
		VendDoorStatus = TestState.Failure;
		using (new InstructionHelper(base.Service))
		{
			IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
			if (service.VendDoorRent().Success)
			{
				Thread.Sleep(500);
				VendDoorStatus = (service.VendDoorClose().Success ? TestState.Success : TestState.Failure);
			}
			return VendDoorStatus;
		}
	}

	internal TestState RunVertialSlotTest(int slot)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected O, but got Unknown
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Invalid comparison between I4 and Unknown
		VerticalSync val = new VerticalSync(base.Service, slot);
		try
		{
			((JobExecutor)val).Run();
			if (6 == (int)((JobExecutor)val).EndStatus)
			{
				Box.Write("The job completed successfully.");
				VerticalSlotTestStatus = TestState.Success;
			}
			else
			{
				foreach (ProgramResult result in ((JobExecutor)val).Results)
				{
					Box.Write($"Failure at Deck {result.Deck} Slot {result.Slot} MSG: {result.Message}");
				}
				VerticalSlotTestStatus = TestState.Failure;
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		return VerticalSlotTestStatus;
	}

	internal TestState RunInit()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between I4 and Unknown
		InitJob val = new InitJob(base.Service);
		try
		{
			((JobExecutor)val).Run();
			if (6 == (int)((JobExecutor)val).EndStatus)
			{
				Box.Write("Init succeeded.");
				InitStatus = TestState.Success;
			}
			else
			{
				InitStatus = TestState.Failure;
				if (((JobExecutor)val).Errors.Count > 0)
				{
					foreach (Error error in ((JobExecutor)val).Errors)
					{
						Box.Write(error.Details);
					}
					Box.Write("Init didn't succeed; errors follow:");
				}
			}
			return InitStatus;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	internal bool GetUnknownStats()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		InventoryStatsJob val = new InventoryStatsJob(base.Service);
		((JobExecutor)val).Run();
		if ((int)((JobExecutor)val).EndStatus != 6)
		{
			return false;
		}
		foreach (ProgramResult result in ((JobExecutor)val).Results)
		{
			if (result.Code == "TotalEmptyCount")
			{
				Box.Write($"  EMPTY slots: {result.Message}");
			}
			else if (result.Code == "UnknownCount")
			{
				Box.Write($"  UNKNOWN slots: {result.Message}");
			}
		}
		Box.Write("Inventory Stats: ");
		return true;
	}

	internal KioskFunctionTest(HardwareService service, OutputBox box, bool hasCortex, string uname)
		: base(service)
	{
		Box = box;
		HasCortex = hasCortex;
		Timestamp = DateTime.Now;
		UserIdentifier = uname;
		TSDriverStatus = (CameraDriverStatus = (SnapDecodeStatus = TestState.NotStarted));
		VendDoorStatus = (InitStatus = (VerticalSlotTestStatus = TestState.NotStarted));
	}

	private TestState MatchCamera()
	{
		IDeviceDescriptor deviceDescriptor = ServiceLocator.Instance.GetService<IUsbDeviceService>().FindActiveCamera(matchDriver: true);
		if (deviceDescriptor != null)
		{
			CameraDriverStatus = TestState.Success;
			Box.Write("Matched camera {0}", deviceDescriptor.Friendlyname);
		}
		else
		{
			Box.Write("Failed to locate camera or match camera driver.");
			CameraDriverStatus = TestState.Failure;
		}
		return CameraDriverStatus;
	}

	private TestState MatchTS()
	{
		IDeviceDescriptor deviceDescriptor = ServiceLocator.Instance.GetService<IUsbDeviceService>().FindTouchScreen(matchDriver: true);
		if (deviceDescriptor != null)
		{
			TSDriverStatus = TestState.Success;
			Box.Write("Found Touchscreen {0}", deviceDescriptor.Friendlyname);
		}
		else
		{
			TSDriverStatus = TestState.Failure;
			Box.Write("Failed to locate touchscreen or match driver.");
		}
		return TSDriverStatus;
	}
}
