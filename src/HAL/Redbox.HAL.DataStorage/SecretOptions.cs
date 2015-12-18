using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.DataStorage;

internal sealed class SecretOptions : IPersistentMap
{
	private readonly IDataStore Helper;

	public void Remove(string key)
	{
		string deleteQuery = string.Format("DELETE FROM [{0}] WHERE OPTIONNAME='{1}'", "SecretOptions_v2", key);
		Helper.Delete(deleteQuery);
	}

	public T GetValue<T>(string name, T defVal)
	{
		string text = LoadData(name);
		if (text == null)
		{
			InsertOption(name, defVal.ToString());
			return defVal;
		}
		return (T)ConversionHelper.ChangeType(text, typeof(T));
	}

	public void SetValue<T>(string key, T value)
	{
		try
		{
			string text = LoadData(key);
			if (text != null)
			{
				UpdateOption(key, value.ToString());
			}
			else
			{
				InsertOption(key, value.ToString());
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("WARNING: SecretOption.SetValueInner caught an exception.", e);
		}
	}

	internal SecretOptions(IDataStore store)
	{
		Helper = store;
	}

	private bool InsertOption(string key, string data)
	{
		string insert = string.Format("INSERT INTO [{0}] (OPTIONNAME,OPTIONVALUE,OPTIONENCRYPTED) VALUES('{1}','{2}',{3})", "SecretOptions_v2", key, data, false);
		return Helper.Insert(insert);
	}

	private bool UpdateOption(string key, string data)
	{
		string update = string.Format("UPDATE [{0}] SET [OPTIONVALUE]= '{1}' WHERE [OPTIONNAME]='{2}'", "SecretOptions_v2", data, key);
		return Helper.Update(update);
	}

	private string LoadData(string key)
	{
		string query = string.Format("SELECT OPTIONVALUE FROM [{0}] WHERE OPTIONNAME = '{1}'", "SecretOptions_v2", key);
		string data = null;
		Helper.ExectuteSelectQuery(delegate(IDataReader reader)
		{
			data = reader.ReadString(0);
		}, query);
		return data;
	}
}
