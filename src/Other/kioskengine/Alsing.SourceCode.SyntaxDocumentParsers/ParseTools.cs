using System.Collections.Generic;
using System.Text;

namespace Alsing.SourceCode.SyntaxDocumentParsers;

public sealed class ParseTools
{
	public static void AddPatternString(string Text, Row Row, Pattern Pattern, TextStyle Style, Span span, bool HasError)
	{
		Word word = new Word();
		word.Style = Style;
		word.Pattern = Pattern;
		word.HasError = HasError;
		word.Span = span;
		word.Text = Text;
		Word word2 = word;
		Row.Add(word2);
	}

	public unsafe static void AddString(string Text, Row Row, TextStyle Style, Span span)
	{
		if (Text == "")
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		char[] array = Text.ToCharArray();
		fixed (char* ptr = &array[0])
		{
			for (int i = 0; i < Text.Length; i++)
			{
				if (ptr[i] == ' ' || ptr[i] == '\t')
				{
					if (stringBuilder.Length != 0)
					{
						Word word = Row.Add(stringBuilder.ToString());
						word.Style = Style;
						word.Span = span;
						stringBuilder = new StringBuilder();
					}
					Word word2 = Row.Add(ptr[i].ToString());
					if (ptr[i] == ' ')
					{
						word2.Type = WordType.Space;
					}
					else
					{
						word2.Type = WordType.Tab;
					}
					word2.Style = Style;
					word2.Span = span;
				}
				else
				{
					stringBuilder.Append(ptr[i].ToString());
				}
			}
			if (stringBuilder.Length != 0)
			{
				Word word3 = Row.Add(stringBuilder.ToString());
				word3.Style = Style;
				word3.Span = span;
			}
		}
	}

	public static List<string> GetWords(string text)
	{
		List<string> list = new List<string>();
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			if (c == ' ' || c == '\t')
			{
				if (stringBuilder.ToString() != "")
				{
					list.Add(stringBuilder.ToString());
					stringBuilder = new StringBuilder();
				}
				list.Add(c.ToString());
			}
			else
			{
				stringBuilder.Append(c.ToString());
			}
		}
		if (stringBuilder.ToString() != "")
		{
			list.Add(stringBuilder.ToString());
		}
		return list;
	}

	public static PatternScanResult GetFirstWord(char[] TextBuffer, PatternCollection Patterns, int StartPosition)
	{
		PatternScanResult result = default(PatternScanResult);
		result.Index = 0;
		result.Token = "";
		return result;
	}
}
