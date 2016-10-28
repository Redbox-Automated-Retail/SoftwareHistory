using System;
using System.Runtime.InteropServices;
using Alsing.Text.PatternMatchers;

namespace Alsing.Text;

[ComVisible(true)]
public class TokenTree
{
	private readonly TokenTreeNode[] nodes;

	private readonly TokenTreeNode root;

	private readonly char[] textLookup;

	private bool[] separatorCharLookup;

	private string separators;

	public string Separators
	{
		get
		{
			return separators;
		}
		set
		{
			separators = value;
			separatorCharLookup = new bool[65536];
			foreach (char c in value)
			{
				separatorCharLookup[(uint)c] = true;
			}
		}
	}

	public void AddPattern(IPatternMatcher matcher, bool caseSensitive, bool needSeparators, object[] tags)
	{
		if (matcher == null)
		{
			throw new ArgumentNullException("matcher");
		}
		AddPattern(null, matcher, caseSensitive, needSeparators, tags);
	}

	public void AddPattern(string prefix, IPatternMatcher matcher, bool caseSensitive, bool needSeparators, object[] tags)
	{
		if (string.IsNullOrEmpty(prefix))
		{
			AddPatternWithoutPrefix(matcher, caseSensitive, needSeparators, tags);
		}
		else if (caseSensitive)
		{
			AddPatternWithCaseSensitivePrefix(prefix, matcher, needSeparators, tags);
		}
		else
		{
			AddPatternWithCaseInsensitivePrefix(prefix, matcher, needSeparators, tags);
		}
	}

	public void AddToken(string text, bool caseSensitive, bool needSeparators, object[] tags)
	{
		if (string.IsNullOrEmpty(text))
		{
			throw new ArgumentNullException(text);
		}
		if (caseSensitive)
		{
			AddCaseSensitiveToken(text, needSeparators, tags);
		}
		else
		{
			AddCaseInsensitiveToken(text, needSeparators, tags);
		}
	}

	private void AddCaseInsensitiveToken(string text, bool needSeparators, object[] tags)
	{
		text = text.ToLower();
		char c = text[0];
		int num = c;
		if (nodes[num] == null)
		{
			nodes[num] = new TokenTreeNode();
		}
		nodes[num].AddToken(text, caseSensitive: false, needSeparators, tags);
		text = char.ToUpper(c) + text.Substring(1);
		c = text[0];
		num = c;
		if (nodes[num] == null)
		{
			nodes[num] = new TokenTreeNode();
		}
		nodes[num].AddToken(text, caseSensitive: false, needSeparators, tags);
	}

	private void AddCaseSensitiveToken(string text, bool needSeparators, object[] tags)
	{
		char c = text[0];
		int num = c;
		if (nodes[num] == null)
		{
			nodes[num] = new TokenTreeNode();
		}
		nodes[num].AddToken(text, caseSensitive: true, needSeparators, tags);
	}

	private void AddPatternWithCaseInsensitivePrefix(string prefix, IPatternMatcher matcher, bool needSeparators, object[] tags)
	{
		prefix = prefix.ToLower();
		char c = prefix[0];
		int num = c;
		if (nodes[num] == null)
		{
			nodes[num] = new TokenTreeNode();
		}
		nodes[num].AddPattern(prefix, caseSensitive: false, needSeparators, matcher, tags);
		prefix = char.ToUpper(c) + prefix.Substring(1);
		c = prefix[0];
		num = c;
		if (nodes[num] == null)
		{
			nodes[num] = new TokenTreeNode();
		}
		nodes[num].AddPattern(prefix, caseSensitive: false, needSeparators, matcher, tags);
	}

	private void AddPatternWithCaseSensitivePrefix(string prefix, IPatternMatcher matcher, bool needSeparators, object[] tags)
	{
		char c = prefix[0];
		int num = c;
		if (nodes[num] == null)
		{
			nodes[num] = new TokenTreeNode();
		}
		nodes[num].AddPattern(prefix, caseSensitive: true, needSeparators, matcher, tags);
	}

