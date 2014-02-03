using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.IDE;
using VistaDB.DDA;
using VistaDB.Provider;

namespace Redbox.KioskEngine.Environment;

public class ShoppingSessionService : IShoppingSessionService, IDisposable
{
	private const string LegacyActiveSessionId = "__active__";

	private string m_currentSessionId;

	private readonly object m_syncObject = new object();

	private readonly IDictionary<string, IShoppingSession> m_sessions = new Dictionary<string, IShoppingSession>();

	public static ShoppingSessionService Instance => Singleton<ShoppingSessionService>.Instance;

	internal VistaDBConnection Connection { get; set; }

	internal string DatabasePath { get; set; }

	public void Reset()
	{
		LogHelper.Instance.Log("Reset Shopping Session Service.");
		AbandonAll(null);
	}

	public void Clear()
	{
		LogHelper.Instance.Log("Clearing the Shopping Session cache.");
		lock (m_syncObject)
		{
			m_sessions.Clear();
			if (Connection == null)
			{
				return;
			}
			using VistaDBCommand vistaDBCommand = new VistaDBCommand();
			vistaDBCommand.Connection = Connection;
			vistaDBCommand.CommandText = "DELETE FROM Session";
			vistaDBCommand.ExecuteNonQuery();
		}
	}

	public void Dispose()
	{
		lock (m_syncObject)
		{
			Reset();
			if (Connection != null)
			{
				Connection.Dispose();
			}
		}
	}

	public void AbandonAll(string reason)
	{
		LogHelper.Instance.Log("Adbandon all shopping sessions.");
		lock (m_syncObject)
		{
			foreach (IShoppingSession value in m_sessions.Values)
			{
				LogHelper.Instance.Log("...Marking session '{0}' abandoned and saving.", value.Id);
				value.Abandon(reason);
				value.End();
				value.Save();
			}
			m_sessions.Clear();
		}
	}

	public IShoppingSession GetSession(string id)
	{
		lock (m_syncObject)
		{
			if (string.IsNullOrEmpty(id))
			{
				LogHelper.Instance.Log("The ID passed to GetSession is null or empty, returning null.");
				return null;
			}
			if (m_sessions.ContainsKey(id))
			{
				LogHelper.Instance.Log("Shopping Session '{0}' is cached in memory.", id);
				return m_sessions[id];
			}
			LogHelper.Instance.Log("Shopping Session '{0}' produced a cache miss, reloading from database.", id);
			IShoppingSession shoppingSession = Load(id);
			if (shoppingSession != null)
			{
				LogHelper.Instance.Log("...Session found, placing in cache.");
				m_sessions[id] = shoppingSession;
				return shoppingSession;
			}
			LogHelper.Instance.Log("Shopping Session '{0}' was not found.", id);
			return null;
		}
	}

	public IShoppingSession GetCurrentSession()
	{
		lock (m_syncObject)
		{
			if (m_sessions.ContainsKey("__active__"))
			{
				LogHelper.Instance.Log("The Shopping Session cache contains the legacy session ID, retrieiving it.");
				return m_sessions["__active__"];
			}
			return GetSession(m_currentSessionId);
		}
	}

	public string StartNewSession(string storeNumber, string sessionId)
	{
		lock (m_syncObject)
		{
			if (m_sessions.ContainsKey("__active__"))
			{
				LogHelper.Instance.Log("The Shopping Session cache contains the Legacy ID.");
				IShoppingSession shoppingSession = m_sessions["__active__"];
				if (!shoppingSession.IsSaved())
				{
					return null;
				}
			}
			foreach (string key in m_sessions.Keys)
			{
				if (!m_sessions[key].IsSaved())
				{
					LogHelper.Instance.Log("Shopping Session '{0}' marked as not saved; saving.", m_sessions[key].Id);
					m_sessions[key].Save();
				}
			}
			m_sessions.Clear();
			ShoppingSession shoppingSession2 = new ShoppingSession((!string.IsNullOrEmpty(sessionId)) ? new Guid(sessionId) : Guid.NewGuid(), logStartEvent: true);
			shoppingSession2.StartedOn = DateTime.Now;
			shoppingSession2.StoreNumber = storeNumber;
			ShoppingSession shoppingSession3 = shoppingSession2;
			shoppingSession3.ShoppingCart = new ShoppingCart(shoppingSession3.Id)
			{
				Type = ShoppingCartType.Online
			};
			string text = shoppingSession3.Id.ToString();
			m_sessions[text] = shoppingSession3;
			LogHelper.Instance.Log("Shopping Session '{0}' added to cache and started.", text);
			return text;
		}
	}

