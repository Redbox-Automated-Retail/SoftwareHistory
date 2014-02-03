using System;
using System.IO;
using System.Text;

namespace HALUtilities;

internal class Writer
{
	private string m_outputFile;

	internal Writer(string outputFile)
	{
		if (!string.IsNullOrEmpty(outputFile))
		{
			m_outputFile = outputFile;
			return;
		}
		throw new ArgumentException("File name must be valid.");
	}

	internal void WritePicArray(byte[,] arr)
	{
		using StreamWriter streamWriter = new StreamWriter(m_outputFile);
		StringBuilder stringBuilder = new StringBuilder();
		int num = DeviceConfiguration.Instance.DevicePixelWidth >> 3;
		for (int i = 0; i < DeviceConfiguration.Instance.DeviceHeight; i++)
		{
			for (int j = 0; j < num; j++)
			{
				short num2 = arr[i, j];
				if (num2 == 0)
				{
					stringBuilder.Append("00");
					continue;
				}
				int num3 = num2 & 0xF0;
				int num4 = num2 & 0xF;
				string value = num3.ToString("X");
				string value2 = num4.ToString("X");
				stringBuilder.Append(value);
				stringBuilder.Append(value2);
			}
			streamWriter.WriteLine(stringBuilder.ToString());
			stringBuilder.Remove(0, stringBuilder.Length);
		}
	}

	internal void WriteBooleanArray(bool[,] pixels)
	{
		StringBuilder stringBuilder = new StringBuilder();
		using StreamWriter streamWriter = new StreamWriter(m_outputFile);
		for (int i = 0; i < DeviceConfiguration.Instance.DeviceHeight; i++)
		{
			for (int j = 0; j < DeviceConfiguration.Instance.DevicePixelWidth; j += 4)
			{
				int num = 0;
				if (pixels[i, j])
				{
					num += 8;
				}
				if (pixels[i, j + 1])
				{
					num += 4;
				}
				if (pixels[i, j + 2])
				{
					num += 2;
				}
				if (pixels[i, j + 3])
				{
					num++;
				}
				stringBuilder.Append(num.ToString("X"));
			}
			streamWriter.WriteLine(stringBuilder.ToString());
			stringBuilder.Remove(0, stringBuilder.Length);
		}
	}

	internal void WriteFormattedBinaryArray(byte[] rawBytes)
	{
		using BinaryWriter binaryWriter = new BinaryWriter(File.Open(m_outputFile, FileMode.CreateNew));
		int num = (DeviceConfiguration.Instance.DevicePixelWidth >> 3) - 1;
		int num2 = 0;
		int num3 = 0;
		while (num2 < rawBytes.Length)
		{
			binaryWriter.Write(rawBytes[num2]);
			if (num3 == num)
			{
				num3 = 0;
				binaryWriter.Write(Environment.NewLine);
			}
			num2++;
			num3++;
		}
		binaryWriter.Write(Environment.NewLine);
	}

	internal void WriteRawBytes(byte[] rawBytes)
	{
		using BinaryWriter binaryWriter = new BinaryWriter(File.Open(m_outputFile, FileMode.CreateNew));
		for (int i = 0; i < rawBytes.Length; i++)
		{
			binaryWriter.Write(rawBytes[i]);
		}
	}

	internal void WriteRawBytes2(byte[] rawBytes)
	{
		using StreamWriter streamWriter = new StreamWriter(m_outputFile);
		for (int i = 0; i < rawBytes.Length; i++)
		{
			streamWriter.Write(rawBytes[i]);
		}
	}
}
