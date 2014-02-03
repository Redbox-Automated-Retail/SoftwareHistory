using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using VistaDB.Provider;

namespace Redbox.KioskEngine.Environment;

internal sealed class ShoppingSession : IShoppingSession
{
	private readonly List<IShoppingSessionError> m_errors = new List<IShoppingSessionError>();

	private readonly List<IShoppingSessionEvent> m_events = new List<IShoppingSessionEvent>();

	public Guid Id { get; internal set; }

	public DateTime? EndedOn { get; internal set; }

	public string StoreNumber { get; internal set; }

	public int ViewsShown { get; set; }

	public DateTime StartedOn { get; internal set; }

	public IShoppingCart ShoppingCart { get; internal set; }

	public ShoppingSessionStatus Status { get; internal set; }

	internal List<IShoppingSessionEvent> Events => m_events;

	internal List<IShoppingSessionError> Errors => m_errors;

	public ShoppingSession(Guid id, bool logStartEvent)
	{
		Id = id;
		if (logStartEvent)
		{
			AddEvent(ShoppingSessionEventType.Start, "Session has started.");
		}
	}

	public void AddEvent(ShoppingSessionEventType type, string description)
	{
		if (!EndedOn.HasValue)
		{
			LogHelper.Instance.Log("Added event to Shopping Session '{0}': type={1}, description={2}", Id, type, description);
			m_events.Add(new ShoppingSessionEvent
			{
				Type = type,
				TimeStamp = DateTime.Now,
				Description = description
			});
		}
	}

	public void AddError(string code, string description, string details)
	{
		LogHelper.Instance.Log("Added error to Shopping Session '{0}': code={1}, description={2}, details={3}", Id, code, description, details);
		m_errors.Add(new ShoppingSessionError
		{
			Code = code,
			Details = details,
			TimeStamp = DateTime.Now,
			Description = description
		});
	}

	public void AddWarning(string code, string description, string details)
	{
		LogHelper.Instance.Log("Added warning to Shopping Session '{0}': code={1}, description={2}, details={3}", Id, code, description, details);
		m_errors.Add(new ShoppingSessionError
		{
			Code = code,
			Details = details,
			TimeStamp = DateTime.Now,
			Description = description,
			IsWarning = true
		});
	}

	public void End()
	{
		LogHelper.Instance.Log("Ending Shopping Session.");
		if (EndedOn.HasValue)
		{
			LogHelper.Instance.Log("...Session already ended, returning.");
			return;
		}
		AddEvent(ShoppingSessionEventType.End, "Session has ended.");
		EndedOn = DateTime.Now;
		LogHelper.Instance.Log("...Session EndedOn set to: {0}", EndedOn);
	}

	public void Save()
	{
		try
		{
			SaveSession();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in ShoppingSession.Save.", e);
		}
	}

	public void Delete()
	{
		try
		{
			DeleteSession();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in ShoppingSession.Delete.", e);
		}
	}

	public bool IsSaved()
	{
		using (VistaDBCommand vistaDBCommand = new VistaDBCommand())
		{
			vistaDBCommand.Connection = ShoppingSessionService.Instance.Connection;
			vistaDBCommand.CommandText = "SELECT COUNT(*) FROM Session WHERE ID = @ID";
			vistaDBCommand.Parameters.Add("@ID", Id);
			int num = Convert.ToInt32(vistaDBCommand.ExecuteScalar());
			if (num > 0)
			{
				LogHelper.Instance.Log("Shopping Session '{0}' is saved.", Id);
				return true;
			}
		}
		LogHelper.Instance.Log("Shopping Session '{0}' is not saved.", Id);
		return false;
	}

	public void Commit(string reason)
	{
		LogHelper.Instance.Log("Commit Shopping Session '{0}'", Id);
		if (EndedOn.HasValue)
		{
			LogHelper.Instance.Log("...Shopping Session has already ended; skipping commit.");
			return;
		}
		AddEvent(ShoppingSessionEventType.Commit, (!string.IsNullOrEmpty(reason)) ? reason : "Session comitted.");
		Status = ShoppingSessionStatus.Committed;
		LogHelper.Instance.Log("...Commit complete.");
	}

	public void Abandon(string reason)
	{
		LogHelper.Instance.Log("Abandon Shopping Session '{0}'", Id);
		if (EndedOn.HasValue)
		{
			LogHelper.Instance.Log("...Shopping Session has already ended; skipping abandon.");
			return;
		}
		AddEvent(ShoppingSessionEventType.Abandon, (!string.IsNullOrEmpty(reason)) ? reason : "Session abandon.");
		Status = ShoppingSessionStatus.Abandon;
		LogHelper.Instance.Log("...Abandon complete.");
	}

	public void LoadShoppingCart()
	{
		LogHelper.Instance.Log("Load Shopping Cart for Shopping Session '{0}'", Id);
		if (ShoppingCart != null)
		{
			LogHelper.Instance.Log("...Shopping Cart already loaded.");
			return;
		}
		ShoppingCart = Redbox.KioskEngine.Environment.ShoppingCart.Load(Id);
		LogHelper.Instance.Log("...Load complete.");
	}

