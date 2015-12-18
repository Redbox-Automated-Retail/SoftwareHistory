using System;
using System.Collections.Generic;
using System.Globalization;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;

namespace Redbox.Rental.Services;

public class InventoryService : IInventory
{
	private IInventoryData _inventoryData;

	private int _random;

	public DateTime? InventoryActionModificationTime => _inventoryData.GetInventoryActionModificationTime();

	public DateTime? RapidStatusModificationTime => _inventoryData.GetRapidStatusModificationTime();

	public InventoryService(IInventoryData inventoryData)
	{
		_inventoryData = inventoryData;
		Random random = new Random();
		_random = random.Next(0, 30);
	}

	public Inventory GetRapidStatus(string barcode)
	{
		try
		{
			return _inventoryData.GetRapidStatus(barcode);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("InventoryService.GetRapidStatus - An unhandled exception occurred.", e);
			return null;
		}
	}

	public ProductLookupCatalogModel GetRapidStatus()
	{
		try
		{
			return _inventoryData.GetRapidStatus();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("InventoryService.GetRapidStatus - An unhandled exception occurred.", e);
			return null;
		}
	}

	public List<RebalancesInModel> GetRebalancesIn()
	{
		List<RebalancesInModel> result = new List<RebalancesInModel>();
		try
		{
			InventoryActionFile inventoryActionFile = _inventoryData.GetInventoryActionFile();
			if (inventoryActionFile == null || inventoryActionFile.RebalancesIn == null)
			{
				return result;
			}
			inventoryActionFile.RebalancesIn.ForEach(delegate(RebalancesIn item)
			{
				result.Add(new RebalancesInModel
				{
					TitleId = item.Id,
					Quantity = item.Qty
				});
			});
			return result;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("InventoryService.GetRebalancesIn - An unhandled exception occurred.", e);
			return result;
		}
	}

	public int GetFillQuantity()
	{
		int num = 0;
		try
		{
			return _inventoryData.GetInventoryActionFile()?.FillQty ?? num;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("InventoryService.GetFillQuantity - An unhandled exception occurred.", e);
			return num;
		}
	}

	public void CheckForNewFiles(Action inventoryCompletion, Action newRapidStatus)
	{
		try
		{
			LogHelper.Instance.Log("InventoryService.CheckForNewFiles - starting");
			_inventoryData.CheckForNewFiles(out var rapidStatusChanged);
			if (rapidStatusChanged)
			{
				newRapidStatus?.Invoke();
			}
			InventoryActionFile inventoryActionFile = _inventoryData.GetInventoryActionFile();
			if (inventoryActionFile == null)
			{
				LogHelper.Instance.Log("InventoryService.CheckForNewFiles - no inventory action completed file exists.");
				return;
			}
			DateTime dateTime = DateTime.ParseExact(inventoryActionFile.CompletedDate, "yyyyMMddHHmmss", CultureInfo.CurrentCulture);
			DateTime? lastCompletedDate = _inventoryData.GetLastCompletedDate();
			if (lastCompletedDate.HasValue)
			{
				DateTime value = dateTime;
				DateTime? dateTime2 = lastCompletedDate;
				if (!(value != dateTime2))
				{
					return;
				}
			}
			LogHelper.Instance.Log("InventoryService.CheckForNewFiles - New completed date found, running thinjob now.");
			_inventoryData.SetLastCompletedDate(dateTime);
			_inventoryData.SetLastRunThinJobDate(DateTime.Now);
			inventoryCompletion?.Invoke();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("InventoryService.CheckForNewFiles - An unhandled exception occurred.", e);
		}
		finally
		{
			LogHelper.Instance.Log("InventoryService.CheckForNewFiles - finished");
			RunByDeadline(inventoryCompletion);
		}
	}

	private void RunByDeadline(Action inventoryCompletion)
	{
		try
		{
			DateTime dateTime = DateTime.Today.AddHours(3.0).AddMinutes(30 + _random);
			DateTime? lastRunThinJobDate = _inventoryData.GetLastRunThinJobDate();
			if (dateTime.DayOfWeek == DayOfWeek.Saturday || dateTime.DayOfWeek == DayOfWeek.Sunday)
			{
				LogHelper.Instance.Log("InventoryService.RunByDeadline - auto thin job does not run on Saturday or Sunday");
				return;
			}
			LogHelper.Instance.Log("InventoryService.RunByDeadline - Runby: {0} LastRunThinJob {1}", dateTime, lastRunThinJobDate);
			if ((!lastRunThinJobDate.HasValue || !lastRunThinJobDate.Value.Date.Equals(DateTime.Today)) && DateTime.Now >= dateTime)
			{
				LogHelper.Instance.Log("InventoryService.RunByDeadline - Thinjob has not run by deadine, running now.");
				_inventoryData.SetLastRunThinJobDate(DateTime.Now);
				inventoryCompletion?.Invoke();
			}
			else
			{
				LogHelper.Instance.Log("InventoryService.RunByDeadline - Thinjob has run by deadine or it has not passed the deadline yet.");
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("InventoryService.RunByDeadline - An unhandled exception occurred.", e);
		}
		finally
		{
			LogHelper.Instance.Log("InventoryService.RunByDeadline - finished");
		}
	}
}