	public void ExporToXml(string fileName)
	{
		try
		{
			XmlTextWriter xmlWriter = new XmlTextWriter(fileName, Encoding.UTF8);
			xmlWriter.WriteStartDocument();
			xmlWriter.WriteStartElement("session-export");
			xmlWriter.WriteAttributeString("machineName", System.Environment.MachineName);
			ForEach(delegate(IShoppingSession session)
			{
				xmlWriter.WriteStartElement("session");
				xmlWriter.WriteAttributeString("id", session.Id.ToString());
				xmlWriter.WriteAttributeString("status", session.Status.ToString());
				xmlWriter.WriteAttributeString("createdon", session.StartedOn.ToString());
				session.LoadShoppingCart();
				if (session.ShoppingCart != null)
				{
					xmlWriter.WriteStartElement("shoppingCart");
					xmlWriter.WriteAttributeString("type", session.ShoppingCart.Type.ToString());
					xmlWriter.WriteAttributeString("email", session.ShoppingCart.Email);
					xmlWriter.WriteAttributeString("lastName", session.ShoppingCart.LastName);
					xmlWriter.WriteAttributeString("firstName", session.ShoppingCart.FirstName);
					xmlWriter.WriteAttributeString("lastFour", session.ShoppingCart.LastFour);
					xmlWriter.WriteAttributeString("expiryYear", session.ShoppingCart.ExpiryYear);
					xmlWriter.WriteAttributeString("expiryMonth", session.ShoppingCart.ExpiryMonth);
					xmlWriter.WriteAttributeString("postalCode", session.ShoppingCart.PostalCode);
					xmlWriter.WriteAttributeString("referenceNumber", session.ShoppingCart.ReferenceNumber.HasValue ? session.ShoppingCart.ReferenceNumber.ToString() : "N/A");
					xmlWriter.WriteAttributeString("createdOn", session.ShoppingCart.CreatedOn.ToString());
					xmlWriter.WriteAttributeString("discountCode", session.ShoppingCart.DiscountCode);
					xmlWriter.WriteAttributeString("discountValue", session.ShoppingCart.DiscountValue.ToString());
					xmlWriter.WriteAttributeString("useCredits", session.ShoppingCart.UseCredits.ToString());
					foreach (KeyValuePair<string, IShoppingCartLineItemGroup> group in session.ShoppingCart.Groups)
					{
						xmlWriter.WriteStartElement("shoppingCartLineItemGroup");
						xmlWriter.WriteAttributeString("name", group.Key);
						xmlWriter.WriteAttributeString("taxRate", group.Value.Totals.TaxRate.ToString());
						xmlWriter.WriteAttributeString("subTotal", group.Value.Totals.SubTotal.ToString());
						xmlWriter.WriteAttributeString("taxAmount", group.Value.Totals.TaxAmount.ToString());
						xmlWriter.WriteAttributeString("grandTotal", group.Value.Totals.GrandTotal.ToString());
						xmlWriter.WriteAttributeString("discountedSubTotal", group.Value.Totals.DiscountedSubtotal.ToString());
						foreach (IShoppingCartLineItem item in group.Value.Items)
						{
							xmlWriter.WriteStartElement("shoppingCartLineItem");
							xmlWriter.WriteAttributeString("barcode", item.Barcode);
							xmlWriter.WriteAttributeString("status", item.Status.ToString());
							xmlWriter.WriteAttributeString("quantity", item.Quantity.ToString());
							xmlWriter.WriteAttributeString("price", item.Price.ToString());
							xmlWriter.WriteAttributeString("extension", (item.Price * item.Quantity).ToString());
							foreach (KeyValuePair<string, decimal> price in item.Prices)
							{
								xmlWriter.WriteStartElement("shoppingCartLineItemPrice");
								xmlWriter.WriteAttributeString("name", price.Key);
								xmlWriter.WriteAttributeString("amount", price.Value.ToString());
								xmlWriter.WriteEndElement();
							}
							xmlWriter.WriteEndElement();
						}
						xmlWriter.WriteEndElement();
					}
					xmlWriter.WriteEndElement();
				}
				xmlWriter.WriteStartElement("events");
				session.ForEachEvent(delegate(IShoppingSessionEvent sessionEvent)
				{
					xmlWriter.WriteStartElement("event");
					xmlWriter.WriteAttributeString("timeStamp", sessionEvent.TimeStamp.ToString());
					xmlWriter.WriteAttributeString("type", sessionEvent.Type.ToString());
					xmlWriter.WriteAttributeString("description", sessionEvent.Description);
					xmlWriter.WriteEndElement();
				});
				xmlWriter.WriteEndElement();
				xmlWriter.WriteEndElement();
			});
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndDocument();
			xmlWriter.Close();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was rasied in ShoppingSessionService.ExporToXml.", e);
		}
	}

