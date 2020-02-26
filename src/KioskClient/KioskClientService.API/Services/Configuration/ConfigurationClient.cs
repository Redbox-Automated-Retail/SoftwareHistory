using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.Configuration;

public class ConfigurationClient
{
	internal class InternalClientConfigFile
	{
		internal class TypeMap
		{
			public string TypeName { get; set; }

			public Type Type { get; set; }

			public static List<TypeMap> GetTypeMapList()
			{
				List<TypeMap> list = new List<TypeMap>();
				list.Add(new TypeMap
				{
					TypeName = "bool",
					Type = typeof(bool)
				});
				list.Add(new TypeMap
				{
					TypeName = "boolean",
					Type = typeof(bool)
				});
				list.Add(new TypeMap
				{
					TypeName = "string",
					Type = typeof(string)
				});
				list.Add(new TypeMap
				{
					TypeName = "integer",
					Type = typeof(int)
				});
				list.Add(new TypeMap
				{
					TypeName = "int",
					Type = typeof(int)
				});
				list.Add(new TypeMap
				{
					TypeName = "long",
					Type = typeof(long)
				});
				list.Add(new TypeMap
				{
					TypeName = "float",
					Type = typeof(float)
				});
				list.Add(new TypeMap
				{
					TypeName = "double",
					Type = typeof(double)
				});
				list.Add(new TypeMap
				{
					TypeName = "date",
					Type = typeof(DateTime)
				});
				list.Add(new TypeMap
				{
					TypeName = "datetime",
					Type = typeof(DateTime)
				});
				list.Add(new TypeMap
				{
					TypeName = "timespan",
					Type = typeof(TimeSpan)
				});
				list.Add(new TypeMap
				{
					TypeName = "blob",
					Type = typeof(byte[])
				});
				return list;
			}

