using System;
using System.Collections.Generic;
using Redbox.Core;

namespace Redbox.IPC.Framework;

internal interface ISession : IMessageSink
{
	ParameterDictionary Context { get; }

	IDictionary<string, object> Properties { get; }

	event EventHandler Disconnect;

	void Start();

	bool IsConnected();
}
