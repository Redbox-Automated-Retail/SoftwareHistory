using System.Drawing;
using System.IO;
using Redbox.HAL.Core;

namespace HALUtilities;

internal class KristelFileCompressor
{
	internal static KristelFileCompressor Instance => Singleton<KristelFileCompressor>.Instance;

	internal bool MakeBin(string inputDirectory, string outputFile)
	{
		string[] files = Directory.GetFiles(inputDirectory, "*.bmp");
		return WriteBinFile(outputFile, files);
	}

	private ushort ConvertTo565Color(Color p)
	{
		ushort num = (ushort)((p.B >> 3) & 0x1F);
		ushort num2 = (ushort)((p.G >> 2 << 5) & 0x7E0);
		ushort num3 = (ushort)((p.R >> 3 << 11) & 0xF800);
		return (ushort)(num3 | num2 | num);
	}

	private void WriteCompressedBitmapToFile(Bitmap b, BinaryWriter file)
	{
		file.Write((ushort)b.Width);
		file.Write((ushort)b.Height);
		file.Write((ushort)0);
		file.Write((ushort)16);
		file.Write((uint)(3359637504u + file.BaseStream.Position + 12));
		file.Write(0u);
		file.Write(0u);
		int num = b.Height * b.Width;
		ushort[] array = new ushort[num];
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		for (num3 = 0; num3 < b.Height; num3++)
		{
			for (num2 = 0; num2 < b.Width; num2++)
			{
				array[num4] = ConvertTo565Color(b.GetPixel(num2, num3));
				num4++;
			}
		}
		int num5 = 0;
		num4 = 0;
		while (num4 < num)
		{
			int i;
			for (i = num4 + 1; i < num; i++)
			{
				num5 = i - num4;
				if (array[i] != array[num4] || num5 >= 254)
				{
					break;
				}
			}
			if (num5 > 1)
			{
				file.Write((byte)(i - num4));
				file.Write(array[num4]);
				num4 = i;
				continue;
			}
			for (i = num4 + 1; i < num; i++)
			{
				num5 = i - num4 - 1;
				if (array[i] == array[i - 1] || num5 > 254)
				{
					break;
				}
			}
			if (i == num)
			{
				num5 = 1;
			}
			file.Write((byte)0);
			file.Write((byte)num5);
			while (num5 > 0)
			{
				file.Write(array[num4]);
				num5--;
				num4++;
			}
		}
	}

	private bool WriteBinFile(string outputFile, string[] fileList)
	{
		if (File.Exists(outputFile))
		{
			FileHelper.SafeDeleteFile(outputFile);
		}
		using (BinaryWriter binaryWriter = new BinaryWriter(File.Open(outputFile, FileMode.OpenOrCreate)))
		{
			binaryWriter.BaseStream.SetLength(0L);
			binaryWriter.Write((ushort)65260);
			binaryWriter.Write((ushort)(fileList.Length + 1));
			binaryWriter.Write(0u);
			binaryWriter.Write(4294967040u);
			binaryWriter.Write(0u);
			binaryWriter.Write(0u);
			long num = binaryWriter.BaseStream.Position + 4;
			for (int i = 0; i < fileList.Length; i++)
			{
				Image image = Image.FromFile(fileList[i]);
				uint value = (uint)(18 + 2 * image.Width * image.Height);
				binaryWriter.Write(4294967042u);
				binaryWriter.Write(1431655765);
				binaryWriter.Write(value);
			}
			for (int j = 0; j < fileList.Length; j++)
			{
				uint num2 = (uint)binaryWriter.BaseStream.Position;
				binaryWriter.BaseStream.Seek(j * 12 + num, SeekOrigin.Begin);
				binaryWriter.Write(num2 + 3359637504u);
				binaryWriter.BaseStream.Seek(num2, SeekOrigin.Begin);
				Bitmap b = (Bitmap)Image.FromFile(fileList[j]);
				WriteCompressedBitmapToFile(b, binaryWriter);
			}
			long position = binaryWriter.BaseStream.Position;
			binaryWriter.BaseStream.Seek(4L, SeekOrigin.Begin);
			binaryWriter.Write((uint)position);
		}
		return true;
	}
}