	private void AddPatternWithoutPrefix(IPatternMatcher matcher, bool caseSensitive, bool needSeparators, object[] tags)
	{
		if (matcher.DefaultPrefixes != null)
		{
			string[] defaultPrefixes = matcher.DefaultPrefixes;
			foreach (string prefix in defaultPrefixes)
			{
				AddPattern(prefix, matcher, caseSensitive, needSeparators, tags);
			}
		}
		else
		{
			PatternMatchReference patternMatchReference = new PatternMatchReference(matcher);
			patternMatchReference.Tags = tags;
			patternMatchReference.NextSibling = root.FirstExpression;
			patternMatchReference.NeedSeparators = needSeparators;
			PatternMatchReference firstExpression = patternMatchReference;
			root.FirstExpression = firstExpression;
		}
	}

	public TokenTree()
	{
		nodes = new TokenTreeNode[65536];
		Separators = ".,;:<>[](){}!\"#¤%&/=?*+-/\\ \t\n\r";
		textLookup = new char[65536];
		for (int i = 0; i < 65536; i++)
		{
			textLookup[i] = (char)i;
		}
		textLookup[9] = ' ';
		root = new TokenTreeNode();
	}

	public MatchResult Match(string text, int startIndex)
	{
		if (string.IsNullOrEmpty(text))
		{
			throw new ArgumentNullException(text);
		}
		MatchResult result = new MatchResult
		{
			Text = text
		};
		int length = text.Length;
		for (int i = startIndex; i < length; i++)
		{
			if (root.FirstExpression != null)
			{
				for (PatternMatchReference patternMatchReference = root.FirstExpression; patternMatchReference != null; patternMatchReference = patternMatchReference.NextSibling)
				{
					int num = patternMatchReference.Matcher.Match(text, i);
					if (num > 0 && num > result.Length)
					{
						bool flag = i == 0 || separatorCharLookup[(uint)text[i - 1]];
						bool flag2 = i + num == length || separatorCharLookup[(uint)text[i + num]];
						if (!patternMatchReference.NeedSeparators || (flag && flag2))
						{
							result.Index = i;
							result.Length = num;
							result.Found = true;
							result.Tags = patternMatchReference.Tags;
						}
					}
				}
			}
			TokenTreeNode tokenTreeNode = nodes[(uint)text[i]];
			if (tokenTreeNode == null)
			{
				if (result.Found)
				{
					break;
				}
				continue;
			}
			for (int j = i + 1; j <= length; j++)
			{
				if (tokenTreeNode.FirstExpression != null)
				{
					for (PatternMatchReference patternMatchReference2 = tokenTreeNode.FirstExpression; patternMatchReference2 != null; patternMatchReference2 = patternMatchReference2.NextSibling)
					{
						int num2 = patternMatchReference2.Matcher.Match(text, j);
						if (num2 > 0 && num2 > result.Length)
						{
							bool flag3 = i == 0 || separatorCharLookup[(uint)text[i - 1]];
							bool flag4 = i + num2 + j == length || separatorCharLookup[(uint)text[i + num2 + j]];
							if (!patternMatchReference2.NeedSeparators || (flag3 && flag4))
							{
								result.Index = i;
								result.Length = num2 + j - i;
								result.Found = true;
								result.Tags = patternMatchReference2.Tags;
							}
						}
					}
				}
				if (tokenTreeNode.IsEnd && j - i >= result.Length)
				{
					bool flag5 = i == 0 || separatorCharLookup[(uint)text[i - 1]];
					bool flag6 = j == length || separatorCharLookup[(uint)text[j]];
					if (!tokenTreeNode.NeedSeparators || (flag5 && flag6))
					{
						result.Index = i;
						result.Tags = tokenTreeNode.Tags;
						result.Found = true;
						result.Length = j - i;
						if (i + result.Length == length)
						{
							break;
						}
					}
				}
				tokenTreeNode = tokenTreeNode.GetNextNode(textLookup[(uint)text[j]]);
				if (tokenTreeNode == null)
				{
					break;
				}
			}
			if (result.Found)
			{
				return result;
			}
		}
		if (result.Found)
		{
			return result;
		}
		return MatchResult.NoMatch;
	}
}
