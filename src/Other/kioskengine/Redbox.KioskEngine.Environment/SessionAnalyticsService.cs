using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using System.Xml.Schema;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class SessionAnalyticsService : ISessionAnalyticsService
{
	internal sealed class AnalyticsSession
	{
		public XDocument document { get; set; }

		public string sessionId { get; set; }

		public DateTime startTime { get; set; }

		public AnalyticsSessionType sessionType { get; set; }

		public bool isOpen { get; set; }

		public AnalyticsSession(string newSessionId, AnalyticsSessionType newSessionType)
		{
			sessionId = newSessionId;
			sessionType = newSessionType;
			startTime = DateTime.Now;
			isOpen = true;
		}
	}

	internal Dictionary<string, AnalyticsSession> m_analyticsSessions = new Dictionary<string, AnalyticsSession>();

	private int m_working;

	public static SessionAnalyticsService Instance => Singleton<SessionAnalyticsService>.Instance;

	public string ActiveSession { get; set; }

	public void Start(string sessionId, AnalyticsSessionType sessionType)
	{
		LogHelper.Instance.Log($"Analytics:Start({sessionId}) requested.");
		if (SessionExists(sessionId))
		{
			LogHelper.Instance.Log("Session already exists, ignoring duplicate Start() request.");
			return;
		}
		try
		{
			AnalyticsSession analyticsSession = new AnalyticsSession(sessionId, sessionType);
			XDocument document = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement("session", new XElement("sessionId", sessionId), new XElement("sessionType", sessionType.ToString()), new XElement("sessionStart", analyticsSession.startTime.ToString("yyyy/MM/dd HH:mm:ss:ms")), new XElement("shoppingCart", new XElement("cartItems"), new XElement("ticketItems")), new XElement("promoCodes"), new XElement("authorizations"), new XElement("views")));
			analyticsSession.document = document;
			m_analyticsSessions.Add(sessionId, analyticsSession);
			ActiveSession = sessionId;
			IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			if (service != null)
			{
				dictionary.Add("application", service["ProductName"] ?? string.Empty);
				dictionary.Add("appVersion", service["ProductVersion"] ?? string.Empty);
			}
			IResourceBundleService service2 = ServiceLocator.Instance.GetService<IResourceBundleService>();
			if (service2 != null)
			{
				dictionary.Add("kioskID", service2.Filter["store_number"] ?? "Undefined");
				dictionary.Add("culture", service2.Filter["culture"] ?? string.Empty);
				dictionary.Add("market", service2.Filter["market"] ?? string.Empty);
				dictionary.Add("banner", service2.Filter["vendor"] ?? string.Empty);
			}
			AddElements("session", dictionary, sessionId);
			LogHelper.Instance.Log($"Analytics:Started({sessionId}) successfully.");
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was raised in SessionAnalyticsService.Start()", e);
		}
	}

	public void AddView(string sessionId, string viewName)
	{
		IShoppingSessionService service = ServiceLocator.Instance.GetService<IShoppingSessionService>();
		IViewService service2 = ServiceLocator.Instance.GetService<IViewService>();
		IResourceBundleService service3 = ServiceLocator.Instance.GetService<IResourceBundleService>();
		if (service2 == null || service == null || service3 == null)
		{
			return;
		}
		IViewFrame viewFrame = service2.GetViewFrame(viewName);
		IShoppingSession session = service.GetSession(sessionId);
		if (viewFrame != null && session != null)
		{
			if (session.ViewsShown == 0)
			{
				session.ViewsShown++;
			}
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("viewName", viewName);
			dictionary.Add("viewSequence", session.ViewsShown.ToString());
			dictionary.Add("viewVersion", viewFrame.ViewVersion ?? "not defined");
			dictionary.Add("activeFlag", viewFrame.ActiveFlag ?? "not defined");
			dictionary.Add("viewCulture", service3.Filter["culture"] ?? string.Empty);
			dictionary.Add("viewStart", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss:ms"));
			service2.ResetViewShownTime();
			AddNode("views", "view");
			AddAttributes("view", dictionary);
			AddNode("view", "viewEvents");
		}
	}

	public void Close(string sessionId)
	{
		LogHelper.Instance.Log($"Analytics:Close({sessionId}) requested");
		try
		{
			if (!m_analyticsSessions.ContainsKey(sessionId))
			{
				LogHelper.Instance.Log($"Requested analytics session ({sessionId}) not found.");
				return;
			}
			AnalyticsSession analyticsSession = m_analyticsSessions[sessionId];
			if (!analyticsSession.isOpen)
			{
				LogHelper.Instance.Log($"Requested analytics session ({sessionId}) was already closed.");
				return;
			}
			IViewService service = ServiceLocator.Instance.GetService<IViewService>();
			if (service != null && service.GetViewShownTime().HasValue)
			{
				AddAttribute("view", "viewEnd", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss:ms"), sessionId);
				DateTime now = DateTime.Now;
				DateTime? viewShownTime = service.GetViewShownTime();
				AddAttribute("view", "viewDuration", (now - viewShownTime).ToString(), sessionId);
			}
			AddElement("session", "sessionEnd", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss:ms"), sessionId);
			AddElement("session", "sessionDuration", (DateTime.Now - analyticsSession.startTime).ToString(), sessionId);
			analyticsSession.isOpen = false;
			BuildAndEnqueueMessage(sessionId);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was raised in SessionAnalyticsService.Close()", e);
		}
	}

	public void DisposeSession(string sessionId)
	{
		try
		{
			if (m_analyticsSessions.ContainsKey(sessionId))
			{
				m_analyticsSessions.Remove(sessionId);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was raised in SessionAnalyticsService.DisposeSession()", e);
		}
	}

	private void BuildAndEnqueueMessage(string sessionId)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			try
			{
				Interlocked.Increment(ref m_working);
				AnalyticsSession analyticsSession = m_analyticsSessions[sessionId];
				IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
				IQueueService service2 = ServiceLocator.Instance.GetService<IQueueService>();
				IResourceBundleService service3 = ServiceLocator.Instance.GetService<IResourceBundleService>();
				string s = service3.Filter["store_number"];
				XDocument document = analyticsSession.document;
				string text = document.ToString(SaveOptions.DisableFormatting);
				byte priority = (byte)((!(MachineSettingsStore.Instance.GetValue("Store", "SendAnalyticsOffPeak", "True") == "True")) ? 200 : 0);
				string name = "Redbox.KioskEngine.Environment.XMLSchema.KioskAnalyticsSchema.xsd";
				Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
				XmlSchema schema = XmlSchema.Read(manifestResourceStream, SchemaValidationHandler);
				XmlSchemaSet xmlSchemaSet = new XmlSchemaSet();
				xmlSchemaSet.Add(schema);
				bool validationErrored = false;
				document.Validate(xmlSchemaSet, delegate(object obj, ValidationEventArgs e2)
				{
					LogHelper.Instance.Log("{0}", e2.Message);
					validationErrored = true;
				});
				if (validationErrored)
				{
					LogHelper.Instance.Log("Kiosk Analytics XML is InValid", LogEntryType.Info);
				}
				else
				{
					LogHelper.Instance.Log("Kiosk Analytics XML is Valid", LogEntryType.Info);
				}
				if (!validationErrored)
				{
					service2.Enqueue(priority, "KioskAnalytics", new
					{
						MessageType = "KioskAnalytics",
						MessageId = Guid.NewGuid(),
						KioskId = int.Parse(s),
						EngineVersion = service["EngineVersion"],
						BundleVersion = service["ProductVersion"],
						SessionId = sessionId,
						StartTime = analyticsSession.startTime.ToString("yyyy/MM/dd HH:mm:ss"),
						EndTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
						SessionData = CompressString(text)
					});
					LogHelper.Instance.Log($"Analytics:Close({sessionId}) completed.");
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An exception was raised in SessionAnalyticsService.BuildAndEnqueueMessage()", e);
			}
			finally
			{
				Interlocked.Decrement(ref m_working);
				m_analyticsSessions.Remove(sessionId);
				LogHelper.Instance.Log($"Analytics:({sessionId}) session removed.");
			}
		});
	}

	private static void SchemaValidationHandler(object sender, ValidationEventArgs args)
	{
		LogHelper.Instance.Log(args.Message, args);
	}

	public bool IsOpen(string sessionId)
	{
		if (!m_analyticsSessions.ContainsKey(sessionId))
		{
			return false;
		}
		return m_analyticsSessions[sessionId].isOpen;
	}

	public bool SessionExists(string sessionId)
	{
		return m_analyticsSessions.ContainsKey(sessionId);
	}

	public void AddNode(string parentNodeName, string nodeName)
	{
		try
		{
			GetLastNode(parentNodeName)?.Add(new XElement(nodeName));
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An exception was raised in SessionAnalyticsService.AddNode({parentNodeName},{nodeName})", e);
		}
	}

	public void AddNode(string parentNodeName, string nodeName, string bufferedSessionID)
	{
		try
		{
			GetLastNode(parentNodeName, bufferedSessionID)?.Add(new XElement(nodeName));
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An exception was raised in SessionAnalyticsService.AddNode({parentNodeName},{nodeName},{bufferedSessionID})", e);
		}
	}

	public void AddNode(string parentNodeName, string nodeName, Dictionary<string, string> attributes, string bufferedSessionID)
	{
		try
		{
			XElement lastNode = GetLastNode(parentNodeName, bufferedSessionID);
			if (lastNode != null)
			{
				XElement xElement = new XElement(nodeName);
				xElement.Add(attributes.Select((KeyValuePair<string, string> o) => new XAttribute(o.Key, o.Value)));
				lastNode.Add(xElement);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An exception was raised in SessionAnalyticsService.AddNode({parentNodeName},{nodeName},{bufferedSessionID})", e);
		}
	}

	public void AddElement(string parentNodeName, string elementName, string elementValue)
	{
		try
		{
			GetLastNode(parentNodeName)?.Add(new XElement(elementName, elementValue));
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An exception was raised in SessionAnalyticsService.AddElement({parentNodeName},{elementName},{elementValue})", e);
		}
	}

	public void AddElement(string parentNodeName, string elementName, string elementValue, string bufferedSessionID)
	{
		try
		{
			GetLastNode(parentNodeName, bufferedSessionID)?.Add(new XElement(elementName, elementValue));
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An exception was raised in SessionAnalyticsService.AddElement({parentNodeName},{elementName},{elementValue},{bufferedSessionID})", e);
		}
	}

	public void AddElements(string parentNodeName, Dictionary<string, string> elements)
	{
		try
		{
			GetLastNode(parentNodeName)?.Add(elements.Select((KeyValuePair<string, string> o) => new XElement(o.Key, o.Value)));
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An exception was raised in SessionAnalyticsService.AddElements({parentNodeName})", e);
		}
	}

	public void AddElements(string parentNodeName, Dictionary<string, string> elements, string bufferedSessionID)
	{
		try
		{
			GetLastNode(parentNodeName, bufferedSessionID)?.Add(elements.Select((KeyValuePair<string, string> o) => new XElement(o.Key, o.Value)));
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An exception was raised in SessionAnalyticsService.AddElements({parentNodeName},{bufferedSessionID})", e);
		}
	}

	public void AddAttribute(string parentNodeName, string attributeName, string attributeValue)
	{
		try
		{
			XElement lastNode = GetLastNode(parentNodeName);
			if (lastNode != null)
			{
				XAttribute xAttribute = lastNode.Attribute(attributeName);
				if (xAttribute == null)
				{
					lastNode.Add(new XAttribute(attributeName, attributeValue));
				}
				else
				{
					xAttribute.SetValue(attributeValue);
				}
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An exception was raised in SessionAnalyticsService.AddAttribute({parentNodeName},{attributeName},{attributeValue})", e);
		}
	}

	public void AddAttribute(string parentNodeName, string attributeName, string attributeValue, string bufferedSessionID)
	{
		try
		{
			XElement lastNode = GetLastNode(parentNodeName, bufferedSessionID);
			if (lastNode != null)
			{
				XAttribute xAttribute = lastNode.Attribute(attributeName);
				if (xAttribute == null)
				{
					lastNode.Add(new XAttribute(attributeName, attributeValue));
				}
				else
				{
					xAttribute.SetValue(attributeValue);
				}
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An exception was raised in SessionAnalyticsService.AddAttribute({parentNodeName},{attributeName},{attributeValue},{bufferedSessionID})", e);
		}
	}

	public void AddAttributes(string parentNodeName, Dictionary<string, string> attributes)
	{
		try
		{
			GetLastNode(parentNodeName)?.Add(attributes.Select((KeyValuePair<string, string> o) => new XAttribute(o.Key, o.Value)));
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An exception was raised in SessionAnalyticsService.AddAttributes({parentNodeName})", e);
		}
	}

	public void AddAttributes(string parentNodeName, Dictionary<string, string> attributes, string bufferedSessionID)
	{
		try
		{
			GetLastNode(parentNodeName, bufferedSessionID)?.Add(attributes.Select((KeyValuePair<string, string> o) => new XAttribute(o.Key, o.Value)));
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An exception was raised in SessionAnalyticsService.AddAttributes({parentNodeName},{bufferedSessionID})", e);
		}
	}

	private SessionAnalyticsService()
	{
	}

	private XElement GetLastNode(string parentNodeName)
	{
		if (ActiveSession != null)
		{
			return GetLastNode(parentNodeName, ActiveSession);
		}
		return null;
	}

	private XElement GetLastNode(string parentNodeName, XDocument document)
	{
		IEnumerable<XElement> source = document.Descendants(parentNodeName);
		if (!source.Any())
		{
			return null;
		}
		return source.Last();
	}

	private XElement GetLastNode(string parentNodeName, string sessionID)
	{
		AnalyticsSession analyticsSession = (m_analyticsSessions.ContainsKey(sessionID) ? m_analyticsSessions[sessionID] : null);
		if (analyticsSession == null)
		{
			return null;
		}
		return GetLastNode(parentNodeName, analyticsSession.document);
	}

	private static string CompressString(string text)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(text);
		MemoryStream memoryStream = new MemoryStream();
		using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, leaveOpen: true))
		{
			gZipStream.Write(bytes, 0, bytes.Length);
		}
		memoryStream.Position = 0L;
		byte[] array = new byte[memoryStream.Length];
		memoryStream.Read(array, 0, array.Length);
		byte[] array2 = new byte[array.Length + 4];
		Buffer.BlockCopy(array, 0, array2, 4, array.Length);
		Buffer.BlockCopy(BitConverter.GetBytes(bytes.Length), 0, array2, 0, 4);
		return Convert.ToBase64String(array2);
	}
}
