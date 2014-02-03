using System.Text.RegularExpressions;

namespace Alsing.SourceCode;

public sealed class Pattern
{
	public static readonly string DefaultSeparators = ".,+-*^\\/()[]{}@:;'?£$#%& \t=<>";

	private string _Separators = DefaultSeparators;

	private string _StringPattern = "";

	public BracketType BracketType;

	public string Category;

	public bool IsComplex;

	public bool IsKeyword;

	public bool IsMultiLineBracket = true;

	public bool IsSeparator;

	public string LowerStringPattern = "";

	public Pattern MatchingBracket;

	public PatternList Parent;

	internal Regex rx;

	public string Separators
	{
		get
		{
			return _Separators;
		}
		set
		{
			_Separators = value;
		}
	}

	public string StringPattern
	{
		get
		{
			return _StringPattern;
		}
		set
		{
			_StringPattern = value;
			LowerStringPattern = _StringPattern.ToLowerInvariant();
		}
	}

	public bool ContainsSeparator
	{
		get
		{
			string stringPattern = StringPattern;
			foreach (char value in stringPattern)
			{
				if (Separators.IndexOf(value) >= 0)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool HasSeparators(string text, int position)
	{
		if (CharIsSeparator(text, position - 1))
		{
			return CharIsSeparator(text, position + StringPattern.Length);
		}
		return false;
	}

	private bool CharIsSeparator(string Text, int Position)
	{
		if (Position < 0 || Position >= Text.Length)
		{
			return true;
		}
		string value = Text.Substring(Position, 1);
		if (Separators.IndexOf(value) >= 0)
		{
			return true;
		}
		return false;
	}

	public PatternScanResult IndexIn(string text, int startPosition, bool matchCase, string separators)
	{
		if (separators != null)
		{
			Separators = separators;
		}
		if (!IsComplex)
		{
			if (!IsKeyword)
			{
				return SimpleFind(text, startPosition, matchCase);
			}
			return SimpleFindKeyword(text, startPosition, matchCase);
		}
		if (!IsKeyword)
		{
			return ComplexFind(text, startPosition);
		}
		return ComplexFindKeyword(text, startPosition);
	}

	private PatternScanResult SimpleFind(string text, int startPosition, bool matchCase)
	{
		int num = (matchCase ? text.IndexOf(StringPattern, startPosition) : text.ToLowerInvariant().IndexOf(LowerStringPattern, startPosition));
		PatternScanResult result = default(PatternScanResult);
		if (num >= 0)
		{
			result.Index = num;
			result.Token = text.Substring(num, StringPattern.Length);
		}
		else
		{
			result.Index = 0;
			result.Token = "";
		}
		return result;
	}

	private PatternScanResult SimpleFindKeyword(string text, int startPosition, bool matchCase)
	{
		PatternScanResult result;
		do
		{
			result = SimpleFind(text, startPosition, matchCase);
			if (result.Token == "")
			{
				return result;
			}
			if (CharIsSeparator(text, result.Index - 1) && CharIsSeparator(text, result.Index + result.Token.Length))
			{
				return result;
			}
			startPosition = result.Index + 1;
		}
		while (startPosition < text.Length);
		result.Token = "";
		result.Index = 0;
		return result;
	}

	private PatternScanResult ComplexFindKeyword(string text, int startPosition)
	{
		PatternScanResult result;
		do
		{
			result = ComplexFind(text, startPosition);
			if (result.Token == "")
			{
				return result;
			}
			if (CharIsSeparator(text, result.Index - 1) && CharIsSeparator(text, result.Index + result.Token.Length))
			{
				return result;
			}
			startPosition = result.Index + 1;
		}
		while (startPosition < text.Length);
		result.Token = "";
		result.Index = 0;
		return result;
	}

	private PatternScanResult ComplexFind(string text, int startPosition)
	{
		MatchCollection matchCollection = rx.Matches(text);
		PatternScanResult result = default(PatternScanResult);
		foreach (Match item in matchCollection)
		{
			int index = item.Index;
			string value = item.Value;
			if (index >= startPosition)
			{
				result.Index = index;
				result.Token = value;
				return result;
			}
		}
		PatternScanResult result2 = default(PatternScanResult);
		result2.Index = 0;
		result2.Token = "";
		return result2;
	}

	public Pattern(string pattern, bool iscomplex)
	{
		StringPattern = pattern;
		if (iscomplex)
		{
			IsComplex = true;
			rx = new Regex(StringPattern, RegexOptions.Compiled);
		}
		else
		{
			IsComplex = false;
		}
	}

	public Pattern(string pattern, bool iscomplex, bool separator, bool keyword)
	{
		Init(pattern, iscomplex, separator, keyword);
	}

	public Pattern(string pattern, bool separator, bool keyword, string escapeChar)
	{
		escapeChar = Regex.Escape(escapeChar);
		string pattern2 = string.Format("(?<=((?<!{0})({0}{0})*))({1})", escapeChar, pattern);
		Init(pattern2, isComplex: true, separator, keyword);
	}

	private void Init(string pattern, bool isComplex, bool separator, bool keyword)
	{
		StringPattern = pattern;
		IsSeparator = separator;
		IsKeyword = keyword;
		IsComplex = isComplex;
		if (isComplex)
		{
			rx = new Regex(StringPattern, RegexOptions.Compiled);
		}
	}
}
