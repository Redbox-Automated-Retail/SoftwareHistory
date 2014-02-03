namespace HALUtilities;

internal static class Utils
{
	internal static string ConvertNumber(int number)
	{
		string text = number.ToString("X");
		if (number < 16)
		{
			text = $"0{text}";
		}
		return text;
	}

	internal static void Dec2BinLeiwe(short decData, short[] data)
	{
		if (decData > 127)
		{
			data[0] = 1;
			decData -= 128;
		}
		else
		{
			data[0] = 0;
		}
		if (decData > 63)
		{
			data[1] = 1;
			decData -= 64;
		}
		else
		{
			data[1] = 0;
		}
		if (decData > 31)
		{
			data[2] = 1;
			decData -= 32;
		}
		else
		{
			data[2] = 0;
		}
		if (decData > 15)
		{
			data[3] = 1;
			decData -= 16;
		}
		else
		{
			data[3] = 0;
		}
		if (decData > 7)
		{
			data[4] = 1;
			decData -= 8;
		}
		else
		{
			data[4] = 0;
		}
		if (decData > 3)
		{
			data[5] = 1;
			decData -= 4;
		}
		else
		{
			data[5] = 0;
		}
		if (decData > 1)
		{
			data[6] = 1;
			decData -= 2;
		}
		else
		{
			data[6] = 0;
		}
		if (decData == 1)
		{
			data[7] = 1;
		}
		else
		{
			data[7] = 0;
		}
	}

	internal static byte ToBinary(short b)
	{
		short num = 0;
		int num2 = 7;
		int num3 = 0;
		while (num2 >= 0)
		{
			if ((b & (1 << num2)) != 0)
			{
				num |= (short)(1 << num3);
			}
			num2--;
			num3++;
		}
		return (byte)num;
	}
}