			public static Type GetType(string typeName)
			{
				List<TypeMap> typeMapList = GetTypeMapList();
				TypeMap typeMap = typeMapList.Where((TypeMap tm) => tm.TypeName.Equals(typeName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
				if (typeMap == null)
				{
					return typeof(string);
				}
				return typeMap.Type;
			}
		}

		private ILogger _logger;

		private string _filePath;

		private DateTime _lastCheckup;

		private TimeSpan _checkTime;

		private Func<bool> _canActivate;

		private Action _activateAction;

		private string _defaultCategory;

		private int _inCheckup;

		private ClientConfigFile _clientConfigFile;

		private const string ConfigFileStaged = ".configstaged";

		private readonly object _lock = new object();

		internal string DefaultCategory => _defaultCategory;

		internal string FilePath => _filePath;

		internal TimeSpan CheckTime => _checkTime;

		internal bool HasCanActivate => _canActivate != null;

		internal bool HasActivateAction => _activateAction != null;

		internal InternalClientConfigFile(ILogger logger, string filepath, string defaultCategory, TimeSpan checkTime, Func<bool> canActivate, Action activateAction)
		{
			_logger = logger;
			_lastCheckup = DateTime.Now;
			_checkTime = checkTime;
			if (canActivate == null)
			{
				canActivate = () => true;
			}
			_canActivate = canActivate;
			_activateAction = activateAction;
			_filePath = filepath;
			_defaultCategory = defaultCategory;
			ActivateConfigFile();
		}

		internal bool TryGetValue<T>(string setting, out T value)
		{
			return TryGetValue<T>(_defaultCategory, setting, out value);
		}

		internal bool TryGetValue<T>(string category, string setting, out T value)
		{
			value = default(T);
			lock (_lock)
			{
				try
				{
					if (_clientConfigFile == null)
					{
						return false;
					}
					DateTime date = DateTime.Now;
					ClientConfigurationData clientConfigurationData = (from d in _clientConfigFile.ConfigurationData
						where d.CategoryName.Equals(category, StringComparison.CurrentCultureIgnoreCase) && d.SettingName.Equals(setting, StringComparison.CurrentCultureIgnoreCase) && d.EffectiveTime <= date && (!d.ExpireTime.HasValue || d.ExpireTime > date)
						orderby d.DefaultTypeRank descending, d.EffectiveTime descending
						select d).FirstOrDefault();
					if (clientConfigurationData == null)
					{
						return false;
					}
					Type type = TypeMap.GetType(clientConfigurationData.SettingDataType);
					if (type == typeof(byte[]))
					{
						byte[] value2 = new byte[0];
						if (clientConfigurationData.Value.Length > 0)
						{
							value2 = Convert.FromBase64String(clientConfigurationData.Value);
						}
						value = (T)Convert.ChangeType(value2, typeof(T));
						return true;
					}
					value = (T)Convert.ChangeType(clientConfigurationData.Value, typeof(T));
					return true;
				}
				catch (Exception exception)
				{
					_logger?.LogError(exception, "Unhandled exception occurred in InternalClientConfigFile.TryGetValue");
					return false;
				}
			}
		}

		internal bool TryGetObject(string setting, out object value)
		{
			return TryGetObject(_defaultCategory, setting, out value);
		}

		internal bool TryGetObject(string category, string setting, out object value)
		{
			value = null;
			lock (_lock)
			{
				try
				{
					if (_clientConfigFile == null)
					{
						return false;
					}
					DateTime date = DateTime.Now;
					ClientConfigurationData clientConfigurationData = (from d in _clientConfigFile.ConfigurationData
						where d.CategoryName.Equals(category, StringComparison.CurrentCultureIgnoreCase) && d.SettingName.Equals(setting, StringComparison.CurrentCultureIgnoreCase) && d.EffectiveTime <= date && (!d.ExpireTime.HasValue || d.ExpireTime > date)
						orderby d.DefaultTypeRank descending, d.EffectiveTime descending
						select d).FirstOrDefault();
					if (clientConfigurationData == null)
					{
						return false;
					}
					Type type = TypeMap.GetType(clientConfigurationData.SettingDataType);
					if (type == typeof(byte[]))
					{
						value = new byte[0];
						if (clientConfigurationData.Value.Length > 0)
						{
							value = Convert.FromBase64String(clientConfigurationData.Value);
						}
						return true;
					}
					value = Convert.ChangeType(clientConfigurationData.Value, TypeMap.GetType(clientConfigurationData.SettingDataType));
					return true;
				}
				catch (Exception exception)
				{
					_logger?.LogError(exception, "Unhandled exception occurred in InternalClientConfigFile.TryGetObject");
					return false;
				}
			}
		}

		internal bool TryGetConfigurationData(string setting, out ClientConfigurationData value)
		{
			return TryGetConfigurationData(_defaultCategory, setting, out value);
		}

		internal bool TryGetConfigurationData(string category, string setting, out ClientConfigurationData value)
		{
			value = null;
			lock (_lock)
			{
				try
				{
					if (_clientConfigFile == null)
					{
						return false;
					}
					DateTime date = DateTime.Now;
					ClientConfigurationData clientConfigurationData = (from d in _clientConfigFile.ConfigurationData
						where d.CategoryName.Equals(category, StringComparison.CurrentCultureIgnoreCase) && d.SettingName.Equals(setting, StringComparison.CurrentCultureIgnoreCase) && d.EffectiveTime <= date && (!d.ExpireTime.HasValue || d.ExpireTime > date)
						orderby d.DefaultTypeRank descending, d.EffectiveTime descending
						select d).FirstOrDefault();
					if (clientConfigurationData == null)
					{
						return false;
					}
					value = clientConfigurationData;
					return true;
				}
				catch (Exception exception)
				{
					_logger?.LogError(exception, "Unhandled exception occurred in InternalClientConfigFile.TryGetConfigData");
					return false;
				}
			}
		}

		internal bool GetAllConfigurationData(out List<ClientConfigurationData> list)
		{
			list = new List<ClientConfigurationData>();
			lock (_lock)
			{
				try
				{
					if (_clientConfigFile == null)
					{
						return false;
					}
					foreach (ClientConfigurationData configurationDatum in _clientConfigFile.ConfigurationData)
					{
						list.Add(configurationDatum);
					}
					return true;
				}
				catch (Exception exception)
				{
					_logger?.LogError(exception, "Unhandled exception occurred in InternalClientConfigFile.GetAllConfigurationData");
					return false;
				}
			}
		}

		internal bool GetAllConfigurationData(string category, string setting, out List<ClientConfigurationData> list)
		{
			bool allConfigurationData = GetAllConfigurationData(out list);
			if (allConfigurationData)
			{
				list = list.Where((ClientConfigurationData d) => d.CategoryName.Equals(category, StringComparison.CurrentCultureIgnoreCase) && d.SettingName.Equals(setting, StringComparison.CurrentCultureIgnoreCase)).ToList();
			}
			return allConfigurationData;
		}

		internal bool GetAllConfigurationData(string setting, out List<ClientConfigurationData> list)
		{
			return GetAllConfigurationData(_defaultCategory, setting, out list);
		}

		internal bool GetCategoryNames(out List<string> list)
		{
			list = new List<string>();
			lock (_lock)
			{
				try
				{
					if (_clientConfigFile == null)
					{
						return false;
					}
					List<string> list2 = _clientConfigFile.ConfigurationData.Select((ClientConfigurationData cd) => cd.CategoryName).Distinct().ToList();
					list = list2;
					return true;
				}
				catch (Exception exception)
				{
					_logger?.LogError(exception, "Unhandled exception occurred in InternalClientConfigFile.GetCategoryNames");
					return false;
				}
			}
		}

		internal bool GetSettingNames(out List<string> list)
		{
			return GetSettingNames(_defaultCategory, out list);
		}

		internal bool GetSettingNames(string category, out List<string> list)
		{
			list = new List<string>();
			lock (_lock)
			{
				try
				{
					if (_clientConfigFile == null)
					{
						return false;
					}
					List<string> list2 = (from cd in _clientConfigFile.ConfigurationData
						where cd.CategoryName.Equals(category, StringComparison.CurrentCultureIgnoreCase)
						select cd.SettingName).Distinct().ToList();
					list = list2;
					return true;
				}
				catch (Exception exception)
				{
					_logger?.LogError(exception, "Unhandled exception occurred in InternalClientConfigFile.GetSettingNames");
					return false;
				}
			}
		}

		internal void CheckForUpdates()
		{
			try
			{
				if (Interlocked.CompareExchange(ref _inCheckup, 1, 0) == 1)
				{
					return;
				}
				try
				{
					if (IsExpired())
					{
						if (NeedsActivation() && _canActivate())
						{
							ActivateConfigFile();
						}
						UpdateLastCheckup();
					}
				}
				finally
				{
					_inCheckup = 0;
				}
			}
			catch (Exception exception)
			{
				_logger?.LogError(exception, "There was an exception in ConfigurationClient.CheckForUpdates");
			}
		}

		internal void UpdateLastCheckup()
		{
			_lastCheckup = DateTime.Now;
		}

		private bool IsExpired()
		{
			return _lastCheckup.Add(_checkTime) < DateTime.Now;
		}

		private void InternalActivateFile()
		{
			if (_clientConfigFile.OID > 0)
			{
				Instance.ActivateConfigFile(_clientConfigFile.OID);
			}
		}

		private bool NeedsActivation()
		{
			string path = Path.ChangeExtension(_filePath, ".configstaged");
			return File.Exists(path);
		}

		private void ActivateConfigFile()
		{
			LoadConfigFile();
			if (NeedsActivation())
			{
				InternalActivateFile();
				if (_activateAction != null)
				{
					_activateAction();
				}
			}
		}

		private void LoadConfigFile()
		{
			lock (_lock)
			{
				try
				{
					_clientConfigFile = null;
					if (!File.Exists(_filePath))
					{
						_logger?.LogError("Error: Config file {0} does not exist.", _filePath);
						return;
					}
					string value = File.ReadAllText(_filePath);
					if (string.IsNullOrEmpty(value))
					{
						_logger?.LogError("Error: Config file {0} is empty.", _filePath);
					}
					else
					{
						_clientConfigFile = JsonConvert.DeserializeObject<ClientConfigFile>(value);
					}
				}
				catch (Exception exception)
				{
					_logger?.LogError(exception, "An unhandled exception occurred in InternalClientConfigFile.LoadConfigFile.");
					_clientConfigFile = null;
				}
			}
		}
	}

