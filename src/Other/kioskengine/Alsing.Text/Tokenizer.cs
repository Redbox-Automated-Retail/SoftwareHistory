using System;
using System.Collections.Generic;
using Alsing.Text.PatternMatchers;

namespace Alsing.Text;

public class Tokenizer
{
	private readonly TokenTree tree;

	public bool IsImmutable { get; private set; }

	public string Text { get; set; }

	public Tokenizer()
	{
		tree = new TokenTree();
	}

	public Tokenizer AddPattern(IPatternMatcher matcher, bool caseSensitive, bool needsSeparators, params object[] tags)
	{
		ThrowIfImmutable();
		tree.AddPattern(matcher, caseSensitive, needsSeparators, tags);
		return this;
	}

	public Tokenizer AddToken(string token, bool caseSensitive, bool needsSeparators, params object[] tags)
	{
		ThrowIfImmutable();
		tree.AddToken(token, caseSensitive, needsSeparators, tags);
		return this;
	}

	public Token[] Tokenize()
	{
		if (Text == null)
		{
			throw new ArgumentNullException("Text");
		}
		MakeImmutable();
		List<Token> list = new List<Token>();
		int num = 0;
		while (num < Text.Length)
		{
			MatchResult matchResult = tree.Match(Text, num);
			if (matchResult.Found)
			{
				string text = Text.Substring(num, matchResult.Index - num);
				Token item = new Token(text, null);
				list.Add(item);
				Token item2 = new Token(matchResult.GetText(), matchResult.Tags);
				num = matchResult.Index + matchResult.Length;
				list.Add(item2);
			}
			else
			{
				string text2 = Text.Substring(num);
				Token item3 = new Token(text2, null);
				list.Add(item3);
				num = Text.Length;
			}
		}
		return list.ToArray();
	}

	private void ThrowIfImmutable()
	{
		if (IsImmutable)
		{
			throw new Exception("Tokens can not be added to an immutable tokenizer");
		}
	}

	public void MakeImmutable()
	{
		IsImmutable = true;
	}
}
