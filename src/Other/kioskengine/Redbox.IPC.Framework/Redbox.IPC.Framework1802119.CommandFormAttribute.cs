using System;

namespace Redbox.IPC.Framework;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class Redbox_002EIPC_002EFramework1802119_002ECommandFormAttribute : Attribute
{
	public string Name { get; set; }

	public string Filter { get; set; }
}
