using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.TicketServices.Proxy.ComponentModel;

namespace Redbox.TicketServices.Proxy;

internal class TicketDataService : ITicketDataService
{
	private ManualResetEvent _manualResetEvent;

	private bool IsFirstTime = true;

	private TicketsContext _ticketsContext;

	private TicketsContext _ticketsStagedContext;

	private int _inRefresh;

	private Timer _changeTimer;

	private readonly object _lock = new object();

	public static TicketDataService Instance => Singleton<TicketDataService>.Instance;

	public bool SellTickets
	{
		get
		{
			if (!IsSellTicketsOn)
			{
				return false;
			}
			if (!TicketsFile.GetHasActive())
			{
				return false;
			}
			WaitForFirstLoad();
			bool result = false;
			ErrorList errorList = new ErrorList();
			try
			{
				lock (_lock)
				{
					result = _ticketsContext.Top80Events.Count() > 0;
				}
			}
			catch (Exception e)
			{
				errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("TDS0061", "Unhandled exception occurred in TicketDataService.SellTickets", e));
			}
			errorList.ToLogHelper();
			return result;
		}
	}

	public bool IsSellTicketsOn
	{
		get
		{
			IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
			if (service == null)
			{
				return false;
			}
			if (service.TryGetValue<bool>("system", "General", "SellTickets", out var value))
			{
				return value;
			}
			return false;
		}
	}

	private bool HasStagedContext => _ticketsStagedContext != null;

	private bool HasActiveContext => _ticketsContext != null;

	public ErrorList Initialize()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			LogHelper.Instance.Log("Tickets data service initialization");
			ServiceLocator.Instance.AddService(typeof(ITicketDataService), Instance);
			_manualResetEvent = new ManualResetEvent(initialState: false);
			ThreadPool.QueueUserWorkItem(delegate
			{
				Refresh();
			});
			if (_changeTimer == null)
			{
				_changeTimer = new Timer(delegate
				{
					Refresh();
				});
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("TDS001", "Unhandled exception occurred in TicketDataService.Initialize", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	public ErrorList Start()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			LogHelper.Instance.Log("Tickets data service start");
			_changeTimer.Change(new TimeSpan(0L), new TimeSpan(0, 0, 120));
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("TDS002", "Unhandled exception occurred in TicketDataService.Start", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	public ErrorList Stop()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			LogHelper.Instance.Log("Tickets data service stop");
			_changeTimer.Change(-1, -1);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("TDS003", "Unhandled exception occurred in TicketDataService.Stop", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	public ErrorList GetTableData(string tableName, out ITicketDataTable data)
	{
		WaitForFirstLoad();
		LogHelper.Instance.Log("Tickets data service GetTableData({0})", tableName);
		data = null;
		ErrorList errorList = new ErrorList();
		try
		{
			lock (_lock)
			{
				if (!_ticketsContext.TableData.ContainsKey(tableName))
				{
					errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("TDS004", "Unhandled exception", $"Error in TicketDataService.GetTableData table:{tableName} does not exist in collection."));
				}
				data = _ticketsContext.TableData[tableName];
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("TDS005", "Unhandled exception occurred in TicketDataService.GetTableData", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	public ErrorList GetImageName(int ownerTypeId, int subTypeId, long ownerId, out string imageName)
	{
		imageName = null;
		ErrorList errorList = new ErrorList();
		try
		{
			lock (_lock)
			{
				ImageInfo imageInfo = _ticketsContext.ImageInfos.FirstOrDefault((ImageInfo img) => img.OwnerTypeId == ownerTypeId && img.OwnerId == ownerId && img.SubTypeId == subTypeId);
				if (imageInfo == null)
				{
					errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("TDS006", $"Image not found for OwnerTypeId: {ownerTypeId} SubTypeId {subTypeId} OwnerId: {ownerId}", ""));
				}
				else
				{
					imageName = imageInfo.Hash;
				}
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("TDS007", "Unhandled exception occurred in TicketDataService.GetImageName", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	public ErrorList GetImage(string imageName, out byte[] data)
	{
		ErrorList errorList = new ErrorList();
		data = null;
		try
		{
			string path = TicketsFile.CacheLocation + imageName;
			if (!File.Exists(path))
			{
				errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("TDS008", $"Image {imageName} was not found in directory {TicketsFile.CacheLocation}", ""));
				errorList.ToLogHelper();
				return errorList;
			}
			using FileStream fileStream = File.OpenRead(path);
			using BinaryReader binaryReader = new BinaryReader(fileStream);
			data = binaryReader.ReadBytes((int)fileStream.Length);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("TDS009", "Unhandled exception occurred in TicketDataService.GetImage", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	public ErrorList GetBrowseEvents(out List<long> events)
	{
		LogHelper.Instance.Log("Tickets data service GetBrowseEvents");
		events = null;
		ErrorList errorList = new ErrorList();
		try
		{
			lock (_lock)
			{
				events = _ticketsContext.BrowseEvents;
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("TDS006", "Unhandled exception occurred in TicketDataService.GetBrowseEvents", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	public ErrorList GetCarouselEvents(out List<long> events)
	{
		LogHelper.Instance.Log("Tickets data service GetCarouselEvents");
		events = null;
		ErrorList errorList = new ErrorList();
		try
		{
			lock (_lock)
			{
				events = _ticketsContext.CarouselEvents;
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("TDS006", "Unhandled exception occurred in TicketDataService.GetCarouselEvents", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	public bool RefreshIfNeeded()
	{
		WaitForFirstLoad();
		bool result = false;
		ErrorList errorList = new ErrorList();
		try
		{
			lock (_lock)
			{
				if (HasStagedContext)
				{
					_ticketsContext = _ticketsStagedContext;
					_ticketsStagedContext = null;
					ExcludedProductions.Instance.RemoveAll();
					return true;
				}
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("TDS010", "Unhandled exception occurred in TicketDataService.RefreshIfNeeded", e));
		}
		errorList.ToLogHelper();
		return result;
	}

	public bool RefreshViews(string browseFilter, string sort)
	{
		WaitForFirstLoad();
		bool result = false;
		ErrorList errorList = new ErrorList();
		try
		{
			lock (_lock)
			{
				_ticketsContext.RefreshViews(browseFilter, sort);
				result = true;
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("TDS011", "Unhandled exception occurred in TicketDataService.RefreshViews", e));
		}
		errorList.ToLogHelper();
		return result;
	}

	private void WaitForFirstLoad()
	{
		if (_manualResetEvent == null)
		{
			return;
		}
		try
		{
			LogHelper.Instance.Log("Tickets data service wait for initial data refresh");
			_manualResetEvent.WaitOne(new TimeSpan(0, 5, 0));
			_manualResetEvent.Close();
			_manualResetEvent = null;
			LogHelper.Instance.Log("Tickets data service finished waiting for initial data refresh");
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Unhandled exception occurred in TicketDataService.Wait", e);
		}
		finally
		{
			_manualResetEvent = null;
		}
	}

	private void Refresh()
	{
		if (Interlocked.CompareExchange(ref _inRefresh, 1, 0) == 1)
		{
			LogHelper.Instance.Log("Tickets data service Refresh - exiting already in refresh");
			return;
		}
		try
		{
			if (IsSellTicketsOn)
			{
				RefreshData();
			}
		}
		finally
		{
			_inRefresh = 0;
			if (_manualResetEvent != null)
			{
				_manualResetEvent.Set();
			}
		}
	}

	private void RefreshData()
	{
		bool flag = false;
		if (TicketsFile.GetHasStaged())
		{
			flag = TicketsFile.Activate();
		}
		if (!TicketsFile.GetHasActive() || (HasActiveContext && !flag))
		{
			return;
		}
		LogHelper.Instance.Log("Tickets data service RefreshData");
		try
		{
			if (IsFirstTime)
			{
				IsFirstTime = false;
				if (!TicketsFile.ExtractImagesToCache())
				{
					return;
				}
			}
			TicketsContext ticketsContext = TicketsContext.GetTicketsContext();
			lock (_lock)
			{
				if (_ticketsContext == null)
				{
					_ticketsContext = ticketsContext;
					_ticketsStagedContext = _ticketsContext;
				}
				else
				{
					_ticketsStagedContext = ticketsContext;
				}
			}
			LogHelper.Instance.Log("Tickets data service RefreshData complete");
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Unhandled exception occurred in TicketDataService.RefreshData", e);
		}
	}

	private TicketDataService()
	{
	}
}
