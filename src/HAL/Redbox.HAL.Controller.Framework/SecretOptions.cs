using System;
using System.IO;
using Redbox.HAL.Core;
using Redbox.HAL.DataStorage;

namespace Redbox.HAL.Controller.Framework;

public class SecretOptions
{
	private const string SecretOptionsTableName = "SecretOptions_v2";

	public static SecretOptions Instance => Singleton<SecretOptions>.Instance;

	private VistaDBHelper Helper => CountersDBManager.Instance.Store;

	public bool Upgrade(TextWriter writer)
	{
		string text = "SecretOptions";
		if (Helper.TableExists(text))
		{
			bool flag = Helper.DropTable(text);
			writer.WriteLine("Deletion of table SecretOptions returned {0}", flag);
		}
		if (Helper.TableExists("SecretOptions_v2"))
		{
			writer.WriteLine("The table {0} exists - take no further action.", "SecretOptions_v2");
			return true;
		}
		string createQuery = string.Format("CREATE TABLE [{0}] ( [OptionName] VarChar(32) NOT NULL PRIMARY KEY, [OptionValue] VarChar(64) NOT NULL, [OptionEncrypted] bit NOT NULL )", "SecretOptions_v2");
		bool flag2 = Helper.CreateTable(createQuery);
		writer.WriteLine("Creation of table {0} returned {1}", "SecretOptions_v2", flag2);
		return flag2;
	}

	public bool RemoveOption(string key)
	{
		string deleteQuery = string.Format("DELETE FROM [{0}] WHERE OPTIONNAME='{1}'", "SecretOptions_v2", key);
		return Helper.Delete(deleteQuery) == 1;
	}

	public T GetValue<T>(string name)
	{
		return GetValueInner(name, default(T), saveDefault: false, encrypt: false);
	}

	public T GetValue<T>(string name, T defValue)
	{
		return GetValueInner(name, defValue, saveDefault: true, encrypt: false);
	}

	public void SetValue<T>(string name, T value)
	{
		try
		{
			SetValueInner(name, value, encrypt: false);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("WARNING: SecretOption.SetValueInner caught an exception.", e);
		}
	}

	private T GetValueInner<T>(string name, T defVal, bool saveDefault, bool encrypt)
	{
		string text = LoadData(name);
		if (text == null)
		{
			InsertOption(name, defVal.ToString());
			return defVal;
		}
		return (T)ConversionHelper.ChangeType(text, typeof(T));
	}

	private void SetValueInner<T>(string key, T value, bool encrypt)
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

	private bool InsertOption(string key, string data)
	{
		string insert = string.Format("INSERT INTO [{0}] (OPTIONNAME,OPTIONVALUE,OPTIONENCRYPTED) VALUES('{1}','{2}',{3})", "SecretOptions_v2", key, data, false);
		return Helper.Insert(insert);
	}

	private string CreateTableString()
	{
		return string.Format("CREATE TABLE [{0}] ( [OptionName] VarChar(32) NOT NULL PRIMARY KEY, [OptionValue] VarChar(64) NOT NULL, [OptionEncrypted] bit NOT NULL )", "SecretOptions_v2");
	}

	private bool UpdateOption(string key, string data)
	{
		string update = string.Format("UPDATE [{0}] SET [OPTIONVALUE]= '{1}' WHERE [OPTIONNAME]='{2}'", "SecretOptions_v2", data, key);
		return Helper.Update(update);
	}

	private string LoadData(string key)
	{
		string query = string.Format("SELECT OPTIONVALUE, OPTIONENCRYPTED FROM [{0}] WHERE OPTIONNAME = '{1}'", "SecretOptions_v2", key);
		string data = null;
		Helper.ExectuteSelectQuery(delegate(IDataReader reader)
		{
			data = reader.ReadString(0);
		}, query);
		return data;
	}

	private SecretOptions()
	{
	}
}
