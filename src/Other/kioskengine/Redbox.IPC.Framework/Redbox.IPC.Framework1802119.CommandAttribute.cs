using System;

namespace Redbox.IPC.Framework;

[AttributeUsage(AttributeTargets.Class)]
public class Redbox_002EIPC_002EFramework1802119_002ECommandAttribute : Attribute
{
	public string Name { get; set; }

	public string Filter { get; set; }

	public Redbox_002EIPC_002EFramework1802119_002ECommandAttribute(string name)
	{
		Name = name;
	}
}
