using System;
using System.Collections.Generic;

namespace Redbox.HAL.Component.Model;

internal interface IUsbDeviceSearchResult : IDisposable
{
	bool Found { get; }

	ErrorList Errors { get; }

	IList<IDeviceDescriptor> Matches { get; }
}
