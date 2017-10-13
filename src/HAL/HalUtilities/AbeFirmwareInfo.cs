using System;
using System.IO;

namespace HALUtilities;

internal sealed class AbeFirmwareInfo : IEquatable<AbeFirmwareInfo>
{
	internal string Kiosk { get; private set; }

	internal bool UpdatedFirmware { get; private set; }

	public bool Equals(AbeFirmwareInfo other)
	{
		return Kiosk.Equals(other.Kiosk);
	}

	internal AbeFirmwareInfo(string file)
	{
		string fileName = Path.GetFileName(file);
		int num = fileName.IndexOf('.');
		if (-1 != num)
		{
			Kiosk = fileName.Substring(7, num - 7);
		}
		else
		{
			Kiosk = "UNKNOWN";
		}
		using StreamReader streamReader = new StreamReader(file);
		while (true)
		{
			string text = streamReader.ReadLine();
			if (text == null)
			{
				break;
			}
			if (text.Equals("Firmware version V6.0    successfully loaded"))
			{
				UpdatedFirmware = true;
			}
		}
	}
}
