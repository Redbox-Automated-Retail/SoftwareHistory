using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskServices.Proxy.ComponentModel;

public interface IRemoteServiceResult
{
	bool Success { get; }

	ErrorList Errors { get; }

	TimeSpan ExecutionTime { get; }

	IDictionary<string, object> Properties { get; }

	ReadOnlyCollection<IRemoteServiceProviderInstruction> Instructions { get; }
}