	public void ForEach(Action<IShoppingSession> action)
	{
		if (Connection == null)
		{
			return;
		}
		try
		{
			using VistaDBCommand vistaDBCommand = new VistaDBCommand();
			vistaDBCommand.Connection = Connection;
			vistaDBCommand.CommandText = "SELECT * FROM Session ORDER BY StartedOn DESC";
			using VistaDBDataReader vistaDBDataReader = vistaDBCommand.ExecuteReader();
			while (vistaDBDataReader.Read())
			{
				ShoppingSession shoppingSession = new ShoppingSession((Guid)vistaDBDataReader["ID"], logStartEvent: false);
				shoppingSession.EndedOn = (DateTime?)vistaDBDataReader["EndedOn"];
				shoppingSession.StartedOn = (DateTime)vistaDBDataReader["StartedOn"];
				shoppingSession.StoreNumber = (string)vistaDBDataReader["StoreNumber"];
				shoppingSession.Status = (ShoppingSessionStatus)Convert.ToInt32(vistaDBDataReader["Status"]);
				ShoppingSession obj = shoppingSession;
				action(obj);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in ShoppingSessionService.ForEach.", e);
		}
	}

	public ErrorList Initialize(string path)
	{
		ServiceLocator.Instance.AddService(typeof(IShoppingSessionService), Instance);
		ErrorList errorList = new ErrorList();
		try
		{
			DatabasePath = path;
			if (File.Exists(DatabasePath) && DateTime.Now.DayOfWeek == DayOfWeek.Monday)
			{
				using IVistaDBDDA vistaDBDDA = VistaDBEngine.Connections.OpenDDA();
				vistaDBDDA.PackDatabase(DatabasePath, null, backup: true, null);
			}
			SchemaMigrator.Migrate(DatabasePath);
			Connection = new VistaDBConnection($"Data Source={DatabasePath};Open Mode=ExclusiveReadWrite;");
			Connection.Open();
			using (VistaDBCommand vistaDBCommand = new VistaDBCommand())
			{
				vistaDBCommand.Connection = Connection;
				vistaDBCommand.CommandText = "DELETE FROM Session WHERE StartedOn < @CleanUpDate";
				DateTime dateTime = DateTime.Now.AddDays(-60.0);
				vistaDBCommand.Parameters.Add("@CleanUpDate", dateTime);
				vistaDBCommand.ExecuteNonQuery();
			}
			ServiceLocator.Instance.GetService<IPreferenceService>()?.AddPreferencePage("ShoppingSessionServices_General", "Remote Services\\Session Services", "Engine Core/Shopping Sessions", PreferencePageTarget.LocalSystem, new ShoppingSessionPreferencePage());
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in ShoppingSessionServie.Initialize.", e);
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("S999", "Unable to initialize the Shopping Session Service database.", e));
			try
			{
				string text = DatabasePath + ".corrupt";
				LogHelper.Instance.Log("Attempt to move '{0}' to '{1}'.", DatabasePath, text);
				File.Move(DatabasePath, text);
			}
			catch (Exception e2)
			{
				LogHelper.Instance.Log("An unhandled exception was raised renaming a corrupt Shopping Session Service data file.", e2);
			}
		}
		return errorList;
	}

	public void SetCurrentSession(string id)
	{
		LogHelper.Instance.Log("Setting current Shopping Session to: {0}", id);
		m_currentSessionId = id;
	}

	private ShoppingSessionService()
	{
	}

	private IShoppingSession Load(string id)
	{
		using (VistaDBCommand vistaDBCommand = new VistaDBCommand())
		{
			vistaDBCommand.Connection = Connection;
			vistaDBCommand.CommandText = "SELECT * FROM Session WHERE ID = @ID";
			vistaDBCommand.Parameters.Add("@ID", id);
			using VistaDBDataReader vistaDBDataReader = vistaDBCommand.ExecuteReader();
			if (vistaDBDataReader.Read())
			{
				ShoppingSession shoppingSession = new ShoppingSession((Guid)vistaDBDataReader["ID"], logStartEvent: false);
				shoppingSession.EndedOn = (DateTime?)vistaDBDataReader["EndedOn"];
				shoppingSession.StartedOn = (DateTime)vistaDBDataReader["StartedOn"];
				shoppingSession.StoreNumber = (string)vistaDBDataReader["StoreNumber"];
				shoppingSession.Status = (ShoppingSessionStatus)Convert.ToInt32(vistaDBDataReader["Status"]);
				return shoppingSession;
			}
		}
		return null;
	}
}
