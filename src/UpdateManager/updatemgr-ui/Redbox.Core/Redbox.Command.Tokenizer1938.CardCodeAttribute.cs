using System;
using System.Reflection;

namespace Redbox.Core;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
internal class Redbox_002ECommand_002ETokenizer1938_002ECardCodeAttribute : Attribute
{
	public string Code { get; set; }

	public static Redbox_002ECommand_002ETokenizer1938_002ECardCodeAttribute GetCode(Redbox_002ECommand_002ETokenizer1938_002ECardType cardType)
	{
		FieldInfo field = typeof(Redbox_002ECommand_002ETokenizer1938_002ECardType).GetField(cardType.ToString());
		if (field == null)
		{
			return null;
		}
		return Attribute.GetCustomAttribute(field, typeof(Redbox_002ECommand_002ETokenizer1938_002ECardCodeAttribute)) as Redbox_002ECommand_002ETokenizer1938_002ECardCodeAttribute;
	}
}
