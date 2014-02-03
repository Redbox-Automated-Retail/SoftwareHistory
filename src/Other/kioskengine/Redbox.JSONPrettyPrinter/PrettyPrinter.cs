using System.IO;
using System.Text;
using Jayrock.Json;

namespace Redbox.JSONPrettyPrinter;

public static class PrettyPrinter
{
	public static void Print(Stream i, Stream o)
	{
		using StreamReader reader = new StreamReader(i);
		using StreamWriter streamWriter = new StreamWriter(o);
		using JsonTextReader reader2 = new JsonTextReader(reader);
		using JsonTextWriter jsonTextWriter = new JsonTextWriter(streamWriter);
		jsonTextWriter.PrettyPrint = true;
		JsonColorWriter jsonColorWriter = new JsonColorWriter(jsonTextWriter, new JsonPalette());
		jsonColorWriter.WriteFromReader(reader2);
		streamWriter.WriteLine();
	}

	public static void Print(TextReader i, Stream o)
	{
		using StreamWriter streamWriter = new StreamWriter(o);
		using JsonTextReader reader = new JsonTextReader(i);
		using JsonTextWriter jsonTextWriter = new JsonTextWriter(streamWriter);
		jsonTextWriter.PrettyPrint = true;
		JsonColorWriter jsonColorWriter = new JsonColorWriter(jsonTextWriter, new JsonPalette());
		jsonColorWriter.WriteFromReader(reader);
		streamWriter.WriteLine();
	}

	public static void Print(TextReader i, TextWriter o)
	{
		using JsonTextReader reader = new JsonTextReader(i);
		using JsonTextWriter jsonTextWriter = new JsonTextWriter(o);
		jsonTextWriter.PrettyPrint = true;
		JsonColorWriter jsonColorWriter = new JsonColorWriter(jsonTextWriter, new JsonPalette());
		jsonColorWriter.WriteFromReader(reader);
		o.WriteLine();
	}

	public static string GetPrettyString(string input)
	{
		StringBuilder stringBuilder = new StringBuilder();
		using (JsonTextReader reader = new JsonTextReader(new StringReader(input)))
		{
			using JsonTextWriter jsonTextWriter = new JsonTextWriter(new StringWriter(stringBuilder));
			jsonTextWriter.PrettyPrint = true;
			jsonTextWriter.WriteFromReader(reader);
		}
		return stringBuilder.ToString();
	}
}
