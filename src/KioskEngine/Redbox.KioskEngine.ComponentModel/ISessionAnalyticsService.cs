using System.Collections.Generic;

namespace Redbox.KioskEngine.ComponentModel;

public interface ISessionAnalyticsService
{
	void Start(string sessionId, AnalyticsSessionType sessionType);

	void AddView(string sessionId, string viewName);

	void Close(string sessionId);

	void DisposeSession(string sessionId);

	bool IsOpen(string sessionId);

	bool SessionExists(string sessionId);

	void AddNode(string parentNodeName, string nodeName);

	void AddElement(string parentNodeName, string elementName, string elementValue);

	void AddElements(string parentNodeName, Dictionary<string, string> elements);

	void AddAttribute(string parentNodeName, string attributeName, string attributeValue);

	void AddAttributes(string parentNodeName, Dictionary<string, string> attributes);

	void AddNode(string parentNodeName, string nodeName, string bufferedSessionID);

	void AddNode(string parentNodeName, string nodeName, Dictionary<string, string> attributes, string bufferedSessionID);

	void AddElement(string parentNodeName, string elementName, string elementValue, string bufferedSessionID);

	void AddElements(string parentNodeName, Dictionary<string, string> elements, string bufferedSessionID);

	void AddAttribute(string parentNodeName, string attributeName, string attributeValue, string bufferedSessionID);

	void AddAttributes(string parentNodeName, Dictionary<string, string> attributes, string bufferedSessionID);
}
