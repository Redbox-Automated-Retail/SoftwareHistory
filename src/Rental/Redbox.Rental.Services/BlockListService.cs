using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Redbox.Core;
using Redbox.Rental.Model;

namespace Redbox.Rental.Services;

public class BlockListService : IBlockList
{
	private IBlockListData _blockListData;

	private IStoreManager _storeManager;

	public BlockListService(IStoreManager storeManager, IBlockListData blockListData)
	{
		_storeManager = storeManager;
		_blockListData = blockListData;
	}

	public bool IsNameInBlockList(string firstName, string lastName, int market_id, string zipCode)
	{
		try
		{
			long kioskId = _storeManager.KioskId;
			DateTime now = DateTime.Now;
			string text = $"{firstName} {lastName}".Trim();
			string text2 = $"{lastName} {firstName}".Trim();
			string pattern = string.Empty;
			string pattern2 = string.Empty;
			if (!string.IsNullOrEmpty(text))
			{
				pattern = Encoding.ASCII.GetBytes(text.ToLower()).ToASCIISHA1Hash();
			}
			if (!string.IsNullOrEmpty(text2))
			{
				pattern2 = Encoding.ASCII.GetBytes(text2.ToLower()).ToASCIISHA1Hash();
			}
			if (CheckPattern(pattern, pattern2, kioskId, market_id, zipCode))
			{
				return true;
			}
			TimeSpan timeSpan = DateTime.Now - now;
			LogHelper.Instance.Log("BlockListService.IsNameInBlockList complete.  Milliseconds: {0}", timeSpan.TotalMilliseconds);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("BlockListService.IsInBlockList: An unhandled exception occurred.", e);
		}
		return false;
	}

	public void CheckForNewFile()
	{
		_blockListData.CheckForNewFile();
	}

	public DateTime? GetVersion()
	{
		return _blockListData.GetVersion();
	}

	private bool CheckPattern(string pattern1, string pattern2, long kioskId, int market_id, string zipCode)
	{
		IEnumerable<BlackList> blackList = _blockListData.GetBlackList(pattern1, pattern2);
		if (blackList.Count() == 0)
		{
			return false;
		}
		if (blackList.Any((BlackList bl) => bl.BlacklistTypeID == BlackListType.All))
		{
			return true;
		}
		if (blackList.Any((BlackList bl) => bl.PatternHash.Equals(pattern1) && bl.BlacklistTypeID == BlackListType.Market && bl.Value.Equals(market_id.ToString())))
		{
			return true;
		}
		if (blackList.Any((BlackList bl) => bl.PatternHash.Equals(pattern2) && bl.BlacklistTypeID == BlackListType.Market && bl.Value.Equals(market_id.ToString())))
		{
			return true;
		}
		if (blackList.Any((BlackList bl) => bl.PatternHash.Equals(pattern1) && bl.BlacklistTypeID == BlackListType.ZipCode && bl.Value.Equals(zipCode)))
		{
			return true;
		}
		if (blackList.Any((BlackList bl) => bl.PatternHash.Equals(pattern2) && bl.BlacklistTypeID == BlackListType.ZipCode && bl.Value.Equals(zipCode)))
		{
			return true;
		}
		if (blackList.Any((BlackList bl) => bl.PatternHash.Equals(pattern1) && bl.BlacklistTypeID == BlackListType.Kiosk && bl.Value.Equals(kioskId.ToString())))
		{
			return true;
		}
		if (blackList.Any((BlackList bl) => bl.PatternHash.Equals(pattern2) && bl.BlacklistTypeID == BlackListType.Kiosk && bl.Value.Equals(kioskId.ToString())))
		{
			return true;
		}
		return false;
	}
}
