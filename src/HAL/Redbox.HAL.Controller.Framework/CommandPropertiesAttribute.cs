using System;
using System.Reflection;

namespace Redbox.HAL.Controller.Framework;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class CommandPropertiesAttribute : Attribute
{
	public int Timeout { get; set; }

	public int StatusBit { get; set; }

	public string Command { get; set; }

	public string Revision { get; set; }

	public int CommandWait { get; set; }

	public string ResetCommand { get; set; }

	public AddressSelector Address { get; set; }

	public AddressSelector StatusAddress { get; set; }

	public int WaitPauseTime { get; set; }

	public int MultiProcWaitTime { get; set; }

	public CommandPropertiesAttribute()
	{
		Timeout = -1;
		StatusBit = -1;
		Revision = "*";
		CommandWait = -1;
		Address = AddressSelector.None;
		StatusAddress = AddressSelector.None;
		WaitPauseTime = -1;
	}

	public static CommandPropertiesAttribute GetProperties(FieldInfo fieldInfo, string revision)
	{
		CommandPropertiesAttribute[] array = (CommandPropertiesAttribute[])Attribute.GetCustomAttributes(fieldInfo, typeof(CommandPropertiesAttribute));
		CommandPropertiesAttribute[] array2 = array;
		foreach (CommandPropertiesAttribute commandPropertiesAttribute in array2)
		{
			if (string.Compare(commandPropertiesAttribute.Revision, revision, ignoreCase: true) == 0)
			{
				return commandPropertiesAttribute;
			}
		}
		CommandPropertiesAttribute[] array3 = array;
		foreach (CommandPropertiesAttribute commandPropertiesAttribute2 in array3)
		{
			if (commandPropertiesAttribute2.Revision == "*")
			{
				return commandPropertiesAttribute2;
			}
		}
		return null;
	}
}
