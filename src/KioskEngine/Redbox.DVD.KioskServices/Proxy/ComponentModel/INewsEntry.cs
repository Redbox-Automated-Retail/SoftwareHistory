using System;

namespace Redbox.KioskServices.Proxy.ComponentModel;

internal interface INewsEntry
{
	int Id { get; }

	string Body { get; }

	string Header { get; }

	DateTime EndOn { get; }

	DateTime StartOn { get; }
}
