using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HALUtilities;

internal class ConversionController
{
	private ConverterType m_type = ConverterType.Redbox_V2;

	internal string OutputFileExtension
	{
		get
		{
			if (m_type == ConverterType.Redbox_V2)
			{
				return ".bin";
			}
			return ".grf";
		}
	}

	internal bool CheckBits(short[] bits, byte b)
	{
		for (int num = bits.Length - 1; num >= 0; num--)
		{
			if (bits[num] != 0)
			{
				if ((b & (1 << num)) == 0)
				{
					return false;
				}
			}
			else if ((b & (1 << num)) != 0)
			{
				return false;
			}
		}
		return true;
	}

	internal string[] GetFileData(string fileName)
	{
		if (fileName.EndsWith(".grf"))
		{
			return ReadStringFile(fileName);
		}
		if (fileName.EndsWith(".bin"))
		{
			return ReadBinaryFile(fileName);
		}
		return new string[0];
	}

	internal string[] ReadBinaryFile(string fileName)
	{
		if (string.IsNullOrEmpty(fileName))
		{
			return new string[0];
		}
		int num = DeviceConfiguration.Instance.DevicePixelWidth >> 3;
		List<string> list = new List<string>();
		StringBuilder stringBuilder = new StringBuilder();
		using (BinaryReader binaryReader = new BinaryReader(File.Open(fileName, FileMode.Open)))
		{
			long length = binaryReader.BaseStream.Length;
			int num2 = 0;
			int num3 = 0;
			while (num2 < length)
			{
				if (num3 == num)
				{
					num3 = 0;
					if (stringBuilder.Length != 60)
					{
						Console.WriteLine("Error");
					}
					list.Add(stringBuilder.ToString());
					stringBuilder.Remove(0, stringBuilder.Length);
				}
				byte number = binaryReader.ReadByte();
				string value = Utils.ConvertNumber(number);
				stringBuilder.Append(value);
				num2++;
				num3++;
			}
			if (stringBuilder.Length != 60)
			{
				Console.WriteLine("Error");
			}
			list.Add(stringBuilder.ToString());
		}
		return list.ToArray();
	}

	private string[] ReadStringFile(string fileName)
	{
		if (string.IsNullOrEmpty(fileName))
		{
			return new string[0];
		}
		List<string> list = new List<string>();
		using (StreamReader streamReader = new StreamReader(fileName))
		{
			string text = "";
			while (text != null)
			{
				text = streamReader.ReadLine();
				if (text != null)
				{
					list.Add(text.Trim());
				}
			}
		}
		return list.ToArray();
	}

	internal bool CompareFiles(string goodFile, string file2)
	{
		string[] fileData = GetFileData(goodFile);
		string[] fileData2 = GetFileData(file2);
		if (fileData.Length != fileData2.Length)
		{
			Console.WriteLine("Error comparing files: lengths not equal.");
			return false;
		}
		for (int i = 0; i < fileData.Length; i++)
		{
			if (!fileData[i].Equals(fileData2[i]))
			{
				Console.WriteLine("Line {0} of comparsion returned false.", i);
			}
		}
		Console.WriteLine("Test succeeded.");
		return true;
	}

