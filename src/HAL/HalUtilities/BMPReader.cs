using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace HALUtilities;

internal class BMPReader
{
	private string m_inputFile;

	internal BMPReader(string inputFile)
	{
		m_inputFile = inputFile;
	}

	internal byte[] ReadBitmapData()
	{
		byte[] array = ReadBitmapDataInternal(out var stride, out var height);
		int num = DeviceConfiguration.Instance.DevicePixelWidth >> 3;
		int num2 = Math.Min(stride, num);
		int num3 = Math.Min(height, DeviceConfiguration.Instance.DeviceHeight);
		byte[] array2 = new byte[DeviceConfiguration.Instance.DeviceHeight * num];
		for (int i = 0; i < num3; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				byte b = array[j + i * stride];
				if (b > 0)
				{
					array2[j + i * num] = b;
				}
			}
		}
		return array2;
	}

	internal bool[,] ReadAndConvertToBooleanArray()
	{
		try
		{
			int stride = 0;
			int height = 0;
			byte[] array = ReadBitmapDataInternal(out stride, out height);
			bool[,] array2 = null;
			array2 = new bool[DeviceConfiguration.Instance.DeviceHeight, DeviceConfiguration.Instance.DevicePixelWidth];
			int num = Math.Min(height, DeviceConfiguration.Instance.DeviceHeight);
			int num2 = Math.Min(stride, DeviceConfiguration.Instance.DevicePixelWidth >> 3);
			int num3 = 0;
			int num4 = 0;
			for (int i = 0; i < num; i++)
			{
				int num5 = i * stride;
				for (int j = 0; j < num2; j++)
				{
					byte b = array[j + num5];
					if (b > 0)
					{
						num3++;
					}
					for (int num6 = 7; num6 >= 0; num6--)
					{
						array2[i, num4++] = (((b & (1 << num6)) != 0) ? true : false);
					}
				}
				num4 = 0;
			}
			return array2;
		}
		catch (Exception arg)
		{
			Console.WriteLine("Caught an exception during conversion to bool array: {0}", arg);
			return null;
		}
	}

	internal bool[,] ReadAsBytesLeiwe()
	{
		int num = 0;
		short[] array = new short[8];
		bool[,] array2 = null;
		using FileStream input = File.OpenRead(m_inputFile);
		using BinaryReader binaryReader = new BinaryReader(input);
		binaryReader.ReadInt16();
		int num2 = binaryReader.ReadInt32();
		binaryReader.ReadInt32();
		int num3 = binaryReader.ReadInt32();
		binaryReader.ReadInt32();
		binaryReader.ReadInt32();
		int num4 = binaryReader.ReadInt32();
		binaryReader.ReadInt16();
		binaryReader.ReadInt16();
		binaryReader.ReadInt32();
		binaryReader.ReadInt32();
		binaryReader.ReadInt32();
		binaryReader.ReadInt32();
		binaryReader.ReadInt32();
		binaryReader.ReadInt32();
		binaryReader.BaseStream.Seek(num3, SeekOrigin.Begin);
		int num5 = num2 / num4;
		int num6 = 0;
		array2 = new bool[DeviceConfiguration.Instance.DeviceHeight, num5 * 8];
		for (int num7 = num4 - 1; num7 >= 0; num7--)
		{
			num6 = 0;
			for (int i = 0; i < num5; i++)
			{
				byte b = binaryReader.ReadByte();
				if (b > 0)
				{
					num++;
				}
				Utils.Dec2BinLeiwe(b, array);
				for (int j = 0; j < array.Length; j++)
				{
					array2[num7, num6++] = array[j] == 1;
				}
			}
		}
		return array2;
	}

	private byte[] ReadBitmapDataInternal(out int stride, out int height)
	{
		byte[] array = null;
		using Bitmap bitmap = new Bitmap(m_inputFile);
		Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
		BitmapData bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);
		stride = bitmapData.Stride;
		height = bitmapData.Height;
		if (bitmapData.PixelFormat != PixelFormat.Format1bppIndexed)
		{
			throw new Exception("Bitmap is invalid format: not 1 bit/pixel (2-color).");
		}
		IntPtr scan = bitmapData.Scan0;
		int num = bitmapData.Stride * bitmap.Height;
		array = new byte[num];
		Marshal.Copy(scan, array, 0, num);
		bitmap.UnlockBits(bitmapData);
		return array;
	}
}
