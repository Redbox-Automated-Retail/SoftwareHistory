using System;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.TicketServices.Proxy.ComponentModel;

namespace Redbox.TicketServices.Kernel;

internal class TicketDataServiceFunctions
{
	[KernelFunction(Name = "kernel.ticketsvcs_selltickets")]
	internal static bool TicketsSellTickets()
	{
		try
		{
			return ServiceLocator.Instance.GetService<ITicketDataService>()?.SellTickets ?? false;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was raised in TicketServiceFunctions.TicketsSellTickets()", e);
			return false;
		}
	}

	[KernelFunction(Name = "kernel.ticketsvcs_issellticketson")]
	internal static bool TicketsIsSellTicketsOn()
	{
		try
		{
			return ServiceLocator.Instance.GetService<ITicketDataService>()?.IsSellTicketsOn ?? false;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was raised in TicketServiceFunctions.TicketsIsSellTicketsOn()", e);
			return false;
		}
	}

	[KernelFunction(Name = "kernel.ticketsvcs_registerimagefromcache")]
	internal static string TicketsRegisterImageFromCache(int ownerTypeId, int subTypeId, long ownerId)
	{
		ITicketDataService service = ServiceLocator.Instance.GetService<ITicketDataService>();
		if (service == null)
		{
			return null;
		}
		IBitmapCacheService service2 = ServiceLocator.Instance.GetService<IBitmapCacheService>();
		if (service2 == null)
		{
			return null;
		}
		ErrorList imageName = service.GetImageName(ownerTypeId, subTypeId, ownerId, out var imageName2);
		if (imageName.ContainsError() || imageName2 == null)
		{
			imageName.ToLogHelper();
			return null;
		}
		string name = $"cache:{imageName2}";
		if (service2.GetImage(name) != null)
		{
			return imageName2;
		}
		imageName = service.GetImage(imageName2, out var data);
		if (imageName.ContainsError() || data == null)
		{
			imageName.ToLogHelper();
			return null;
		}
		service2.RegisterImage(imageName2, data);
		return imageName2;
	}

	[KernelFunction(Name = "kernel.ticketsvcs_datatablegetall")]
	internal static LuaTable TicketsDataTableGetAll(string tableName)
	{
		LogHelper.Instance.Log("Getting data for ticket table: {0}", tableName);
		try
		{
			ITicketDataService service = ServiceLocator.Instance.GetService<ITicketDataService>();
			if (service == null)
			{
				return null;
			}
			ErrorList tableData = service.GetTableData(tableName, out var data);
			if (tableData.Count > 0)
			{
				tableData.ToLogHelper();
				return null;
			}
			if (data == null)
			{
				LogHelper.Instance.Log($"Error in TicketDataServiceFunctions.TicketDataTableGetAll, GetTableData returned null for table: {tableName}");
				return null;
			}
			LuaTable result = data.ToLuaDataTable();
			LogHelper.Instance.Log("Finished getting table {0} and converting to a lua table rows: {1}", tableName, data.Data.Count);
			return result;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An exception was raised in TicketServiceFunctions.TicketDataTableGetAll({tableName}).", e);
			return null;
		}
	}

	[KernelFunction(Name = "kernel.ticketsvcs_getbrowseevents")]
	internal static LuaTable TicketsGetBrowseEvents()
	{
		try
		{
			ITicketDataService service = ServiceLocator.Instance.GetService<ITicketDataService>();
			if (service == null)
			{
				return null;
			}
			ErrorList browseEvents = service.GetBrowseEvents(out var events);
			if (browseEvents.Count > 0)
			{
				browseEvents.ToLogHelper();
				return null;
			}
			browseEvents = service.GetTableData("Events", out var data);
			if (browseEvents.Count > 0)
			{
				browseEvents.ToLogHelper();
				return null;
			}
			return data.ToLuaTable(events);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was raised in TicketServiceFunctions.TicketsGetBrowseEvents()", e);
			return null;
		}
	}

	[KernelFunction(Name = "kernel.ticketsvcs_getcarouselevents")]
	internal static LuaTable TicketsGetCarouselEvents()
	{
		try
		{
			ITicketDataService service = ServiceLocator.Instance.GetService<ITicketDataService>();
			if (service == null)
			{
				return null;
			}
			ErrorList carouselEvents = service.GetCarouselEvents(out var events);
			if (carouselEvents.Count > 0)
			{
				carouselEvents.ToLogHelper();
				return null;
			}
			carouselEvents = service.GetTableData("Events", out var data);
			if (carouselEvents.Count > 0)
			{
				carouselEvents.ToLogHelper();
				return null;
			}
			return data.ToLuaTable(events);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was raised in TicketServiceFunctions.TicketsGetCarouselEvents()", e);
			return null;
		}
	}

	[KernelFunction(Name = "kernel.ticketsvcs_refreshifneeded")]
	internal static bool TicketsRefreshIfNeeded()
	{
		try
		{
			return ServiceLocator.Instance.GetService<ITicketDataService>()?.RefreshIfNeeded() ?? false;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was raised in TicketServiceFunctions.TicketsRefreshIfNeeded()", e);
			return false;
		}
	}

	[KernelFunction(Name = "kernel.ticketsvcs_refreshviews")]
	internal static bool TicketsRefreshViews(string browseFilter, string sort)
	{
		try
		{
			return ServiceLocator.Instance.GetService<ITicketDataService>()?.RefreshViews(browseFilter, sort) ?? false;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was raised in TicketServiceFunctions.TicketsRefreshViews()", e);
			return false;
		}
	}

	[KernelFunction(Name = "kernel.ticketsvcs_excludeproduction")]
	internal static bool TicketsExcludeProduction(long production_id)
	{
		try
		{
			IExcludeProductions service = ServiceLocator.Instance.GetService<IExcludeProductions>();
			if (service == null)
			{
				return false;
			}
			service.Add(production_id);
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was raised in TicketServiceFunctions.TicketsExcludeProduction()", e);
			return false;
		}
	}
}
