using System.Collections.Generic;

namespace Redbox.KioskEngine.ComponentModel;

public interface ISessionAnalyticsService
{
	int MessagePriority { get; set; }

	void Start(string sessionId, AnalyticsSessionType sessionType);

	void AddView(string sessionId, string viewName);

	void AddViewEvent(string subItemType, string subItemValue);

	void AddViewEvent2(string subItemType, string subItemValue, string sessionId);

	void Close(string sessionId);

	void DisposeSession(string sessionId);

	bool IsOpen(string sessionId);

	bool SessionExists(string sessionId);

	void AddNode(string parentNodeName, string nodeName);

	void AddElement(string parentNodeName, string elementName, string elementValue);

	void AddElements(string parentNodeName, Dictionary<string, string> elements);

	void AddOrUpdateElement(string parentNodeName, string elementName, string elementValue);

	void AddOrUpdateElement(string parentNodeName, string elementName, string elementValue, string bufferedSessionID);

	void RemoveElement(string parentNodeName, string elementName);

	void RemoveElement(string parentNodeName, string elementName, string bufferedSessionId);

	void AddAttribute(string parentNodeName, string attributeName, string attributeValue);

	void AddAttributes(string parentNodeName, Dictionary<string, string> attributes);

	void AddNode(string parentNodeName, string nodeName, string bufferedSessionID);

	void AddNode(string parentNodeName, string nodeName, Dictionary<string, string> attributes, string bufferedSessionID);

	void AddElement(string parentNodeName, string elementName, string elementValue, string bufferedSessionID);

	void AddElements(string parentNodeName, Dictionary<string, string> elements, string bufferedSessionID);

	void AddAttribute(string parentNodeName, string attributeName, string attributeValue, string bufferedSessionID);

	void AddAttributes(string parentNodeName, Dictionary<string, string> attributes, string bufferedSessionID);

	void AddLoggedInCustomerProfileNumber(string customerProfileNumber);

	void AddLoggedInCustomerProfileNumber(string customerProfileNumber, string sessionId);

	void AddAuthTransactionInfo(string customerProfileNumber, string accountNumber, string transactionId);

	void AddAuthTransactionInfo(string customerProfileNumber, string accountNumber, string transactionId, string sessionId);
}
