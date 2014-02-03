using System;

namespace Redbox.IPC.Framework;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
internal class CommandFormAttribute : Attribute
{
	public string Name { get; set; }
}