	private static ConfigurationClient _instance;

	private ILogger _logger;

	private string _updateManagerAddress = "127.0.0.1";

	private int _updateManagerPort = 7004;

	private readonly Dictionary<string, InternalClientConfigFile> _clientConfigFileList = new Dictionary<string, InternalClientConfigFile>();

	private readonly object _lock = new object();

	private Timer _changeTimer;

	private bool _isRunning;

	private string _url;

	private int _timeout;

	public static ConfigurationClient Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new ConfigurationClient();
			}
			return _instance;
		}
	}

	public void Initialize(ILogger logger, string updateManagerAddress, int updateManagerPort)
	{
		Initialize(logger, updateManagerAddress, updateManagerPort, 60000);
	}

	public void Initialize(ILogger logger, string updateManagerAddress, int updateManagerPort, int timeout)
	{
		_logger = logger;
		if (!string.IsNullOrWhiteSpace(updateManagerAddress))
		{
			_updateManagerAddress = updateManagerAddress;
		}
		if (updateManagerPort > 0)
		{
			_updateManagerPort = updateManagerPort;
		}
		_timeout = timeout;
		if (_changeTimer == null)
		{
			_changeTimer = new Timer(delegate
			{
				CheckAllForUpdates();
			});
			_isRunning = false;
		}
		else
		{
			Stop();
		}
	}

	public void Start()
	{
		_changeTimer.Change(new TimeSpan(0L), new TimeSpan(0, 0, 120));
		_isRunning = true;
	}

	public void Stop()
	{
		_changeTimer.Change(-1, -1);
		_isRunning = false;
	}

	public void Add(string key, string filepath, string defaultCategory)
	{
		Add(key, filepath, defaultCategory, null, null, null);
	}

	public void Add(string key, string filepath, string defaultCategory, TimeSpan? checkTime, Func<bool> canActivate, Action activateAction)
	{
		lock (_lock)
		{
			if (_clientConfigFileList.ContainsKey(key))
			{
				throw new Exception($"Exception occurred when trying to add a health item with a duplicate key {key}");
			}
			_clientConfigFileList.Add(key, new InternalClientConfigFile(_logger, filepath, defaultCategory, checkTime ?? new TimeSpan(0, 0, 120), canActivate, activateAction));
		}
	}

	public void Remove(string key)
	{
		lock (_lock)
		{
			if (_clientConfigFileList.TryGetValue(key, out var _))
			{
				_clientConfigFileList.Remove(key);
				return;
			}
			throw new Exception($"Error trying to remove a configfile item with a non-existent key {key}");
		}
	}

	public List<ConfigurationClientItem> GetItems()
	{
		List<ConfigurationClientItem> list = new List<ConfigurationClientItem>();
		lock (_lock)
		{
			foreach (KeyValuePair<string, InternalClientConfigFile> clientConfigFile in _clientConfigFileList)
			{
				list.Add(new ConfigurationClientItem
				{
					Key = clientConfigFile.Key,
					DefaultCategory = clientConfigFile.Value.DefaultCategory,
					FilePath = clientConfigFile.Value.FilePath,
					CheckTime = clientConfigFile.Value.CheckTime,
					HasCanActivate = clientConfigFile.Value.HasCanActivate,
					HasActivateAction = clientConfigFile.Value.HasActivateAction
				});
			}
			return list;
		}
	}

	public void RemoveAll()
	{
		lock (_lock)
		{
			_clientConfigFileList.Clear();
		}
	}

	public void CheckAllForUpdates()
	{
		if (!_isRunning)
		{
			return;
		}
		lock (_lock)
		{
			foreach (KeyValuePair<string, InternalClientConfigFile> clientConfigFile in _clientConfigFileList)
			{
				clientConfigFile.Value.CheckForUpdates();
			}
		}
	}

	public bool TryGetValue<T>(string key, string setting, out T value)
	{
		value = default(T);
		lock (_lock)
		{
			if (!_clientConfigFileList.TryGetValue(key, out var value2))
			{
				return false;
			}
			return value2.TryGetValue<T>(setting, out value);
		}
	}

	public bool TryGetValue<T>(string key, string category, string setting, out T value)
	{
		value = default(T);
		lock (_lock)
		{
			if (!_clientConfigFileList.TryGetValue(key, out var value2))
			{
				return false;
			}
			return value2.TryGetValue<T>(category, setting, out value);
		}
	}

	public bool TryGetObject(string key, string setting, out object value)
	{
		value = null;
		lock (_lock)
		{
			if (!_clientConfigFileList.TryGetValue(key, out var value2))
			{
				return false;
			}
			return value2.TryGetObject(setting, out value);
		}
	}

	public bool TryGetObject(string key, string category, string setting, out object value)
	{
		value = null;
		lock (_lock)
		{
			if (!_clientConfigFileList.TryGetValue(key, out var value2))
			{
				return false;
			}
			return value2.TryGetObject(category, setting, out value);
		}
	}

	public bool TryGetConfigurationData(string key, string setting, out ClientConfigurationData value)
	{
		value = null;
		lock (_lock)
		{
			if (!_clientConfigFileList.TryGetValue(key, out var value2))
			{
				return false;
			}
			return value2.TryGetConfigurationData(setting, out value);
		}
	}

	public bool TryGetConfigurationData(string key, string category, string setting, out ClientConfigurationData value)
	{
		value = null;
		lock (_lock)
		{
			if (!_clientConfigFileList.TryGetValue(key, out var value2))
			{
				return false;
			}
			return value2.TryGetConfigurationData(category, setting, out value);
		}
	}

	public bool GetAllConfigurationData(string key, out List<ClientConfigurationData> list)
	{
		list = new List<ClientConfigurationData>();
		lock (_lock)
		{
			if (!_clientConfigFileList.TryGetValue(key, out var value))
			{
				return false;
			}
			return value.GetAllConfigurationData(out list);
		}
	}

	public bool GetAllConfigurationData(string key, string category, string setting, out List<ClientConfigurationData> list)
	{
		list = new List<ClientConfigurationData>();
		lock (_lock)
		{
			if (!_clientConfigFileList.TryGetValue(key, out var value))
			{
				return false;
			}
			return value.GetAllConfigurationData(category, setting, out list);
		}
	}

	public bool GetAllConfigurationData(string key, string setting, out List<ClientConfigurationData> list)
	{
		list = new List<ClientConfigurationData>();
		lock (_lock)
		{
			if (!_clientConfigFileList.TryGetValue(key, out var value))
			{
				return false;
			}
			return value.GetAllConfigurationData(setting, out list);
		}
	}

	public bool GetCategoryNames(string key, out List<string> list)
	{
		list = new List<string>();
		lock (_lock)
		{
			if (!_clientConfigFileList.TryGetValue(key, out var value))
			{
				return false;
			}
			return value.GetCategoryNames(out list);
		}
	}

	public bool GetSettingNames(string key, string category, out List<string> list)
	{
		list = new List<string>();
		lock (_lock)
		{
			if (!_clientConfigFileList.TryGetValue(key, out var value))
			{
				return false;
			}
			return value.GetSettingNames(category, out list);
		}
	}

	public bool GetSettingNames(string key, out List<string> list)
	{
		list = new List<string>();
		lock (_lock)
		{
			if (!_clientConfigFileList.TryGetValue(key, out var value))
			{
				return false;
			}
			return value.GetSettingNames(out list);
		}
	}

	internal void ActivateConfigFile(long configFileId)
	{
		try
		{
			_logger.LogInformation($"ConfigurationClient - Sending config file {configFileId} activation to update manager");
			using TcpClient tcpClient = new TcpClient(_updateManagerAddress, _updateManagerPort);
			NetworkStream stream = tcpClient.GetStream();
			byte[] bytes = Encoding.ASCII.GetBytes($"configfile activate configfileid: {configFileId}{Environment.NewLine}");
			stream.Write(bytes, 0, bytes.Length);
			byte[] buffer = new byte[1024];
			int num = stream.Read(buffer, 0, 1024);
		}
		catch (Exception exception)
		{
			_logger?.LogError(exception, $"ConfigurationClient - Unhandled exception occurred activating config file {configFileId}");
		}
	}

	private ConfigurationClient()
	{
	}
}
