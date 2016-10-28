using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Alsing.SourceCode;

[ComVisible(true)]
public class SyntaxDefinitionList
{
	private readonly List<SyntaxDefinition> languages;

	public SyntaxDefinitionList()
	{
		languages = new List<SyntaxDefinition>();
		string[] files = Directory.GetFiles(".", "*.syn");
		string[] array = files;
		foreach (string file in array)
		{
			SyntaxDefinitionLoader syntaxDefinitionLoader = new SyntaxDefinitionLoader();
			SyntaxDefinition item = syntaxDefinitionLoader.Load(file);
			languages.Add(item);
		}
	}

	public SyntaxDefinition GetLanguageFromFile(string path)
	{
		string extension = Path.GetExtension(path);
		foreach (SyntaxDefinition language in languages)
		{
			foreach (FileType fileType in language.FileTypes)
			{
				if (extension.ToLowerInvariant() == fileType.Extension.ToLowerInvariant())
				{
					return language;
				}
			}
		}
		return null;
	}

	public List<SyntaxDefinition> GetSyntaxDefinitions()
	{
		return languages;
	}
}
