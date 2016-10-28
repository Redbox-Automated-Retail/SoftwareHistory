using System;
using System.Runtime.InteropServices;
using Alsing.Text.PatternMatchers;

namespace Alsing.Text;

[ComVisible(true)]
public class TokenTreeNode
{
	public bool CaseSensitive = true;

	public char Char;

	public TokenTreeNode[] ChildNodes;

	public bool ContainsCaseInsensitiveData;

	public long Count;

	public PatternMatchReference FirstExpression;

	public bool IsEnd;

	public bool NeedSeparators;

	public TokenTreeNode NextSibling;

	public object[] Tags;

	public TokenTreeNode()
	{
		ChildNodes = new TokenTreeNode[256];
	}

	public override string ToString()
	{
		if (Tags != null)
		{
			return Tags.ToString();
		}
		return "TokenTreeNode " + Char;
	}

	public void AddPattern(string prefix, bool caseSensitive, bool needSeparators, IPatternMatcher matcher, object[] tags)
	{
		if (string.IsNullOrEmpty(prefix))
		{
			throw new ArgumentNullException("prefix");
		}
		TokenTreeNode tokenTreeNode = AddTokenInternal(prefix, caseSensitive);
		PatternMatchReference patternMatchReference = new PatternMatchReference(matcher);
		patternMatchReference.NextSibling = FirstExpression;
		patternMatchReference.Tags = tags;
		patternMatchReference.NeedSeparators = needSeparators;
		PatternMatchReference firstExpression = patternMatchReference;
		tokenTreeNode.FirstExpression = firstExpression;
	}

	public void AddPattern(bool caseSensitive, bool needSeparators, IPatternMatcher matcher, object[] tags)
	{
		PatternMatchReference patternMatchReference = new PatternMatchReference(matcher);
		patternMatchReference.NextSibling = FirstExpression;
		patternMatchReference.Tags = tags;
		patternMatchReference.NeedSeparators = needSeparators;
		PatternMatchReference firstExpression = patternMatchReference;
		FirstExpression = firstExpression;
	}

	public void AddToken(string token, bool caseSensitive, bool needSeparators, object[] tags)
	{
		if (string.IsNullOrEmpty(token))
		{
			throw new ArgumentNullException("token");
		}
		TokenTreeNode tokenTreeNode = AddTokenInternal(token, caseSensitive);
		tokenTreeNode.IsEnd = true;
		tokenTreeNode.Tags = tags;
		tokenTreeNode.NeedSeparators = needSeparators;
		tokenTreeNode.CaseSensitive = caseSensitive;
	}

	public TokenTreeNode AddTokenInternal(string token, bool caseSensitive)
	{
		Char = token[0];
		if (!caseSensitive)
		{
			ContainsCaseInsensitiveData = true;
		}
		if (token.Length == 1)
		{
			return this;
		}
		string text = token.Substring(1);
		char c = text[0];
		int num = c & 0xFF;
		TokenTreeNode tokenTreeNode = ChildNodes[num];
		TokenTreeNode result;
		if (tokenTreeNode == null)
		{
			TokenTreeNode tokenTreeNode2 = new TokenTreeNode();
			ChildNodes[num] = tokenTreeNode2;
			result = tokenTreeNode2.AddTokenInternal(text, caseSensitive);
			MakeRepeatingWS(tokenTreeNode2);
		}
		else
		{
			tokenTreeNode = GetMatchingNode(c, tokenTreeNode);
			result = tokenTreeNode.AddTokenInternal(text, caseSensitive);
		}
		return result;
	}

	private static void MakeRepeatingWS(TokenTreeNode child)
	{
		if (child.Char == ' ')
		{
			child.ChildNodes[32] = child;
		}
	}

	private static TokenTreeNode GetMatchingNode(char childChar, TokenTreeNode node)
	{
		while (node.NextSibling != null && node.Char != childChar)
		{
			node = node.NextSibling;
		}
		if (node.Char != childChar)
		{
			return node.NextSibling = new TokenTreeNode();
		}
		return node;
	}

	public TokenTreeNode GetNextNode(char c)
	{
		char c2 = c;
		if (ContainsCaseInsensitiveData)
		{
			c2 = CharUtils.ToLower(c);
		}
		int num = c2 & 0xFF;
		TokenTreeNode tokenTreeNode;
		for (tokenTreeNode = ChildNodes[num]; tokenTreeNode != null; tokenTreeNode = tokenTreeNode.NextSibling)
		{
			c2 = c;
			if (tokenTreeNode.ContainsCaseInsensitiveData)
			{
				c2 = CharUtils.ToLower(c);
			}
			if (tokenTreeNode.Char == c2)
			{
				break;
			}
		}
		return tokenTreeNode;
	}
}
