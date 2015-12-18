using System;

namespace Redbox.Core;

internal class Redbox_002ECommand_002ETokenizer2108_002ECustomEditorAttribute : Attribute
{
	public string Text { get; private set; }

	public string GetMethodName { get; set; }

	public string SetMethodName { get; set; }

	public Redbox_002ECommand_002ETokenizer2108_002ECustomEditorAttribute(string text)
	{
		Text = text;
	}
}