	internal int ConvertAll(string directory)
	{
		string[] files = Directory.GetFiles(directory, "*.bmp");
		string[] array = files;
		foreach (string text in array)
		{
			Converter converter = new Converter(m_type);
			string inputFile = Path.Combine(directory, text);
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text);
			fileNameWithoutExtension += OutputFileExtension;
			string outputFile = Path.Combine(directory, fileNameWithoutExtension);
			if (!converter.ConvertBMPToGRF(inputFile, outputFile))
			{
				return 1;
			}
		}
		return 0;
	}

	internal int TestConversion()
	{
		string directoryName = Path.GetDirectoryName(typeof(Program).Assembly.Location);
		string fullPath = Path.GetFullPath($"{directoryName}\\..\\..\\test");
		string text = Path.Combine(fullPath, "bitmaps");
		string text2 = Path.Combine(fullPath, "grfs");
		string[] files = Directory.GetFiles(text);
		string[] files2 = Directory.GetFiles(text2);
		if (files.Length != files2.Length)
		{
			Console.WriteLine("bitmaps {0} don't match grafs {1}", files.Length, files2.Length);
			return 1;
		}
		string[] array = files;
		foreach (string text3 in array)
		{
			string inputfile = Path.Combine(text, text3);
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text3);
			_ = fileNameWithoutExtension + OutputFileExtension;
			string text4 = Path.Combine(directoryName, fileNameWithoutExtension + OutputFileExtension);
			ReadAndWrite(inputfile, text4);
			string text5 = Path.Combine(text2, fileNameWithoutExtension + ".grf");
			if (!CompareFiles(text5, text4))
			{
				Console.WriteLine("File {0} doesn't compare to test file {1}", text4, text5);
				return 2;
			}
		}
		Console.WriteLine("Test ok.");
		return 0;
	}

	internal void ReadAndWrite(string inputfile, string outputfile)
	{
		try
		{
			Converter converter = new Converter(m_type);
			converter.ConvertBMPToGRF(inputfile, outputfile);
		}
		catch (Exception ex)
		{
			Console.WriteLine("Caught unhandled exception during conversion: {0}", ex.Message);
			Console.WriteLine(ex.StackTrace);
		}
	}

	internal int TestBitConverters()
	{
		short[] array = new short[8];
		for (short num = 0; num < 256; num++)
		{
			byte b = Utils.ToBinary(num);
			Utils.Dec2BinLeiwe(num, array);
			if (!CheckBits(array, b))
			{
				Console.WriteLine("Failure to check bits(1)");
				return 1;
			}
		}
		Console.WriteLine("Check bits ok.");
		return 0;
	}

	internal void Execute(string[] args)
	{
		string text = string.Empty;
		string text2 = string.Empty;
		for (int i = 0; i < args.Length; i++)
		{
			int result;
			if ("-o".Equals(args[i], StringComparison.CurrentCultureIgnoreCase))
			{
				text2 = args[++i];
			}
			else if ("-version".Equals(args[i], StringComparison.CurrentCultureIgnoreCase))
			{
				string value = args[++i];
				ConverterType type = (ConverterType)Enum.Parse(typeof(ConverterType), value, ignoreCase: true);
				m_type = type;
			}
			else if ("-i".Equals(args[i], StringComparison.CurrentCultureIgnoreCase))
			{
				text = args[++i];
			}
			else if ("-convertall".Equals(args[i], StringComparison.CurrentCultureIgnoreCase))
			{
				string directory = args[++i];
				int num = ConvertAll(directory);
				if (num != 0)
				{
					Console.WriteLine("Command failed; exiting with code {0}", num);
				}
				Environment.Exit(num);
			}
			else if ("-test".Equals(args[i], StringComparison.CurrentCultureIgnoreCase))
			{
				int num2 = TestConversion();
				if (num2 != 0)
				{
					Console.WriteLine("Command failed; exiting with code {0}", num2);
				}
				Environment.Exit(num2);
			}
			else if ("-testbitconverter".Equals(args[i], StringComparison.CurrentCultureIgnoreCase))
			{
				int num3 = TestBitConverters();
				if (num3 != 0)
				{
					Console.WriteLine("Command failed; exiting with code {0}", num3);
				}
				Environment.Exit(num3);
			}
			else if ("-ppl".Equals(args[i], StringComparison.CurrentCultureIgnoreCase))
			{
				string s = args[++i];
				if (int.TryParse(s, out result))
				{
					DeviceConfiguration.Instance.DevicePixelWidth = result;
				}
			}
			else if ("-height".Equals(args[i], StringComparison.CurrentCultureIgnoreCase))
			{
				string s2 = args[++i];
				if (int.TryParse(s2, out result))
				{
					DeviceConfiguration.Instance.DeviceHeight = result;
				}
			}
		}
		if (string.IsNullOrEmpty(text))
		{
			Console.WriteLine("Please specify an input file.");
			Environment.Exit(1);
		}
		if (string.IsNullOrEmpty(text2))
		{
			string directoryName = Path.GetDirectoryName(text);
			text2 = Path.GetFileNameWithoutExtension(text);
			text2 = $"{text2}{OutputFileExtension}";
			text2 = Path.Combine(directoryName, text2);
		}
		ReadAndWrite(text, text2);
	}
}
