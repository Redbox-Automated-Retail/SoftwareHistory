using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using Redbox.Core;
using Redbox.HAL.Client;
using Redbox.HAL.Core;
using Redbox.HardwareServices.Proxy.ComponentModel;
using Redbox.IPC.Framework;
using Redbox.KioskEngine.ComponentModel;
using Redbox.REDS.Framework;

namespace Redbox.HardwareServices.Proxy;

internal class HardwareService : IHardwareService
{
	internal const int DefaultHardwareServiceTimeout = 600000;

	internal const string DefaultHardwareServiceUrl = "rcp://localhost:7001";

	private Redbox.HAL.Client.HardwareJob[] SuspendedMaintenanceModeJobs;

	private readonly IList<Redbox.HAL.Client.HardwareJob> m_activeJobs = new List<Redbox.HAL.Client.HardwareJob>();

	private int m_working;

	public static HardwareService Instance => Singleton<HardwareService>.Instance;

	public string Url
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Store Services", "HardwareServiceURL", "rcp://localhost:7001");
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Store Services", "HardwareServiceURL", value);
			Reset();
		}
	}

	public int Timeout
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Store Services", "HardwareServiceTimeout", 600000);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Store Services", "HardwareServiceTimeout", value);
			Reset();
		}
	}

	internal Redbox.HAL.Client.HardwareService Service { get; set; }

	public bool CanSwitch()
	{
		IEnumerable<Redbox.HAL.Client.HardwareJob> enumerable = m_activeJobs.Where((Redbox.HAL.Client.HardwareJob item) => string.Compare(item.ProgramName, "return", ignoreCase: true) == 0);
		int num;
		if (!enumerable.Any())
		{
			num = ((m_working == 0) ? 1 : 0);
			if (num != 0)
			{
				goto IL_0076;
			}
		}
		else
		{
			num = 0;
		}
		enumerable.ForEach(delegate(Redbox.HAL.Client.HardwareJob item)
		{
			LogHelper.Instance.Log("Return Job {0} Running", item.ID);
		});
		goto IL_0076;
		IL_0076:
		return (byte)num != 0;
	}

	public void Reset()
	{
		LogHelper.Instance.Log("Reset hardware service:");
		string version = AssemblyInfoHelper.GetVersion(typeof(Redbox.HAL.Client.HardwareService).Assembly);
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		service["HALVersion"] = version;
		service["HALServiceVersion"] = (string)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Redbox\\HAL", "Product Version", version);
		Service = new Redbox.HAL.Client.HardwareService(IPCProtocol.Parse(Url));
		try
		{
			foreach (Redbox.HAL.Client.HardwareJob activeJob in m_activeJobs)
			{
				LogHelper.Instance.Log("...Disconnect job '{0}'.", activeJob.ID);
				activeJob.Disconnect();
			}
			LogHelper.Instance.Log("...Clear local job list.");
			m_activeJobs.Clear();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in HardwareService.Reset.", e);
		}
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList CompileScriptResources()
	{
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		if (service == null)
		{
			return errorList;
		}
		LogHelper.Instance.Log("Compile HAL Script resources:");
		IHardwareService service2 = ServiceLocator.Instance.GetService<IHardwareService>();
		IFileCacheService service3 = ServiceLocator.Instance.GetService<IFileCacheService>();
		foreach (IResource resource in service.GetResources("halscript"))
		{
			object content = resource.GetAspect("content").GetContent();
			string text = ((!(content is byte[])) ? ((string)content) : Encoding.ASCII.GetString((byte[])content));
			if (!string.IsNullOrEmpty(text))
			{
				string text2 = Path.ChangeExtension(resource.Name, ".hs");
				service3.SetFileContent(text2, text, overwrite: true);
				string path = Path.Combine(service3.GetCachePath(), text2);
				LogHelper.Instance.Log("...Compile HAL script resource '{0}'.", resource.Name);
				Redbox.KioskEngine.ComponentModel.ErrorList errorList2 = service2.Compile(path, resource.Name);
				if (errorList2.ContainsError())
				{
					errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("H033", $"The HAL Script resource '{resource.Name}' failed to compile, the errors that follow are the details.", "Correct the script resource and recompile."));
					errorList.AddRange(errorList2);
				}
			}
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList Initialize()
	{
		LogHelper.Instance.Log("Initialize Hardware Service, URL: {0}", Url);
		Redbox.KioskEngine.ComponentModel.ErrorList result = new Redbox.KioskEngine.ComponentModel.ErrorList();
		ServiceLocator.Instance.AddService(typeof(IHardwareService), Instance);
		Reset();
		return result;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList Init(out string jobId, HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler)
	{
		jobId = null;
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		HardwareCommandResult hardwareCommandResult = Service.Init(new HardwareJobSchedule
		{
			Priority = HardwareJobPriority.Highest
		}, out var job);
		if (!hardwareCommandResult.Success)
		{
			hardwareCommandResult.Errors.AddToLocalErrorList(errorList);
		}
		else
		{
			jobId = ConfigureJob(job, eventHandler, statusChangeHandler, errorList);
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList ScheduleJob(string scriptName, string label, bool enableDebugging, string priority, string startTimeString, ReadOnlyCollection<object> stackValues, HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, bool allowMultiple, out string jobId)
	{
		jobId = null;
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		IHardwareJob hardwareJob = null;
		if (!allowMultiple)
		{
			Redbox.KioskEngine.ComponentModel.ErrorList hardwareJobs = GetHardwareJobs(out var hardwareJobs2);
			if (hardwareJobs.ContainsError())
			{
				errorList.AddRange(hardwareJobs);
				return errorList;
			}
			hardwareJob = hardwareJobs2.ToList().Find((IHardwareJob item) => string.Compare(scriptName, item.ProgramName, ignoreCase: true) == 0 && string.Compare(label, item.Label, ignoreCase: true) == 0 && (string.Compare(HardwareJobStatus.Pending.ToString(), item.Status, ignoreCase: true) == 0 || string.Compare(HardwareJobStatus.Running.ToString(), item.Status, ignoreCase: true) == 0 || string.Compare(HardwareJobStatus.Suspended.ToString(), item.Status, ignoreCase: true) == 0));
		}
		Redbox.HAL.Client.HardwareJob job;
		if (hardwareJob == null)
		{
			DateTime? startTime = null;
			if (DateTime.TryParse(startTimeString, out var result))
			{
				startTime = result;
			}
			HardwareCommandResult hardwareCommandResult = Service.ScheduleJob(scriptName, label, enableDebugging, new HardwareJobSchedule
			{
				StartTime = startTime,
				Priority = Enum<HardwareJobPriority>.ParseIgnoringCase(priority, HardwareJobPriority.Normal)
			}, out job);
			if (!hardwareCommandResult.Success)
			{
				hardwareCommandResult.Errors.AddToLocalErrorList(errorList);
			}
			else
			{
				foreach (object stackValue in stackValues)
				{
					job.Push(stackValue);
				}
				jobId = ConfigureJob(job, eventHandler, statusChangeHandler, errorList);
			}
		}
		else
		{
			HardwareCommandResult job2 = Service.GetJob(hardwareJob.ID, out job);
			if (job2.Success)
			{
				jobId = ConfigureJob(job, eventHandler, statusChangeHandler, errorList);
			}
			else
			{
				job2.Errors.AddToLocalErrorList(errorList);
			}
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList Vend(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, string[] barcodes, int? prepositionedDeck, int? prepositionedSlot, out string jobId)
	{
		jobId = null;
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		HardwareCommandResult hardwareCommandResult;
		Redbox.HAL.Client.HardwareJob job;
		if (!prepositionedDeck.HasValue && !prepositionedSlot.HasValue)
		{
			hardwareCommandResult = Service.Vend(barcodes, new HardwareJobSchedule
			{
				Priority = HardwareJobPriority.Highest
			}, out job);
		}
		else
		{
			hardwareCommandResult = Service.ScheduleJob("vend", null, enableDebugging: false, new HardwareJobSchedule
			{
				Priority = HardwareJobPriority.Highest
			}, out job);
			if (hardwareCommandResult.Success && prepositionedSlot.HasValue && prepositionedDeck.HasValue)
			{
				job.Push(barcodes);
				job.Push(prepositionedSlot.Value);
				job.Push(prepositionedDeck.Value);
				job.Push(barcodes.Length);
				job.Push("PREPOSITION-PICKER");
			}
		}
		if (!hardwareCommandResult.Success)
		{
			hardwareCommandResult.Errors.AddToLocalErrorList(errorList);
		}
		else
		{
			jobId = ConfigureJob(job, eventHandler, statusChangeHandler, errorList);
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList PreposVend(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, string[] barcodes, out string jobId)
	{
		jobId = null;
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		HardwareCommandResult hardwareCommandResult = Service.PreposVend(new HardwareJobSchedule
		{
			Priority = HardwareJobPriority.Highest
		}, barcodes, out var job);
		if (!hardwareCommandResult.Success)
		{
			hardwareCommandResult.Errors.AddToLocalErrorList(errorList);
		}
		else
		{
			jobId = ConfigureJob(job, eventHandler, statusChangeHandler, errorList);
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList CheckPicker(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, out string jobId)
	{
		jobId = null;
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		HardwareCommandResult hardwareCommandResult = Service.CheckPicker(new HardwareJobSchedule
		{
			Priority = HardwareJobPriority.Highest
		}, out var job);
		if (!hardwareCommandResult.Success)
		{
			hardwareCommandResult.Errors.AddToLocalErrorList(errorList);
		}
		else
		{
			jobId = ConfigureJob(job, eventHandler, statusChangeHandler, errorList);
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList VendLocations(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, IList<int> locations, out string jobId)
	{
		jobId = null;
		int num = 1;
		Location location = new Location();
		List<Location> list = new List<Location>();
		foreach (int location2 in locations)
		{
			if (num % 2 == 0)
			{
				location.Slot = location2;
				list.Add(location);
				location = new Location();
			}
			else
			{
				location.Deck = location2;
			}
			num++;
		}
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		HardwareCommandResult hardwareCommandResult = Service.Vend(list, new HardwareJobSchedule
		{
			Priority = HardwareJobPriority.High
		}, out var job);
		if (!hardwareCommandResult.Success)
		{
			hardwareCommandResult.Errors.AddToLocalErrorList(errorList);
		}
		else
		{
			jobId = ConfigureJob(job, eventHandler, statusChangeHandler, errorList);
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList ResetLocations(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, IList<int> locations, out string jobId)
	{
		jobId = null;
		int num = 1;
		Location location = new Location();
		List<Location> list = new List<Location>();
		foreach (int location2 in locations)
		{
			if (num % 2 == 0)
			{
				location.Slot = location2;
				list.Add(location);
				location = new Location();
			}
			else
			{
				location.Deck = location2;
			}
			num++;
		}
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		HardwareCommandResult hardwareCommandResult = Service.ResetLocations(list, new HardwareJobSchedule
		{
			Priority = HardwareJobPriority.Highest
		}, out var job);
		if (!hardwareCommandResult.Success)
		{
			hardwareCommandResult.Errors.AddToLocalErrorList(errorList);
		}
		else
		{
			jobId = ConfigureJob(job, eventHandler, statusChangeHandler, errorList);
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList VendUnknown(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, int deck, int slot, out string jobId)
	{
		jobId = null;
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		HardwareCommandResult hardwareCommandResult = Service.VendUnknown(deck, slot, new HardwareJobSchedule
		{
			Priority = HardwareJobPriority.High
		}, out var job);
		if (!hardwareCommandResult.Success)
		{
			hardwareCommandResult.Errors.AddToLocalErrorList(errorList);
		}
		else
		{
			jobId = ConfigureJob(job, eventHandler, statusChangeHandler, errorList);
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList Return(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, out string jobId)
	{
		jobId = null;
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		HardwareCommandResult hardwareCommandResult = Service.Return(new HardwareJobSchedule
		{
			Priority = HardwareJobPriority.Highest
		}, out var job);
		if (!hardwareCommandResult.Success)
		{
			hardwareCommandResult.Errors.AddToLocalErrorList(errorList);
		}
		else
		{
			jobId = ConfigureJob(job, eventHandler, statusChangeHandler, errorList);
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList ReturnUnknown(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, string returnTime, out string jobId)
	{
		jobId = null;
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		HardwareCommandResult hardwareCommandResult = Service.ReturnUnknown(new HardwareJobSchedule
		{
			Priority = HardwareJobPriority.Highest
		}, returnTime, out var job);
		if (!hardwareCommandResult.Success)
		{
			hardwareCommandResult.Errors.AddToLocalErrorList(errorList);
		}
		else
		{
			jobId = ConfigureJob(job, eventHandler, statusChangeHandler, errorList);
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList Sync(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, string label, DateTime? startTime, int startDeck, int startSlot, int endDeck, int endSlot, out string jobId)
	{
		jobId = null;
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		HardwareCommandResult hardwareCommandResult = Service.HardSync(new SyncRange(startDeck, endDeck, new Redbox.HAL.Core.Range(startSlot, endSlot)), new HardwareJobSchedule
		{
			Priority = HardwareJobPriority.Low,
			StartTime = startTime
		}, out var job);
		if (!hardwareCommandResult.Success)
		{
			hardwareCommandResult.Errors.AddToLocalErrorList(errorList);
		}
		else
		{
			job.SetLabel(label);
			jobId = ConfigureJob(job, eventHandler, statusChangeHandler, errorList);
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList SyncLocations(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, string label, DateTime? startTime, IList<int> locations, out string jobId)
	{
		jobId = null;
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		if (locations.Count == 0)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("H004", "SyncUnknowns requires at least two values in the locations list.", "Correct the values provided so that at least one deck and one slot value are provided."));
			return errorList;
		}
		HardwareCommandResult hardwareCommandResult = Service.ScheduleJob("sync-locations", null, enableDebugging: false, new HardwareJobSchedule
		{
			Priority = HardwareJobPriority.Low,
			StartTime = startTime
		}, out var job);
		if (!hardwareCommandResult.Success)
		{
			hardwareCommandResult.Errors.AddToLocalErrorList(errorList);
		}
		else
		{
			foreach (int location in locations)
			{
				job.Push(location);
			}
			job.Push(locations.Count / 2);
			job.SetLabel(label);
			jobId = ConfigureJob(job, eventHandler, statusChangeHandler, errorList);
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList GetMachineInfo(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, out string jobId)
	{
		jobId = null;
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		HardwareCommandResult machineInfo = Service.GetMachineInfo(new HardwareJobSchedule
		{
			Priority = HardwareJobPriority.Highest
		}, out var job);
		if (!machineInfo.Success)
		{
			machineInfo.Errors.AddToLocalErrorList(errorList);
		}
		else
		{
			jobId = ConfigureJob(job, eventHandler, statusChangeHandler, errorList);
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList GetHardwareStatus(DateTime? startTime, HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, out string jobId)
	{
		jobId = null;
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		HardwareCommandResult hardwareStatus = Service.GetHardwareStatus(new HardwareJobSchedule
		{
			Priority = HardwareJobPriority.Immediate
		}, out var job);
		if (!hardwareStatus.Success)
		{
			hardwareStatus.Errors.AddToLocalErrorList(errorList);
		}
		else
		{
			jobId = ConfigureJob(job, eventHandler, statusChangeHandler, errorList);
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList SoftSync(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, out string jobId)
	{
		jobId = null;
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		HardwareCommandResult hardwareCommandResult = Service.SoftSync(new HardwareJobSchedule
		{
			Priority = HardwareJobPriority.Highest
		}, out var job);
		if (!hardwareCommandResult.Success)
		{
			hardwareCommandResult.Errors.AddToLocalErrorList(errorList);
		}
		else
		{
			jobId = ConfigureJob(job, eventHandler, statusChangeHandler, errorList);
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList QlmUnload(string label, DateTime? startTime, HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, out string jobId)
	{
		jobId = null;
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		HardwareCommandResult hardwareCommandResult = Service.QlmUnload(new HardwareJobSchedule
		{
			StartTime = startTime,
			Priority = HardwareJobPriority.Normal
		}, out var job);
		if (!hardwareCommandResult.Success)
		{
			hardwareCommandResult.Errors.AddToLocalErrorList(errorList);
		}
		else
		{
			jobId = ConfigureJob(job, eventHandler, statusChangeHandler, errorList);
			job.SetLabel(label);
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList QlmThin(string label, DateTime? startTime, string[] ids, HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, out string jobId)
	{
		jobId = null;
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		if (ids.Length == 0)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("H004", "Thin requires at least one barcode in the list.", "Correct the values provided so that at least one barcode is provided."));
			return errorList;
		}
		HardwareCommandResult hardwareCommandResult = Service.Thin(ids, new HardwareJobSchedule
		{
			StartTime = startTime,
			Priority = HardwareJobPriority.BelowNormal
		}, out var job);
		if (!hardwareCommandResult.Success)
		{
			hardwareCommandResult.Errors.AddToLocalErrorList(errorList);
		}
		else
		{
			jobId = ConfigureJob(job, eventHandler, statusChangeHandler, errorList);
			job.SetLabel(label);
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList VmzThin(string label, DateTime? startTime, string[] barcodesToBin, string[] rebalances, string[] redeployments, string[] barcodesToVmz, HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, out string jobId)
	{
		jobId = null;
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		if (barcodesToBin.Length == 0 && rebalances.Length == 0 && redeployments.Length == 0 && barcodesToVmz.Length == 0)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("H004", "Thin requires at least one barcode in the list.", "Correct the values provided so that at least one barcode is provided."));
			return errorList;
		}
		HardwareCommandResult hardwareCommandResult = Service.ThinVMZ(barcodesToBin, rebalances, redeployments, barcodesToVmz, new HardwareJobSchedule
		{
			StartTime = startTime,
			Priority = HardwareJobPriority.BelowNormal
		}, out var job);
		if (!hardwareCommandResult.Success)
		{
			hardwareCommandResult.Errors.AddToLocalErrorList(errorList);
		}
		else
		{
			jobId = ConfigureJob(job, eventHandler, statusChangeHandler, errorList);
			job.SetLabel(label);
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList QlmUnloadAndThin(string label, DateTime? startTime, string[] ids, HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, out string jobId)
	{
		jobId = null;
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		if (ids.Length == 0)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("H004", "Thin requires at least one barcode in the list.", "Correct the values provided so that at least one barcode is provided."));
			return errorList;
		}
		HardwareCommandResult hardwareCommandResult = Service.QlmUnloadAndThin(ids, new HardwareJobSchedule
		{
			StartTime = startTime,
			Priority = HardwareJobPriority.BelowNormal
		}, out var job);
		if (!hardwareCommandResult.Success)
		{
			hardwareCommandResult.Errors.AddToLocalErrorList(errorList);
		}
		else
		{
			jobId = ConfigureJob(job, eventHandler, statusChangeHandler, errorList);
			job.SetLabel(label);
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList GetBarcodesInBin(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, out string jobId)
	{
		jobId = null;
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		HardwareCommandResult barcodesInBin = Service.GetBarcodesInBin(new HardwareJobSchedule
		{
			Priority = HardwareJobPriority.Highest
		}, out var job);
		if (!barcodesInBin.Success)
		{
			barcodesInBin.Errors.AddToLocalErrorList(errorList);
		}
		else
		{
			jobId = ConfigureJob(job, eventHandler, statusChangeHandler, errorList);
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList GetInventoryStats(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, out string jobId)
	{
		jobId = null;
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		HardwareCommandResult inventoryStats = Service.GetInventoryStats(new HardwareJobSchedule
		{
			Priority = HardwareJobPriority.Highest
		}, out var job);
		if (!inventoryStats.Success)
		{
			inventoryStats.Errors.AddToLocalErrorList(errorList);
		}
		else
		{
			jobId = ConfigureJob(job, eventHandler, statusChangeHandler, errorList);
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList VMZRemovedThins(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, out string jobId)
	{
		jobId = null;
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		HardwareCommandResult hardwareCommandResult = Service.VMZRemovedThins(new HardwareJobSchedule
		{
			Priority = HardwareJobPriority.Highest
		}, out var job);
		if (!hardwareCommandResult.Success)
		{
			hardwareCommandResult.Errors.AddToLocalErrorList(errorList);
		}
		else
		{
			jobId = ConfigureJob(job, eventHandler, statusChangeHandler, errorList);
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList GetNonThinsInVMZ(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, out string jobId)
	{
		jobId = null;
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		HardwareCommandResult nonThinsInVMZ = Service.GetNonThinsInVMZ(new HardwareJobSchedule
		{
			Priority = HardwareJobPriority.Highest
		}, out var job);
		if (!nonThinsInVMZ.Success)
		{
			nonThinsInVMZ.Errors.AddToLocalErrorList(errorList);
		}
		else
		{
			jobId = ConfigureJob(job, eventHandler, statusChangeHandler, errorList);
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList VMZMerchSummary(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, out string jobId)
	{
		jobId = null;
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		HardwareCommandResult hardwareCommandResult = Service.VMZMerchSummary(new HardwareJobSchedule
		{
			Priority = HardwareJobPriority.Highest
		}, out var job);
		if (!hardwareCommandResult.Success)
		{
			hardwareCommandResult.Errors.AddToLocalErrorList(errorList);
		}
		else
		{
			jobId = ConfigureJob(job, eventHandler, statusChangeHandler, errorList);
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList FileMarkerDisk(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, int markerDeck, int markerSlot, out string jobId)
	{
		jobId = null;
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		HardwareCommandResult hardwareCommandResult = Service.FileMarkerDisk(markerDeck, markerSlot, new HardwareJobSchedule
		{
			Priority = HardwareJobPriority.Highest
		}, out var job);
		LogHelper.Instance.Log($"KernelFunction result.Success| {hardwareCommandResult.Success}");
		if (!hardwareCommandResult.Success)
		{
			hardwareCommandResult.Errors.AddToLocalErrorList(errorList);
		}
		else
		{
			jobId = ConfigureJob(job, eventHandler, statusChangeHandler, errorList);
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList ClearMerchStatus(string[] ids, HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, out string jobId)
	{
		jobId = null;
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		HardwareCommandResult hardwareCommandResult = Service.ClearMerchStatus(ids, new HardwareJobSchedule
		{
			Priority = HardwareJobPriority.Highest
		}, out var job);
		LogHelper.Instance.Log($"KernelFunction result.Success| {hardwareCommandResult.Success}");
		if (!hardwareCommandResult.Success)
		{
			hardwareCommandResult.Errors.AddToLocalErrorList(errorList);
		}
		else
		{
			jobId = ConfigureJob(job, eventHandler, statusChangeHandler, errorList);
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList GetBarcodeVMZPosition(string[] ids, HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, out string jobId)
	{
		jobId = null;
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		HardwareCommandResult barcodeVMZPosition = Service.GetBarcodeVMZPosition(ids, new HardwareJobSchedule
		{
			Priority = HardwareJobPriority.Highest
		}, out var job);
		LogHelper.Instance.Log($"KernelFunction result.Success| {barcodeVMZPosition.Success}");
		if (!barcodeVMZPosition.Success)
		{
			barcodeVMZPosition.Errors.AddToLocalErrorList(errorList);
		}
		else
		{
			jobId = ConfigureJob(job, eventHandler, statusChangeHandler, errorList);
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList Compile(string path, string name)
	{
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		HardwareCommandResult hardwareCommandResult = Service.ExecuteServiceCommand($"PROGRAM compile path: @'{path}' name: '{name}' requires-client-connection: false");
		if (!hardwareCommandResult.Success)
		{
			hardwareCommandResult.Errors.AddToLocalErrorList(errorList);
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList Trash(string jobId)
	{
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		HardwareCommandResult job = Service.GetJob(jobId, out var job2);
		if (!job.Success)
		{
			job.Errors.AddToLocalErrorList(errorList);
		}
		else
		{
			job = job2.Trash();
			if (!job.Success)
			{
				job.Errors.AddToLocalErrorList(errorList);
			}
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList Resume(string jobId)
	{
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		HardwareCommandResult job = Service.GetJob(jobId, out var job2);
		if (!job.Success)
		{
			job.Errors.AddToLocalErrorList(errorList);
		}
		else
		{
			job = job2.Resume();
			if (!job.Success)
			{
				job.Errors.AddToLocalErrorList(errorList);
			}
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList Suspend(string jobId)
	{
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		HardwareCommandResult job = Service.GetJob(jobId, out var job2);
		if (!job.Success)
		{
			job.Errors.AddToLocalErrorList(errorList);
		}
		else
		{
			job = job2.Suspend();
			if (!job.Success)
			{
				job.Errors.AddToLocalErrorList(errorList);
			}
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList Terminate(string jobId)
	{
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		HardwareCommandResult job = Service.GetJob(jobId, out var job2);
		if (!job.Success)
		{
			job.Errors.AddToLocalErrorList(errorList);
		}
		else
		{
			job = job2.Terminate();
			if (!job.Success)
			{
				job.Errors.AddToLocalErrorList(errorList);
			}
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList ResumeAll()
	{
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		HardwareCommandResult hardwareCommandResult = Service.ResumeAll();
		if (!hardwareCommandResult.Success)
		{
			hardwareCommandResult.Errors.AddToLocalErrorList(errorList);
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList SuspendAll()
	{
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		HardwareCommandResult hardwareCommandResult = Service.SuspendAll();
		if (!hardwareCommandResult.Success)
		{
			hardwareCommandResult.Errors.AddToLocalErrorList(errorList);
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList GetProgramResults(string jobId, out ReadOnlyCollection<IHardwareResult> hardwareResults)
	{
		hardwareResults = new List<IHardwareResult>().AsReadOnly();
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		HardwareCommandResult job = Service.GetJob(jobId, out var job2);
		if (!job.Success)
		{
			job.Errors.AddToLocalErrorList(errorList);
		}
		else
		{
			job = job2.GetResults(out var results);
			if (!job.Success)
			{
				job.Errors.AddToLocalErrorList(errorList);
			}
			else
			{
				List<IHardwareResult> list = new List<IHardwareResult>();
				ProgramResult[] array = results;
				foreach (ProgramResult programResult in array)
				{
					string itemID = ((programResult.ItemID != null && programResult.ItemID.Barcode != null) ? programResult.ItemID.Barcode : "UNKNOWN");
					if (programResult.ItemID != null && programResult.ItemID.IsEmpty())
					{
						itemID = "EMPTY";
					}
					else if (programResult.ItemID != null && programResult.ItemID.IsUnknown())
					{
						itemID = "UNKNOWN";
					}
					else if (programResult.ItemID != null && programResult.ItemID.RawResult == "UNDEFINED")
					{
						itemID = "UNDEFINED";
					}
					string previousItem = ((programResult.PreviousItem != null) ? programResult.PreviousItem.Barcode : "NONE");
					if (programResult.PreviousItem != null && programResult.PreviousItem.IsEmpty())
					{
						previousItem = "EMPTY";
					}
					else if (programResult.PreviousItem != null && programResult.PreviousItem.IsUnknown())
					{
						previousItem = "UNKNOWN";
					}
					list.Add(new HardwareResult
					{
						ID = programResult.ID.ToString(),
						ItemID = itemID,
						Message = programResult.Message,
						Code = programResult.Code,
						Deck = programResult.Deck,
						Slot = programResult.Slot,
						MetaData = ((programResult.ItemID != null) ? programResult.ItemID.Metadata : null),
						Timestamp = programResult.TimeStamp.ToString(),
						PreviousItem = previousItem,
						ReturnTime = ((!programResult.ReturnTime.HasValue) ? "NONE" : programResult.ReturnTime.ToString())
					});
				}
				hardwareResults = list.AsReadOnly();
			}
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList GetJobErrors(string jobId, out Redbox.KioskEngine.ComponentModel.ErrorList errors)
	{
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		errors = new Redbox.KioskEngine.ComponentModel.ErrorList();
		HardwareCommandResult job = Service.GetJob(jobId, out var job2);
		if (!job.Success)
		{
			job.Errors.AddToLocalErrorList(errorList);
		}
		else
		{
			job = job2.GetErrors(out var errors2);
			if (!job.Success)
			{
				job.Errors.AddToLocalErrorList(errorList);
			}
			else
			{
				errors2.AddToLocalErrorList(errors);
			}
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList ExecuteImmediate(string command, HardwareImmediateExecutionHandler eventHandler, bool clearStack)
	{
		Redbox.KioskEngine.ComponentModel.ErrorList result = new Redbox.KioskEngine.ComponentModel.ErrorList();
		ThreadPool.QueueUserWorkItem(delegate
		{
			try
			{
				Interlocked.Increment(ref m_working);
				HardwareCommandResult hardwareCommandResult = Service.ExecuteImmediate(command, out var job);
				if (eventHandler != null)
				{
					List<string> errorList = new List<string>();
					hardwareCommandResult.Errors.ForEach(delegate(Redbox.HAL.Core.Error error)
					{
						errorList.Add(error.Description);
					});
					Stack<string> stack = new Stack<string>();
					if (job != null)
					{
						job.GetStack(out stack);
						if (clearStack)
						{
							job.ClearStack();
						}
					}
					ServiceLocator.Instance.GetService<ICallbackService>()?.EnqueueCallback(new HardwareImmediateExecutionCallback
					{
						EventHandler = eventHandler,
						Command = command,
						Stack = stack,
						Errors = errorList,
						Success = hardwareCommandResult.Success
					});
				}
			}
			finally
			{
				Interlocked.Decrement(ref m_working);
			}
		});
		return result;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList ExecuteServiceCommand(string command)
	{
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		HardwareCommandResult hardwareCommandResult = Service.ExecuteServiceCommand(command);
		if (!hardwareCommandResult.Success)
		{
			hardwareCommandResult.Errors.AddToLocalErrorList(errorList);
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList GetSchedulerStatus(out string status)
	{
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		HardwareCommandResult schedulerStatus = Service.GetSchedulerStatus(out status);
		if (!schedulerStatus.Success)
		{
			schedulerStatus.Errors.AddToLocalErrorList(errorList);
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList GetHardwareJobs(out ReadOnlyCollection<IHardwareJob> hardwareJobs)
	{
		hardwareJobs = new List<IHardwareJob>().AsReadOnly();
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		HardwareCommandResult jobs = Service.GetJobs(out var jobs2);
		if (!jobs.Success)
		{
			jobs.Errors.AddToLocalErrorList(errorList);
		}
		else
		{
			if (jobs2 == null || jobs2.Length == 0)
			{
				return errorList;
			}
			List<IHardwareJob> list = new List<IHardwareJob>();
			Redbox.HAL.Client.HardwareJob[] array = jobs2;
			foreach (Redbox.HAL.Client.HardwareJob hardwareJob in array)
			{
				string startTime = ((!hardwareJob.StartTime.HasValue) ? null : hardwareJob.StartTime.Value.ToString("MM/dd/yyyy hh:mm tt"));
				string executionTime = ((!hardwareJob.ExecutionTime.HasValue) ? null : hardwareJob.ExecutionTime.Value.ToString());
				list.Add(new HardwareJob
				{
					ID = hardwareJob.ID,
					Label = hardwareJob.Label,
					StartTime = startTime,
					ExecutionTime = executionTime,
					ProgramName = hardwareJob.ProgramName,
					Status = hardwareJob.Status.ToString(),
					Priority = hardwareJob.Priority.ToString()
				});
			}
			hardwareJobs = list.AsReadOnly();
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList SuspendAllJobsForMaintenance()
	{
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		if (SuspendedMaintenanceModeJobs != null && SuspendedMaintenanceModeJobs.Length != 0)
		{
			return errorList;
		}
		HardwareCommandResult hardwareCommandResult = Service.SuspendJobsBelowPriority(HardwareJobPriority.Highest, out SuspendedMaintenanceModeJobs);
		if (!hardwareCommandResult.Success)
		{
			hardwareCommandResult.Errors.AddToLocalErrorList(errorList);
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList GetSuspendedMaintenanceModeJobs(out ReadOnlyCollection<IHardwareJob> suspendedJobs)
	{
		Redbox.KioskEngine.ComponentModel.ErrorList result = new Redbox.KioskEngine.ComponentModel.ErrorList();
		List<IHardwareJob> list = new List<IHardwareJob>();
		Redbox.HAL.Client.HardwareJob[] suspendedMaintenanceModeJobs = SuspendedMaintenanceModeJobs;
		foreach (Redbox.HAL.Client.HardwareJob hardwareJob in suspendedMaintenanceModeJobs)
		{
			string startTime = ((!hardwareJob.StartTime.HasValue) ? null : hardwareJob.StartTime.Value.ToString("MM/dd/yyyy hh:mm tt"));
			string executionTime = ((!hardwareJob.ExecutionTime.HasValue) ? null : hardwareJob.ExecutionTime.Value.ToString());
			list.Add(new HardwareJob
			{
				ID = hardwareJob.ID,
				Label = hardwareJob.Label,
				StartTime = startTime,
				ExecutionTime = executionTime,
				ProgramName = hardwareJob.ProgramName,
				Status = hardwareJob.Status.ToString(),
				Priority = hardwareJob.Priority.ToString()
			});
		}
		suspendedJobs = list.AsReadOnly();
		return result;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList ResumeAllMaintenanceSuspendedJobs()
	{
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		if (SuspendedMaintenanceModeJobs == null || SuspendedMaintenanceModeJobs.Length == 0)
		{
			return errorList;
		}
		HardwareCommandResult hardwareCommandResult = Service.ResumeJobs(SuspendedMaintenanceModeJobs);
		if (!hardwareCommandResult.Success)
		{
			hardwareCommandResult.Errors.AddToLocalErrorList(errorList);
		}
		SuspendedMaintenanceModeJobs = null;
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList GetInitStatus(out string status)
	{
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		HardwareCommandResult initStatus = Service.GetInitStatus(out status);
		if (!initStatus.Success)
		{
			initStatus.Errors.AddToLocalErrorList(errorList);
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList GetQuickReturnStatus(out string status)
	{
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		HardwareCommandResult quickReturnStatus = Service.GetQuickReturnStatus(out status);
		if (!quickReturnStatus.Success)
		{
			quickReturnStatus.Errors.AddToLocalErrorList(errorList);
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList NotifyMaintModeState(bool goingIntoMM)
	{
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		HardwareCommandResult hardwareCommandResult = Service.NotifyMaintModeState(goingIntoMM);
		if (!hardwareCommandResult.Success)
		{
			hardwareCommandResult.Errors.AddToLocalErrorList(errorList);
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList ReadFraudDisc(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, out string jobId)
	{
		jobId = null;
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		HardwareCommandResult hardwareCommandResult = Service.ReadFraudDisc(new HardwareJobSchedule
		{
			Priority = HardwareJobPriority.High
		}, out var job);
		if (!hardwareCommandResult.Success)
		{
			hardwareCommandResult.Errors.AddToLocalErrorList(errorList);
		}
		else
		{
			jobId = ConfigureJob(job, eventHandler, statusChangeHandler, errorList);
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList PrepForRedeploy(string[] decksToChange, HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, out string jobId)
	{
		jobId = null;
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		if (decksToChange.Length == 0)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("H004", "PrepForRedeploy requires at least one deck in the list.", "Correct the values provided so that at least one deck is provided."));
			return errorList;
		}
		HardwareCommandResult hardwareCommandResult = Service.PrepForRedeploy(decksToChange, new HardwareJobSchedule
		{
			Priority = HardwareJobPriority.Highest
		}, out var job);
		if (!hardwareCommandResult.Success)
		{
			hardwareCommandResult.Errors.AddToLocalErrorList(errorList);
		}
		else
		{
			jobId = ConfigureJob(job, eventHandler, statusChangeHandler, errorList);
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList PrepUnloadZone(int rotation, int last_deck, HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, out string jobId)
	{
		jobId = null;
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		HardwareCommandResult hardwareCommandResult = null;
		hardwareCommandResult = ((last_deck != 0) ? Service.PrepUnloadZone(rotation, last_deck, new HardwareJobSchedule
		{
			Priority = HardwareJobPriority.Highest
		}, out var job) : Service.PrepUnloadZone(rotation, new HardwareJobSchedule
		{
			Priority = HardwareJobPriority.Highest
		}, out job));
		if (!hardwareCommandResult.Success)
		{
			hardwareCommandResult.Errors.AddToLocalErrorList(errorList);
		}
		else
		{
			jobId = ConfigureJob(job, eventHandler, statusChangeHandler, errorList);
		}
		return errorList;
	}

	internal string ConfigureJob(Redbox.HAL.Client.HardwareJob job, HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, Redbox.KioskEngine.ComponentModel.ErrorList errors)
	{
		m_activeJobs.Add(job);
		job.EventRaised += delegate(Redbox.HAL.Client.HardwareJob j, DateTime timeStamp, string message)
		{
			try
			{
				ICallbackService service = ServiceLocator.Instance.GetService<ICallbackService>();
				if (eventHandler != null && service != null)
				{
					if (message == "ReturnEnd" || message == "DisableCancel")
					{
						HardwareEventCallback hardwareEventCallback = new HardwareEventCallback();
						hardwareEventCallback.EventHandler = eventHandler;
						hardwareEventCallback.TimeStamp = timeStamp.ToString();
						hardwareEventCallback.JobId = j.ID;
						hardwareEventCallback.Message = message;
						hardwareEventCallback.Invoke();
					}
					else
					{
						service.EnqueueCallback(new HardwareEventCallback
						{
							EventHandler = eventHandler,
							TimeStamp = timeStamp.ToString(),
							JobId = j.ID,
							Message = message
						});
					}
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in job.EventRaised handler.", e);
			}
		};
		job.StatusChanged += delegate(Redbox.HAL.Client.HardwareJob j, HardwareJobStatus jobStatus)
		{
			try
			{
				ICallbackService service = ServiceLocator.Instance.GetService<ICallbackService>();
				if (statusChangeHandler != null)
				{
					service?.EnqueueCallback(new HardwareStatusChangeCallback
					{
						EventHandler = statusChangeHandler,
						JobId = j.ID,
						Status = jobStatus.ToString()
					});
				}
				if (jobStatus == HardwareJobStatus.Stopped || jobStatus == HardwareJobStatus.Completed || jobStatus == HardwareJobStatus.Errored || jobStatus == HardwareJobStatus.Garbage)
				{
					LogHelper.Instance.Log("Disconnect from job '{0}', removing from active job list.", j.ID);
					j.Disconnect();
					m_activeJobs.Remove(j);
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in job.StatusChanged handler.", e);
			}
		};
		HardwareCommandResult stack = job.GetStack(out var stack2);
		if (!stack.Success)
		{
			stack.Errors.AddToLocalErrorList(errors);
			LogHelper.Instance.Log("The call to HAL's GetStack failed.");
		}
		else
		{
			LogHelper.Instance.Log("HAL stack for job: {0}, script: {1}", job.ID, job.ProgramName);
			foreach (string item in stack2)
			{
				LogHelper.Instance.Log("...{0}", item);
			}
			if (stack2.Count == 0)
			{
				LogHelper.Instance.Log("...The HAL stack is empty for this job.");
			}
		}
		stack = job.Connect();
		if (!stack.Success)
		{
			stack.Errors.AddToLocalErrorList(errors);
		}
		stack = job.Resume();
		if (!stack.Success)
		{
			stack.Errors.AddToLocalErrorList(errors);
		}
		return job.ID;
	}

	private HardwareService()
	{
	}
}
