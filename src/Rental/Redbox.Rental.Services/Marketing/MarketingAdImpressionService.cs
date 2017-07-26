using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Marketing;

namespace Redbox.Rental.Services.Marketing;

public class MarketingAdImpressionService : IMarketingAdImpressionService
{
	private List<MarketingAdImpression> _marketingAdImpressions = new List<MarketingAdImpression>();

	private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

	private int _lockTimeout = 3000;

	private Timer _changeTimer;

	private int _inRefresh;

	private string _impressionFileName;

	private IStoreManager _storeManagerService;

	public MarketingAdImpressionService(IStoreManager storeManagerService)
	{
		_storeManagerService = storeManagerService;
		string path = Path.Combine(Path.GetDirectoryName(typeof(MarketingAdImpressionService).Assembly.Location), "..\\data");
		_impressionFileName = Path.Combine(path, "marketingadimpressions.json");
		if (_changeTimer == null)
		{
			_changeTimer = new Timer(delegate
			{
				OnTimer();
			});
		}
	}

	public void AddImpression(MarketingAdCampaign marketingAdCampaign, MarketingAdLocation marketingAdLocation)
	{
		Task.Run(delegate
		{
			if (!_lock.TryEnterWriteLock(_lockTimeout))
			{
				LogHelper.Instance.LogError("MarketingAdImpressionService.AddImpression", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
				return;
			}
			try
			{
				DateTime dt = DateTime.Now;
				MarketingAdImpression marketingAdImpression = _marketingAdImpressions.Where((MarketingAdImpression item) => item.CampaignId == marketingAdCampaign.Id && item.AdLocation == marketingAdLocation && item.ImpressionStartDate <= dt && item.ImpressionEndDate > dt).FirstOrDefault();
				if (marketingAdImpression == null)
				{
					marketingAdImpression = new MarketingAdImpression
					{
						CampaignId = marketingAdCampaign.Id,
						Name = marketingAdCampaign.Name,
						StartDate = marketingAdCampaign.StartDate,
						EndDate = marketingAdCampaign.EndDate,
						StartTime = marketingAdCampaign.StartTime,
						EndTime = marketingAdCampaign.EndTime,
						DaysOfWeek = (marketingAdCampaign.DaysOfWeek.HasValue ? new int?((int)marketingAdCampaign.DaysOfWeek.Value) : ((int?)null)),
						AdLocation = marketingAdLocation,
						ImpressionStartDate = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0),
						ImpressionEndDate = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0).AddHours(1.0),
						Count = 0
					};
					_marketingAdImpressions.Add(marketingAdImpression);
				}
				marketingAdImpression.Count++;
			}
			catch (Exception e)
			{
				LogHelper.Instance.LogException("MarketingAdImpressionService.AddImpression - an exception occurred.", e);
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		});
	}

	public void Start()
	{
		try
		{
			LogHelper.Instance.Log("MarketingAdImpressionService.Start");
			LoadImpressions();
			int minutes = new Random().Next(5, 55);
			_changeTimer.Change(new TimeSpan(0, minutes, 0), new TimeSpan(4, minutes, 0));
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("MarketingAdImpressionService.Start - an exception occurred.", e);
		}
	}

	public void Stop()
	{
		try
		{
			LogHelper.Instance.Log("MarketingAdImpressionService.Stop");
			SaveImpressions();
			_changeTimer.Change(-1, -1);
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("MarketingAdImpressionService.Stop - an exception occurred.", e);
		}
	}

	private void OnTimer()
	{
		if (Interlocked.CompareExchange(ref _inRefresh, 1, 0) == 1)
		{
			LogHelper.Instance.Log("MarketingAdImpressionService.Refresh - exiting already in OnTimer");
			return;
		}
		try
		{
			UnloadImpressions();
		}
		finally
		{
			_inRefresh = 0;
		}
	}

	private void UnloadImpressions()
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("MarketingAdImpressionService.UnloadImpressions", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return;
		}
		try
		{
			PerformUnload();
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	private void PerformUnload()
	{
		IPromotionServices service = ServiceLocator.Instance.GetService<IPromotionServices>();
		if (service == null)
		{
			LogHelper.Instance.Log("MarketingAdImpressionService.PerformUnload - IPromotionServices was not found");
			return;
		}
		List<MarketingAdImpression> list = _marketingAdImpressions.Where((MarketingAdImpression item) => item.ImpressionEndDate < DateTime.Now).ToList();
		if (list.Count != 0)
		{
			list.ForEach(delegate(MarketingAdImpression item)
			{
				_marketingAdImpressions.Remove(item);
			});
			LogHelper.Instance.Log("MarketingAdImpressionService.PerformUnload = Count {0}", list.Count);
			List<MarketingAdImpressionData> adData = new List<MarketingAdImpressionData>();
			list.ForEach(delegate(MarketingAdImpression item)
			{
				adData.Add(new MarketingAdImpressionData
				{
					CampaignId = item.CampaignId,
					Name = item.Name,
					StartDate = item.StartDate,
					EndDate = item.EndDate,
					StartTime = item.StartTime,
					EndTime = item.EndTime,
					DaysOfWeek = item.DaysOfWeek,
					AdLocation = (int)item.AdLocation,
					Count = item.Count,
					ImpressionStartDate = item.ImpressionStartDate,
					ImpressionEndDate = item.ImpressionEndDate
				});
			});
			service.MarketingAdImpressions(_storeManagerService.KioskId, adData);
		}
	}

	private void SaveImpressions()
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("MarketingAdImpressionService.SaveImpressions", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return;
		}
		try
		{
			WriteData(_marketingAdImpressions);
			_marketingAdImpressions.Clear();
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	private void LoadImpressions()
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("MarketingAdImpressionService.LoadImpressions", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return;
		}
		try
		{
			List<MarketingAdImpression> list = ReadData();
			if (list != null)
			{
				list.ForEach(delegate(MarketingAdImpression item)
				{
					_marketingAdImpressions.Add(item);
				});
				LogHelper.Instance.Log("MarketingAdImpressionService.LoadImpressions - count {0}", list.Count);
			}
			DeleteData();
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	private void WriteData(List<MarketingAdImpression> data)
	{
		try
		{
			if (data.Count == 0)
			{
				DeleteData();
				return;
			}
			LogHelper.Instance.Log("MarketingAdImpressionService.WriteData = Writing impressions - count {0}", data.Count);
			string s = data.ToJson();
			byte[] bytes = Encoding.ASCII.GetBytes(s);
			File.WriteAllBytes(_impressionFileName, bytes);
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException($"MarketingAdImpressionService.WriteData - an exception occurred writing {_impressionFileName}.", e);
		}
	}

	private List<MarketingAdImpression> ReadData()
	{
		try
		{
			if (!File.Exists(_impressionFileName))
			{
				return null;
			}
			byte[] bytes = File.ReadAllBytes(_impressionFileName);
			return Encoding.ASCII.GetString(bytes).ToObject<List<MarketingAdImpression>>();
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException($"MarketingAdImpressionService.ReadData - an exception occurred reading {_impressionFileName}.", e);
		}
		return null;
	}

	private void DeleteData()
	{
		try
		{
			if (File.Exists(_impressionFileName))
			{
				File.Delete(_impressionFileName);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException($"MarketingAdImpressionService.DeleteData - an exception occurred deleting {_impressionFileName}.", e);
		}
	}
}
