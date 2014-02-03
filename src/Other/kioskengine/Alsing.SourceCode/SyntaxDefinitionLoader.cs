using System;
using System.Collections;
using System.Drawing;
using System.Xml;

namespace Alsing.SourceCode;

public class SyntaxDefinitionLoader
{
	private Hashtable spanDefinitionLookup = new Hashtable();

	private Hashtable styleLookup = new Hashtable();

	private SyntaxDefinition syntaxDefinition = new SyntaxDefinition();

	public SyntaxDefinition Load(string File)
	{
		styleLookup = new Hashtable();
		spanDefinitionLookup = new Hashtable();
		syntaxDefinition = new SyntaxDefinition();
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(File);
		ReadLanguageDefinition(xmlDocument);
		return syntaxDefinition;
	}

	public SyntaxDefinition Load(string File, string Separators)
	{
		styleLookup = new Hashtable();
		spanDefinitionLookup = new Hashtable();
		syntaxDefinition = new SyntaxDefinition
		{
			Separators = Separators
		};
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(File);
		ReadLanguageDefinition(xmlDocument);
		if (syntaxDefinition.mainSpanDefinition == null)
		{
			throw new Exception("no main block found in syntax");
		}
		return syntaxDefinition;
	}

	public SyntaxDefinition LoadXML(string XML)
	{
		styleLookup = new Hashtable();
		spanDefinitionLookup = new Hashtable();
		syntaxDefinition = new SyntaxDefinition();
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(XML);
		ReadLanguageDefinition(xmlDocument);
		if (syntaxDefinition.mainSpanDefinition == null)
		{
			throw new Exception("no main block found in syntax");
		}
		return syntaxDefinition;
	}

	private void ReadLanguageDefinition(XmlNode xml)
	{
		ParseLanguage(xml["Language"]);
	}

