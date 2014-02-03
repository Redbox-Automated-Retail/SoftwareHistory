using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.IDE;
using VistaDB.DDA;
using VistaDB.Provider;

namespace Redbox.KioskEngine.Environment;

public class QueueService : IQueueService, IDisposable
{
	internal sealed class Message : IMessage
	{
		public int ID { get; set; }

		public string GUID { get; set; }

		public byte[] Data { get; set; }

		public string Type { get; set; }

		public byte Priority { get; set; }

		public DateTime CreatedOn { get; set; }
	}

	internal const int DefaultQueueReadPeriodMin = 8000;

	internal const int DefaultQueueReadPeriodMax = 30000;

	private Timer m_queueTimer;

	private volatile bool m_isProcessingMessage;

	public static QueueService Instance => Singleton<QueueService>.Instance;

	public int QueueReadPeriodMin
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			return service.GetValue("Remote Services\\Queue Service", "ReadPeriodMin", 8000);
		}
		set
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			service.SetValue("Remote Services\\Queue Service", "ReadPeriodMin", value);
		}
	}

	public int QueueReadPeriodMax
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			return service.GetValue("Remote Services\\Queue Service", "ReadPeriodMax", 30000);
		}
		set
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			service.SetValue("Remote Services\\Queue Service", "ReadPeriodMax", value);
		}
	}

	internal VistaDBConnection Connection { get; set; }

	internal string DatabasePath { get; set; }

	public event ProcessMessageDelegate ProcessMessage;

	public void StopQueueWorker()
	{
		if (m_queueTimer != null)
		{
			m_queueTimer.Dispose();
			m_queueTimer = null;
			LogHelper.Instance.Log("Queue worker stopped.");
		}
	}

	public bool IsWorkerStarted()
	{
		return m_queueTimer != null;
	}

	public void StartQueueWorker()
	{
		if (m_queueTimer != null)
		{
			return;
		}
		m_queueTimer = new Timer(delegate
		{
			if (m_isProcessingMessage)
			{
				return;
			}
			try
			{
				m_isProcessingMessage = true;
				ErrorList errorList = Dequeue(delegate(IMessage message)
				{
					try
					{
						return ProcessMessage != null && ProcessMessage(message);
					}
					catch
					{
						return false;
					}
				});
				if (errorList.ContainsError())
				{
					LogHelper.Instance.Log("Unable to dequeue message in Queue Service.StartQueueWorker; errors follow:");
					{
						foreach (Redbox.KioskEngine.ComponentModel.Error item in errorList)
						{
							LogHelper.Instance.Log("...{0}", item);
						}
						return;
					}
				}
			}
			finally
			{
				try
				{
					int randomNumber = GetRandomNumber(QueueReadPeriodMin, QueueReadPeriodMax);
					LogHelper.Instance.Log("Queue Service: The next dequeue time will be in {0}ms.", randomNumber);
					m_queueTimer.Change(randomNumber, randomNumber);
				}
				catch (Exception e)
				{
					LogHelper.Instance.Log("An unhandled exception was raised during m_queueTimer.Change in QueueService.StartQueueWorker.", e);
				}
				finally
				{
					m_isProcessingMessage = false;
				}
			}
		}, null, QueueReadPeriodMin, QueueReadPeriodMax);
		LogHelper.Instance.Log("Queue worker started at read period of: {0}", QueueReadPeriodMin);
	}

	public void Dispose()
	{
		if (Connection != null)
		{
			Connection.Dispose();
		}
	}

	public ErrorList Clear()
	{
		ErrorList errorList = new ErrorList();
		if (Connection == null)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("Q998", "No database connection available.", "The initialization of the service may have failed; please reinitialize the Queue Service."));
			return errorList;
		}
		try
		{
			using VistaDBCommand vistaDBCommand = new VistaDBCommand();
			vistaDBCommand.Connection = Connection;
			vistaDBCommand.CommandText = "DELETE FROM Queue";
			vistaDBCommand.ExecuteNonQuery();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in QueueService.Clear", e);
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("Q999", "An unhandled exception was raised in QueueService.Clear.", e));
		}
		return errorList;
	}

	public ErrorList Peek(out object instance)
	{
		ErrorList errorList = new ErrorList();
		instance = null;
		if (Connection == null)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("Q998", "No database connection available.", "The initialization of the service may have failed; please reinitialize the Queue Service."));
			return errorList;
		}
		try
		{
			using VistaDBCommand vistaDBCommand = new VistaDBCommand();
			vistaDBCommand.Connection = Connection;
			vistaDBCommand.CommandText = "SELECT TOP 1 * FROM Queue ORDER BY Priority DESC, ID ASC";
			using VistaDBDataReader vistaDBDataReader = vistaDBCommand.ExecuteReader();
			if (vistaDBDataReader.Read())
			{
				instance = Encoding.ASCII.GetString((byte[])vistaDBDataReader["Data"]);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in QueueService.Peek", e);
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("Q999", "An unhandled exception was raised in QueueService.Peek.", e));
		}
		return errorList;
	}

	public ErrorList GetDepth(out int count)
	{
		ErrorList errorList = new ErrorList();
		count = 0;
		if (Connection == null)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("Q998", "No database connection available.", "The initialization of the service may have failed; please reinitialize the Queue Service."));
			return errorList;
		}
		try
		{
			using VistaDBCommand vistaDBCommand = new VistaDBCommand();
			vistaDBCommand.Connection = Connection;
			vistaDBCommand.CommandText = "SELECT COUNT(*) FROM Queue";
			count = (int)vistaDBCommand.ExecuteScalar();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in QueueService.GetDepth.", e);
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("Q999", "An unhandled exception was raised in QueueService.GetDepth.", e));
		}
		return errorList;
	}

	public ErrorList Enqueue(byte priority, string type, object message)
	{
		ErrorList errorList = new ErrorList();
		if (Connection == null)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("Q998", "No database connection available.", "The initialization of the service may have failed; please reinitialize the Queue Service."));
			return errorList;
		}
		try
		{
			using VistaDBCommand vistaDBCommand = new VistaDBCommand();
			vistaDBCommand.Connection = Connection;
			vistaDBCommand.CommandText = "INSERT INTO Queue( Priority, Type, Data ) VALUES( @Priority, @Type, @Data )";
			vistaDBCommand.Parameters.Add("@Type", type);
			vistaDBCommand.Parameters.Add("@Priority", priority);
			vistaDBCommand.Parameters.Add("@Data", Encoding.ASCII.GetBytes(ObjectExtensions.ToJson(message)));
			vistaDBCommand.ExecuteNonQuery();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in QueueService.Enqueue.", e);
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("Q999", "An unhandled exception was raised in QueueService.Enqueue.", e));
		}
		return errorList;
	}

	public ErrorList EnqueueOnlyOne(byte priority, string type, object message)
	{
		ErrorList errorList = new ErrorList();
		if (Connection == null)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("Q998", "No database connection available.", "The initialization of the service may have failed; please reinitialize the Queue Service."));
			return errorList;
		}
		try
		{
			DeleteMessageByType(type);
			int num = 0;
			using VistaDBCommand vistaDBCommand = new VistaDBCommand();
			vistaDBCommand.Connection = Connection;
			vistaDBCommand.CommandText = "INSERT INTO Queue( Priority, Type, Data ) VALUES( @Priority, @Type, @Data ); SELECT @@IDENTITY;";
			vistaDBCommand.Parameters.Add("@Type", type);
			vistaDBCommand.Parameters.Add("@Priority", priority);
			vistaDBCommand.Parameters.Add("@Data", Encoding.ASCII.GetBytes(ObjectExtensions.ToJson(message)));
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("QID", Convert.ToInt32(vistaDBCommand.ExecuteScalar()).ToString(), "used to get unique value of queued softsync"));
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in QueueService.EnqueueOnlyOne.", e);
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("Q999", "An unhandled exception was raised in QueueService.EnqueueOnlyOne.", e));
		}
		return errorList;
	}

	public ErrorList Update(int id, object message)
	{
		return Update(id, ObjectExtensions.ToJson(message));
	}

	public ErrorList Update(int id, string message)
	{
		ErrorList errorList = new ErrorList();
		if (Connection == null || string.IsNullOrEmpty(message))
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("Q998", "No database connection available.", "The initialization of the service may have failed; please reinitialize the Queue Service."));
			return errorList;
		}
		try
		{
			using VistaDBCommand vistaDBCommand = new VistaDBCommand();
			vistaDBCommand.Connection = Connection;
			vistaDBCommand.CommandText = "UPDATE Queue SET Data = @Data WHERE ID = @Id";
			vistaDBCommand.Parameters.Add("@Id", id);
			vistaDBCommand.Parameters.Add("@Data", Encoding.ASCII.GetBytes(message));
			vistaDBCommand.ExecuteNonQuery();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in QueueService.Update.", e);
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("Q999", "An unhandled exception was raised in QueueService.EnqueueOnlyOne.", e));
		}
		return errorList;
	}

	public ErrorList ForEach(Action<IMessage> action)
	{
		ErrorList errorList = new ErrorList();
		if (Connection == null)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("Q998", "No database connection available.", "The initialization of the service may have failed; please reinitialize the Queue Service."));
			return errorList;
		}
		try
		{
			using VistaDBCommand vistaDBCommand = new VistaDBCommand();
			vistaDBCommand.Connection = Connection;
			vistaDBCommand.CommandText = "SELECT * FROM Queue ORDER BY Priority DESC, ID ASC";
			using VistaDBDataReader vistaDBDataReader = vistaDBCommand.ExecuteReader();
			while (vistaDBDataReader.Read())
			{
				action(new Message
				{
					ID = (int)vistaDBDataReader["ID"],
					Type = (string)vistaDBDataReader["Type"],
					CreatedOn = (DateTime)vistaDBDataReader["CreatedOn"],
					Data = (byte[])vistaDBDataReader["Data"],
					Priority = (byte)vistaDBDataReader["Priority"]
				});
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was rasied in QueueService.ForEach.", e);
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("Q999", "An unhandled exception was raised in QueueService.ForEach.", e));
		}
		return errorList;
	}

	public ErrorList ForEach(Action<IMessage> action, string type)
	{
		ErrorList errorList = new ErrorList();
		if (Connection == null)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("Q998", "No database connection available.", "The initialization of the service may have failed; please reinitialize the Queue Service."));
			return errorList;
		}
		try
		{
			using VistaDBCommand vistaDBCommand = new VistaDBCommand();
			vistaDBCommand.Connection = Connection;
			vistaDBCommand.CommandText = "SELECT * FROM Queue WHERE Type = @Type ORDER BY Priority DESC, ID ASC";
			vistaDBCommand.Parameters.Add("@Type", type);
			using VistaDBDataReader vistaDBDataReader = vistaDBCommand.ExecuteReader();
			while (vistaDBDataReader.Read())
			{
				action(new Message
				{
					ID = (int)vistaDBDataReader["ID"],
					Type = (string)vistaDBDataReader["Type"],
					CreatedOn = (DateTime)vistaDBDataReader["CreatedOn"],
					Data = (byte[])vistaDBDataReader["Data"],
					Priority = (byte)vistaDBDataReader["Priority"]
				});
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in QueueService.ForEach with type.", e);
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("Q999", "An unhandled exception was raised in QueueService.ForEach with type.", e));
		}
		return errorList;
	}

	public ErrorList Initialize(string path)
	{
		LogHelper.Instance.Log("Initialize queue service, storage path: {0}", path);
		ServiceLocator.Instance.AddService(typeof(IQueueService), Instance);
		ErrorList errorList = new ErrorList();
		try
		{
			DatabasePath = path;
			if (File.Exists(DatabasePath))
			{
				LogHelper.Instance.Log("> Pack message database...");
				using IVistaDBDDA vistaDBDDA = VistaDBEngine.Connections.OpenDDA();
				vistaDBDDA.PackDatabase(DatabasePath, null, backup: false, null);
			}
			SchemaMigrator.Migrate(DatabasePath);
			Connection = new VistaDBConnection($"Data Source={DatabasePath};Open Mode=ExclusiveReadWrite;");
			Connection.Open();
			ServiceLocator.Instance.GetService<IPreferenceService>()?.AddPreferencePage("QueueServices_General", "Remote Services\\Queue Services", "Engine Core/Queue Services", PreferencePageTarget.LocalSystem, new QueuePreferencePage());
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in QueueServie.Initialize.", e);
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("Q999", "Unable to initialize the Queue Service database.", e));
			try
			{
				string text = DatabasePath + ".corrupt";
				LogHelper.Instance.Log("Attempt to move '{0}' to '{1}'.", DatabasePath, text);
				File.Move(DatabasePath, text);
			}
			catch (Exception e2)
			{
				LogHelper.Instance.Log("An unhandled exception was raised renaming a corrupt Queue Service data file.", e2);
			}
		}
		return errorList;
	}

	public ErrorList Dequeue(Predicate<IMessage> predicate)
	{
		ErrorList errorList = new ErrorList();
		if (Connection == null)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("Q998", "No database connection available.", "The initialization of the service may have failed; please reinitialize the Queue Service."));
			return errorList;
		}
		try
		{
			using VistaDBCommand vistaDBCommand = new VistaDBCommand();
			vistaDBCommand.Connection = Connection;
			if (DateTime.UtcNow.Hour > 22 || DateTime.UtcNow.Hour < 3)
			{
				vistaDBCommand.CommandText = "SELECT TOP 1 ID, Type, Priority, Data, CreatedOn FROM Queue ORDER BY Priority DESC, ID ASC;";
			}
			else
			{
				vistaDBCommand.CommandText = "SELECT TOP 1 ID, Type, Priority, Data, CreatedOn FROM Queue WHERE Priority > 0 ORDER BY Priority DESC, ID ASC;";
			}
			using VistaDBDataReader vistaDBDataReader = vistaDBCommand.ExecuteReader();
			if (!vistaDBDataReader.Read())
			{
				return errorList;
			}
			if (!predicate(new Message
			{
				ID = (int)vistaDBDataReader["ID"],
				Type = (string)vistaDBDataReader["Type"],
				CreatedOn = (DateTime)vistaDBDataReader["CreatedOn"],
				Data = (byte[])vistaDBDataReader["Data"],
				Priority = (byte)vistaDBDataReader["Priority"]
			}))
			{
				return errorList;
			}
			using VistaDBCommand vistaDBCommand2 = new VistaDBCommand();
			vistaDBCommand2.Connection = Connection;
			vistaDBCommand2.CommandText = "DELETE FROM Queue WHERE ID = @ID";
			vistaDBCommand2.Parameters.Add("@ID", vistaDBDataReader["ID"]);
			vistaDBCommand2.ExecuteNonQuery();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was rasied in QueueService.Dequeue.", e);
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("Q999", "An unhandled exception was raised in QueueService.Dequeue.", e));
		}
		return errorList;
	}

	public ErrorList DeleteMessage(int id)
	{
		ErrorList errorList = new ErrorList();
		if (Connection == null)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("Q998", "No database connection available.", "The initialization of the service may have failed; please reinitialize the Queue Service."));
			return errorList;
		}
		try
		{
			using VistaDBCommand vistaDBCommand = new VistaDBCommand();
			vistaDBCommand.Connection = Connection;
			vistaDBCommand.CommandText = "DELETE FROM Queue WHERE ID = @ID";
			vistaDBCommand.Parameters.Add("@ID", id);
			vistaDBCommand.ExecuteNonQuery();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in QueueService.DeleteMessage.", e);
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("Q999", "An unhandled exception was raised in QueueService.DeleteMessage.", e));
		}
		return errorList;
	}

	public ErrorList DeleteMessageByType(string type)
	{
		ErrorList errorList = new ErrorList();
		if (Connection == null)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("Q998", "No database connection available.", "The initialization of the service may have failed; please reinitialize the Queue Service."));
			return errorList;
		}
		try
		{
			using VistaDBCommand vistaDBCommand = new VistaDBCommand();
			vistaDBCommand.Connection = Connection;
			vistaDBCommand.CommandText = "DELETE FROM Queue WHERE Type = @Type";
			vistaDBCommand.Parameters.Add("@Type", type);
			vistaDBCommand.ExecuteNonQuery();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in QueueService.DeleteMessageByType.", e);
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("Q999", "An unhandled exception was raised in QueueService.DeleteMessageByType.", e));
		}
		return errorList;
	}

	public ErrorList ExportToXml(string fileName)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			XmlTextWriter xmlWriter = new XmlTextWriter(fileName, Encoding.UTF8);
			xmlWriter.WriteStartDocument();
			xmlWriter.WriteStartElement("queue-export");
			ForEach(delegate(IMessage m)
			{
				xmlWriter.WriteStartElement("message");
				xmlWriter.WriteAttributeString("id", m.ID.ToString());
				xmlWriter.WriteAttributeString("type", m.Type);
				xmlWriter.WriteAttributeString("priority", m.Priority.ToString());
				xmlWriter.WriteAttributeString("createdon", m.CreatedOn.ToString());
				xmlWriter.WriteCData(Encoding.ASCII.GetString(m.Data));
				xmlWriter.WriteEndElement();
			});
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndDocument();
			xmlWriter.Close();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in QueueService.ExportToXml.", e);
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("Q999", "An unhandled exception was raised in QueueService.ExportToXml.", e));
		}
		return errorList;
	}

	private QueueService()
	{
	}

	private static int GetRandomNumber(int min, int max)
	{
		RNGCryptoServiceProvider rNGCryptoServiceProvider = new RNGCryptoServiceProvider();
		byte[] array = new byte[4];
		rNGCryptoServiceProvider.GetBytes(array);
		int num = BitConverter.ToInt32(array, 0);
		if (min != max)
		{
			return min + Math.Abs(num % (max - min));
		}
		return min;
	}
}
