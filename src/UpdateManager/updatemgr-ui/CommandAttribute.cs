using System;

namespace Redbox.IPC.Framework;

[AttributeUsage(AttributeTargets.Class)]
internal class CommandAttribute : Attribute
{
	public string Name { get; set; }

	public CommandAttribute(string name)
	{
		Name = name;
	}
}
