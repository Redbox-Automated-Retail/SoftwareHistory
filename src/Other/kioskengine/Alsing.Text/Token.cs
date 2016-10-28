using System;
using System.Runtime.InteropServices;

namespace Alsing.Text;

[ComVisible(true)]
public sealed class Token
{
	public string Text { get; private set; }

	public object[] Tags { get; private set; }

	public Token(string text, object[] tags)
	{
		if (tags == null)
		{
			tags = new object[0];
		}
		Text = text;
		Tags = tags;
	}

	public override string ToString()
	{
		return Text;
	}

	public bool HasTag(object tag)
	{
		return Array.IndexOf(Tags, tag) >= 0;
	}
}
