using System.Collections.Generic;
using System.IO;

namespace HALUtilities;

internal class CameraErrorFinder
{
	internal void FindErrors(List<InventoryHelper.Location> inventory, TextWriter outputFile)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		string fullPath = Path.GetFullPath(Program.ServicePath + "\\..\\logs");
		string[] files = Directory.GetFiles(fullPath);
		string[] array = files;
		foreach (string path in array)
		{
			using StreamReader streamReader = new StreamReader(File.Open(path, FileMode.Open, FileAccess.Read));
			while (true)
			{
				string text = streamReader.ReadLine();
				if (text != null)
				{
					if (text.Contains("CAMERA CAPTURE instruction errored"))
					{
						num3++;
					}
					else if (text.Contains("Snapshot returned false"))
					{
						num4++;
					}
					continue;
				}
				break;
			}
		}
		foreach (InventoryHelper.Location item in inventory)
		{
			if (item.ID.Equals("UNKNOWN"))
			{
				num++;
			}
			else if (item.ID.Contains("DUPLICATE"))
			{
				num2++;
			}
		}
		outputFile.WriteLine($"There were {num3} instance of CAMERA CAPTURE error.");
		outputFile.WriteLine($"There were {num4} instance of snapshot failure messages.");
		outputFile.WriteLine($"There were {num2} duplicate entries.");
		outputFile.WriteLine($"There were {num} UNKNOWN discs.");
	}
}