	public void ForEachEvent(Action<IShoppingSessionEvent> action)
	{
		try
		{
			if (!EndedOn.HasValue)
			{
				foreach (IShoppingSessionEvent @event in Events)
				{
					action(@event);
				}
				return;
			}
			using VistaDBCommand vistaDBCommand = new VistaDBCommand();
			vistaDBCommand.Connection = ShoppingSessionService.Instance.Connection;
			vistaDBCommand.CommandText = "SELECT * FROM SessionEvent WHERE SessionID = @ID";
			vistaDBCommand.Parameters.Add("@ID", Id);
			using VistaDBDataReader vistaDBDataReader = vistaDBCommand.ExecuteReader();
			while (vistaDBDataReader.Read())
			{
				action(new ShoppingSessionEvent
				{
					TimeStamp = (DateTime)vistaDBDataReader["CreatedOn"],
					Description = (string)vistaDBDataReader["Description"],
					Type = (ShoppingSessionEventType)Convert.ToInt32(vistaDBDataReader["Type"])
				});
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in ShoppingSession.ForEachEvent.", e);
		}
	}

	public void ForEachError(Action<IShoppingSessionError> action)
	{
		try
		{
			if (!EndedOn.HasValue)
			{
				foreach (IShoppingSessionError error in Errors)
				{
					action(error);
				}
				return;
			}
			using VistaDBCommand vistaDBCommand = new VistaDBCommand();
			vistaDBCommand.Connection = ShoppingSessionService.Instance.Connection;
			vistaDBCommand.CommandText = "SELECT * FROM SessionError WHERE SessionID = @ID";
			vistaDBCommand.Parameters.Add("@ID", Id);
			using VistaDBDataReader vistaDBDataReader = vistaDBCommand.ExecuteReader();
			while (vistaDBDataReader.Read())
			{
				action(new ShoppingSessionError
				{
					Code = (string)vistaDBDataReader["Code"],
					Details = (string)vistaDBDataReader["Details"],
					TimeStamp = (DateTime)vistaDBDataReader["CreatedOn"],
					Description = (string)vistaDBDataReader["Description"],
					IsWarning = Convert.ToBoolean((int)vistaDBDataReader["IsWarning"])
				});
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in ShoppingSession.ForEachError.", e);
		}
	}

	internal void SaveSession()
	{
		LogHelper.Instance.Log("Save Shopping Session '{0}'", Id);
		if (ShoppingSessionService.Instance.Connection == null || IsSaved())
		{
			LogHelper.Instance.Log("...Session already saved or unable to connect to the database.");
			return;
		}
		using (VistaDBCommand vistaDBCommand = new VistaDBCommand())
		{
			vistaDBCommand.Connection = ShoppingSessionService.Instance.Connection;
			vistaDBCommand.CommandText = "INSERT INTO Session(ID, StoreNumber, StartedOn, EndedOn, Status) VALUES(@ID, @StoreNumber, @StartedOn, @EndedOn, @Status)";
			vistaDBCommand.Parameters.Add("@ID", Id);
			vistaDBCommand.Parameters.Add("@StoreNumber", StoreNumber);
			vistaDBCommand.Parameters.Add("@StartedOn", StartedOn);
			vistaDBCommand.Parameters.Add("@EndedOn", EndedOn);
			vistaDBCommand.Parameters.Add("@Status", Status);
			vistaDBCommand.ExecuteNonQuery();
		}
		foreach (IShoppingSessionEvent @event in Events)
		{
			using VistaDBCommand vistaDBCommand2 = new VistaDBCommand();
			vistaDBCommand2.Connection = ShoppingSessionService.Instance.Connection;
			vistaDBCommand2.CommandText = "INSERT INTO SessionEvent(SessionID, Type, CreatedOn, Description) VALUES( @SessionID, @Type, @CreatedOn, @Description)";
			vistaDBCommand2.Parameters.Add("@SessionID", Id);
			vistaDBCommand2.Parameters.Add("@Type", @event.Type);
			vistaDBCommand2.Parameters.Add("@CreatedOn", @event.TimeStamp);
			vistaDBCommand2.Parameters.Add("@Description", @event.Description);
			vistaDBCommand2.ExecuteNonQuery();
		}
		foreach (IShoppingSessionError error in Errors)
		{
			using VistaDBCommand vistaDBCommand3 = new VistaDBCommand();
			vistaDBCommand3.Connection = ShoppingSessionService.Instance.Connection;
			vistaDBCommand3.CommandText = "INSERT INTO SessionError(SessionID, Code, IsWarning, Details, CreatedOn, Description) VALUES( @SessionID, @Code, @IsWarning, @Details, @CreatedOn, @Description)";
			vistaDBCommand3.Parameters.Add("@SessionID", Id);
			vistaDBCommand3.Parameters.Add("@Code", error.Code);
			vistaDBCommand3.Parameters.Add("@IsWarning", error.IsWarning);
			vistaDBCommand3.Parameters.Add("@Details", error.Details);
			vistaDBCommand3.Parameters.Add("@CreatedOn", error.TimeStamp);
			vistaDBCommand3.Parameters.Add("@Description", error.Description);
			vistaDBCommand3.ExecuteNonQuery();
		}
		if (ShoppingCart != null)
		{
			LogHelper.Instance.Log("...Save Shopping Cart.");
			ShoppingCart.Save(Id);
		}
		LogHelper.Instance.Log("...Save complete.");
	}

	internal void DeleteSession()
	{
		LogHelper.Instance.Log("Delete Shopping Session '{0}'", Id);
		if (ShoppingSessionService.Instance.Connection == null)
		{
			LogHelper.Instance.Log("...No connection to the session database.");
			return;
		}
		using (VistaDBCommand vistaDBCommand = new VistaDBCommand())
		{
			vistaDBCommand.Connection = ShoppingSessionService.Instance.Connection;
			vistaDBCommand.CommandText = "DELETE FROM Session WHERE ID = @ID";
			vistaDBCommand.Parameters.Add("@ID", Id);
			vistaDBCommand.ExecuteNonQuery();
		}
		LogHelper.Instance.Log("...Delete compelte.");
	}
}
