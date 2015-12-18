using System;
using System.IO;

namespace HALUtilities;

public class Converter
{
	private readonly ConverterType m_converterType;

	public Converter(ConverterType ctype)
	{
		m_converterType = ctype;
	}

	public bool ConvertBMPToGRF(string inputFile, string outputFile)
	{
		if (m_converterType == ConverterType.None)
		{
			return false;
		}
		if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
		{
			Console.WriteLine("Input file must be valid.");
			return false;
		}
		if (string.IsNullOrEmpty(outputFile))
		{
			Console.WriteLine("Output file name must be valid.");
			return false;
		}
		try
		{
			BMPReader bMPReader = new BMPReader(inputFile);
			Writer writer = new Writer(outputFile);
			if (m_converterType == ConverterType.FME)
			{
				bool[,] pixels = bMPReader.ReadAsBytesLeiwe();
				writer.WriteBooleanArray(pixels);
			}
			else if (m_converterType == ConverterType.Redbox_V1)
			{
				bool[,] pixels2 = bMPReader.ReadAndConvertToBooleanArray();
				writer.WriteBooleanArray(pixels2);
			}
			else if (m_converterType == ConverterType.Redbox_V2)
			{
				byte[] rawBytes = bMPReader.ReadBitmapData();
				writer.WriteRawBytes(rawBytes);
			}
			return File.Exists(outputFile);
		}
		catch (Exception arg)
		{
			Console.WriteLine("Conversion failed: {0}", arg);
			return false;
		}
	}
}
