using System;
using System.Collections.Generic;
using System.Globalization;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Kernel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Local;

namespace Redbox.Rental.Data;

public class LocalData : ILocalData
{
	public class Constants
	{
		public const string BadCardList = "BadCardList";

		public const string NumberOfAttempts = "number_of_attempts";

		public const string LastAttempt = "last_attempt";

		public const string LastAttemptFormat = "yyyyMMddHHmm";

		public const string Local = "Local";
	}

	public ILocalDataInstance LoadData()
	{
		LocalDataInstance localDataInstance = new LocalDataInstance(this);
		LoadBadCardList(localDataInstance);
		return localDataInstance;
	}

	public void UpdateBadCard(IBadCard badCard)
	{
		if (badCard != null)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			if (badCard.NumberOfAttempts.HasValue)
			{
				dictionary["number_of_attempts"] = badCard.NumberOfAttempts.Value;
			}
			if (badCard.LastAttempt.HasValue)
			{
				dictionary["last_attempt"] = badCard.LastAttempt.Value.ToString("yyyyMMddHHmm");
			}
			string value = VistaDBStringHelper.ToString(dictionary);
			Set("BadCardList", badCard.HashId, value);
		}
	}

	public void RemoveBadCard(IBadCard badCard)
	{
		if (badCard != null && badCard.HashId != null)
		{
			Remove("BadCardList", badCard.HashId);
		}
	}

	private void LoadBadCardList(LocalDataInstance instance)
	{
		GetAll("BadCardList").ForEach(delegate(KeyValuePair<string, string> item)
		{
			TryCatchData("LocalData.LoadBadCardList", item, delegate
			{
				string key = item.Key;
				if (!instance.BadCardList.ContainsKey(key))
				{
					IBadCard badCard = new BadCard
					{
						HashId = item.Key
					};
					Dictionary<string, object> dictionary = item.Value.ParseLuaValue();
					int result = 0;
					if (dictionary.ContainsKey("number_of_attempts") && int.TryParse(dictionary["number_of_attempts"].ToString(), out result))
					{
						badCard.NumberOfAttempts = result;
					}
					if (dictionary.ContainsKey("last_attempt") && DateTime.TryParseExact(dictionary["last_attempt"].ToString(), "yyyyMMddHHmm", new CultureInfo("en-US"), DateTimeStyles.None, out var result2))
					{
						badCard.LastAttempt = result2;
					}
					instance.BadCardList[badCard.HashId] = badCard;
				}
			});
		});
	}

	private void TryCatchData(string name, KeyValuePair<string, string> pair, Action action)
	{
		try
		{
			action();
		}
		catch (Exception e)
		{
			try
			{
				LogHelper.Instance.Log($"{name} - Unhandled exception occurred, skipping {$"Key: {pair.Key} Value: {pair.Value}"}", e);
			}
			catch (Exception e2)
			{
				LogHelper.Instance.Log("LocalData.TryCatchData - An unhandled exception ocurred", e2);
			}
		}
	}

	private Dictionary<string, string> GetAll(string tableName)
	{
		Dictionary<string, string> dict = new Dictionary<string, string>();
		try
		{
			IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
			if (service == null)
			{
				return dict;
			}
			Dictionary<string, object> all = service.GetAll("Local", tableName);
			if (all == null)
			{
				return dict;
			}
			all.ForEach(delegate(KeyValuePair<string, object> item)
			{
				if (item.Value is string)
				{
					dict[item.Key] = Convert.ToString(item.Value);
				}
			});
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"LocalData.GetAll{tableName} - An unhandled exception ocurred", e);
		}
		return dict;
	}

	private void Set(string tableName, string keyName, string value)
	{
		try
		{
			ServiceLocator.Instance.GetService<IDataStoreService>()?.Set("Local", tableName, keyName, value);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log(string.Format("An exception was raised in LocalData.Set({0},{1},{2}).", "Local", tableName, keyName), e);
			EnvironmentFunctions.HandleDataStoreErrors("Local");
		}
	}

	private void Remove(string tableName, string keyName)
	{
		try
		{
			ServiceLocator.Instance.GetService<IDataStoreService>()?.Remove("Local", tableName, keyName);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log(string.Format("An exception was raised in LocalData.Remove({0},{1},{2}).", "Local", tableName, keyName), e);
			EnvironmentFunctions.HandleDataStoreErrors("Local");
		}
	}
}