	private void ParseLanguage(XmlNode node)
	{
		string name = "";
		string name2 = "";
		foreach (XmlAttribute attribute in node.Attributes)
		{
			if (attribute.Name.ToLowerInvariant() == "name")
			{
				name = attribute.Value;
			}
			if (attribute.Name.ToLowerInvariant() == "startblock")
			{
				name2 = attribute.Value;
			}
		}
		syntaxDefinition.Name = name;
		syntaxDefinition.mainSpanDefinition = GetBlock(name2);
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.NodeType == XmlNodeType.Element)
			{
				if (childNode.Name.ToLowerInvariant() == "filetypes")
				{
					ParseFileTypes(childNode);
				}
				if (childNode.Name.ToLowerInvariant() == "block")
				{
					ParseBlock(childNode);
				}
				if (childNode.Name.ToLowerInvariant() == "style")
				{
					ParseStyle(childNode);
				}
			}
		}
	}

	private void ParseFileTypes(XmlNode node)
	{
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.NodeType != XmlNodeType.Element || !(childNode.Name.ToLowerInvariant() == "filetype"))
			{
				continue;
			}
			string extension = "";
			string name = "";
			foreach (XmlAttribute attribute in childNode.Attributes)
			{
				if (attribute.Name.ToLowerInvariant() == "name")
				{
					name = attribute.Value;
				}
				if (attribute.Name.ToLowerInvariant() == "extension")
				{
					extension = attribute.Value;
				}
			}
			FileType fileType = new FileType();
			fileType.Extension = extension;
			fileType.Name = name;
			FileType item = fileType;
			syntaxDefinition.FileTypes.Add(item);
		}
	}

	private void ParseBlock(XmlNode node)
	{
		string name = "";
		string name2 = "";
		bool multiLine = false;
		bool terminateChildren = false;
		Color backColor = Color.Transparent;
		foreach (XmlAttribute attribute in node.Attributes)
		{
			if (attribute.Name.ToLowerInvariant() == "name")
			{
				name = attribute.Value;
			}
			if (attribute.Name.ToLowerInvariant() == "style")
			{
				name2 = attribute.Value;
			}
			if (attribute.Name.ToLowerInvariant() == "ismultiline")
			{
				multiLine = bool.Parse(attribute.Value);
			}
			if (attribute.Name.ToLowerInvariant() == "terminatechildren")
			{
				terminateChildren = bool.Parse(attribute.Value);
			}
			if (attribute.Name.ToLowerInvariant() == "backcolor")
			{
				backColor = Color.FromName(attribute.Value);
			}
		}
		SpanDefinition block = GetBlock(name);
		block.BackColor = backColor;
		block.Name = name;
		block.MultiLine = multiLine;
		block.Style = GetStyle(name2);
		block.TerminateChildren = terminateChildren;
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.NodeType == XmlNodeType.Element)
			{
				if (childNode.Name.ToLowerInvariant() == "scope")
				{
					string text = "";
					string pattern = "";
					string name3 = "";
					string expansionText = "";
					string text2 = "";
					string value = "false";
					string value2 = "false";
					string value3 = "false";
					string value4 = "false";
					string text3 = "";
					string text4 = "";
					string text5 = "";
					string value5 = "false";
					bool defaultExpanded = true;
					foreach (XmlAttribute attribute2 in childNode.Attributes)
					{
						switch (attribute2.Name.ToLowerInvariant())
						{
						case "start":
							text = attribute2.Value;
							break;
						case "escapechar":
							text5 = attribute2.Value;
							break;
						case "end":
							pattern = attribute2.Value;
							break;
						case "style":
							name3 = attribute2.Value;
							break;
						case "text":
							expansionText = attribute2.Value;
							break;
						case "defaultexpanded":
							defaultExpanded = bool.Parse(attribute2.Value);
							break;
						case "endisseparator":
							text2 = attribute2.Value;
							break;
						case "startiskeyword":
							value3 = attribute2.Value;
							break;
						case "startiscomplex":
							value = attribute2.Value;
							break;
						case "endiscomplex":
							value2 = attribute2.Value;
							break;
						case "endiskeyword":
							value4 = attribute2.Value;
							break;
						case "spawnblockonstart":
							text3 = attribute2.Value;
							break;
						case "spawnblockonend":
							text4 = attribute2.Value;
							break;
						case "causeindent":
							value5 = attribute2.Value;
							break;
						}
					}
					if (text != "")
					{
						bool iscomplex = bool.Parse(value);
						bool iscomplex2 = bool.Parse(value2);
						bool causeIndent = bool.Parse(value5);
						Scope scope = new Scope();
						scope.Style = GetStyle(name3);
						scope.ExpansionText = expansionText;
						scope.DefaultExpanded = defaultExpanded;
						scope.CauseIndent = causeIndent;
						Scope scope2 = scope;
						Pattern start = new Pattern(text, iscomplex, separator: false, bool.Parse(value3));
						Pattern pattern2 = ((text5 != "") ? new Pattern(pattern, separator: false, bool.Parse(value4), text5) : new Pattern(pattern, iscomplex2, separator: false, bool.Parse(value4)));
						if (text2 != "")
						{
							pattern2.IsSeparator = bool.Parse(text2);
						}
						scope2.Start = start;
						scope2.EndPatterns.Add(pattern2);
						block.ScopePatterns.Add(scope2);
						if (text3 != "")
						{
							scope2.spawnSpanOnStart = GetBlock(text3);
						}
						if (text4 != "")
						{
							scope2.spawnSpanOnEnd = GetBlock(text4);
						}
					}
				}
				if (childNode.Name.ToLowerInvariant() == "bracket")
				{
					string text6 = "";
					string pattern3 = "";
					string name4 = "";
					string value6 = "false";
					string value7 = "false";
					string value8 = "false";
					string value9 = "false";
					string value10 = "true";
					foreach (XmlAttribute attribute3 in childNode.Attributes)
					{
						switch (attribute3.Name.ToLowerInvariant())
						{
						case "start":
							text6 = attribute3.Value;
							break;
						case "end":
							pattern3 = attribute3.Value;
							break;
						case "style":
							name4 = attribute3.Value;
							break;
						case "endisseparator":
							if (attribute3.Name.ToLowerInvariant() == "startisseparator" && attribute3.Name.ToLowerInvariant() == "startiskeyword")
							{
								value8 = attribute3.Value;
							}
							break;
						case "startiscomplex":
							value6 = attribute3.Value;
							break;
						case "endiscomplex":
							value7 = attribute3.Value;
							break;
						case "endiskeyword":
							value9 = attribute3.Value;
							break;
						case "ismultiline":
							value10 = attribute3.Value;
							break;
						}
					}
					if (text6 != "")
					{
						PatternList patternList = new PatternList();
						patternList.Style = GetStyle(name4);
						PatternList patternList2 = patternList;
						bool iscomplex3 = bool.Parse(value6);
						bool iscomplex4 = bool.Parse(value7);
						bool isMultiLineBracket = bool.Parse(value10);
						Pattern pattern4 = new Pattern(text6, iscomplex3, separator: false, bool.Parse(value8));
						Pattern pattern5 = (pattern4.MatchingBracket = new Pattern(pattern3, iscomplex4, separator: false, bool.Parse(value9)));
						pattern5.MatchingBracket = pattern4;
						pattern4.BracketType = BracketType.StartBracket;
						pattern5.BracketType = BracketType.EndBracket;
						pattern4.IsMultiLineBracket = (pattern5.IsMultiLineBracket = isMultiLineBracket);
						patternList2.Add(pattern4);
						patternList2.Add(pattern5);
						block.OperatorsList.Add(patternList2);
					}
				}
			}
			if (childNode.Name.ToLowerInvariant() == "keywords")
			{
				foreach (XmlNode childNode2 in childNode.ChildNodes)
				{
					if (!(childNode2.Name.ToLowerInvariant() == "patterngroup"))
					{
						continue;
					}
					PatternList patternList3 = new PatternList();
					block.KeywordsList.Add(patternList3);
					foreach (XmlAttribute attribute4 in childNode2.Attributes)
					{
						switch (attribute4.Name.ToLowerInvariant())
						{
						case "style":
							patternList3.Style = GetStyle(attribute4.Value);
							break;
						case "name":
							patternList3.Name = attribute4.Value;
							break;
						case "normalizecase":
							patternList3.NormalizeCase = bool.Parse(attribute4.Value);
							break;
						case "casesensitive":
							patternList3.CaseSensitive = bool.Parse(attribute4.Value);
							break;
						}
					}
					foreach (XmlNode childNode3 in childNode2.ChildNodes)
					{
						if (childNode3.Name.ToLowerInvariant() == "pattern")
						{
							bool iscomplex5 = false;
							bool separator = false;
							string category = null;
							string text7 = "";
							if (childNode3.Attributes != null)
							{
								foreach (XmlAttribute attribute5 in childNode3.Attributes)
								{
									switch (attribute5.Name.ToLowerInvariant())
									{
									case "text":
										text7 = attribute5.Value;
										break;
									case "iscomplex":
										iscomplex5 = bool.Parse(attribute5.Value);
										break;
									case "isseparator":
										separator = bool.Parse(attribute5.Value);
										break;
									case "category":
										category = attribute5.Value;
										break;
									}
								}
							}
							if (text7 != "")
							{
								Pattern pattern6 = new Pattern(text7, iscomplex5, separator, keyword: true);
								pattern6.Category = category;
								Pattern pattern7 = pattern6;
								patternList3.Add(pattern7);
							}
						}
						else
						{
							if (!(childNode3.Name.ToLowerInvariant() == "patterns"))
							{
								continue;
							}
							string value11 = childNode3.ChildNodes[0].Value;
							value11 = value11.Replace("\t", " ");
							while (value11.IndexOf("  ") >= 0)
							{
								value11 = value11.Replace("  ", " ");
							}
							string[] array = value11.Split();
							foreach (string text8 in array)
							{
								if (text8 != "")
								{
									patternList3.Add(new Pattern(text8, iscomplex: false, separator: false, keyword: true));
								}
							}
						}
					}
				}
			}
			if (childNode.Name.ToLowerInvariant() == "operators")
			{
				foreach (XmlNode childNode4 in childNode.ChildNodes)
				{
					if (!(childNode4.Name.ToLowerInvariant() == "patterngroup"))
					{
						continue;
					}
					PatternList patternList4 = new PatternList();
					block.OperatorsList.Add(patternList4);
					foreach (XmlAttribute attribute6 in childNode4.Attributes)
					{
						switch (attribute6.Name.ToLowerInvariant())
						{
						case "style":
							patternList4.Style = GetStyle(attribute6.Value);
							break;
						case "name":
							patternList4.Name = attribute6.Value;
							break;
						case "normalizecase":
							patternList4.NormalizeCase = bool.Parse(attribute6.Value);
							break;
						case "casesensitive":
							patternList4.CaseSensitive = bool.Parse(attribute6.Value);
							break;
						}
					}
					foreach (XmlNode childNode5 in childNode4.ChildNodes)
					{
						if (childNode5.Name.ToLowerInvariant() == "pattern")
						{
							bool iscomplex6 = false;
							bool separator2 = false;
							string text9 = "";
							string category2 = null;
							if (childNode5.Attributes != null)
							{
								foreach (XmlAttribute attribute7 in childNode5.Attributes)
								{
									switch (attribute7.Name.ToLowerInvariant())
									{
									case "text":
										text9 = attribute7.Value;
										break;
									case "iscomplex":
										iscomplex6 = bool.Parse(attribute7.Value);
										break;
									case "isseparator":
										separator2 = bool.Parse(attribute7.Value);
										break;
									case "category":
										category2 = attribute7.Value;
										break;
									}
								}
							}
							if (text9 != "")
							{
								Pattern pattern8 = new Pattern(text9, iscomplex6, separator2, keyword: false);
								pattern8.Category = category2;
								Pattern pattern9 = pattern8;
								patternList4.Add(pattern9);
							}
						}
						else
						{
							if (!(childNode5.Name.ToLowerInvariant() == "patterns"))
							{
								continue;
							}
							string value12 = childNode5.ChildNodes[0].Value;
							value12 = value12.Replace("\t", " ");
							while (value12.IndexOf("  ") >= 0)
							{
								value12 = value12.Replace("  ", " ");
							}
							string[] array = value12.Split();
							foreach (string text10 in array)
							{
								if (text10 != "")
								{
									patternList4.Add(new Pattern(text10, iscomplex: false, separator: false, keyword: false));
								}
							}
						}
					}
				}
			}
			if (!(childNode.Name.ToLowerInvariant() == "childblocks"))
			{
				continue;
			}
			foreach (XmlNode childNode6 in childNode.ChildNodes)
			{
				if (!(childNode6.Name.ToLowerInvariant() == "child"))
				{
					continue;
				}
				foreach (XmlAttribute attribute8 in childNode6.Attributes)
				{
					if (attribute8.Name.ToLowerInvariant() == "name")
					{
						block.childSpanDefinitions.Add(GetBlock(attribute8.Value));
					}
				}
			}
		}
	}

	private TextStyle GetStyle(string Name)
	{
		if (styleLookup[Name] == null)
		{
			TextStyle value = new TextStyle();
			styleLookup.Add(Name, value);
		}
		return (TextStyle)styleLookup[Name];
	}

	private SpanDefinition GetBlock(string Name)
	{
		if (spanDefinitionLookup[Name] == null)
		{
			SpanDefinition value = new SpanDefinition(syntaxDefinition);
			spanDefinitionLookup.Add(Name, value);
		}
		return (SpanDefinition)spanDefinitionLookup[Name];
	}

	private void ParseStyle(XmlNode node)
	{
		string name = "";
		string name2 = "";
		string text = "";
		bool bold = false;
		bool italic = false;
		bool underline = false;
		foreach (XmlAttribute attribute in node.Attributes)
		{
			switch (attribute.Name.ToLowerInvariant())
			{
			case "name":
				name = attribute.Value;
				break;
			case "forecolor":
				name2 = attribute.Value;
				break;
			case "backcolor":
				text = attribute.Value;
				break;
			case "bold":
				bold = bool.Parse(attribute.Value);
				break;
			case "italic":
				italic = bool.Parse(attribute.Value);
				break;
			case "underline":
				underline = bool.Parse(attribute.Value);
				break;
			}
		}
		TextStyle style = GetStyle(name);
		if (text != "")
		{
			style.BackColor = Color.FromName(text);
		}
		style.ForeColor = Color.FromName(name2);
		style.Bold = bold;
		style.Italic = italic;
		style.Underline = underline;
		style.Name = name;
	}
}
