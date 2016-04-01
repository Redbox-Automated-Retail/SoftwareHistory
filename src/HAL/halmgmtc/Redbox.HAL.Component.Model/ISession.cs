using System;
using System.Collections.Generic;

namespace Redbox.HAL.Component.Model;

public interface ISession : IMessageSink
{
	bool LogDetailedMessages { get; }

	IDictionary<string, string> Context { get; }

	event EventHandler Disconnect;

	void Start();
}
