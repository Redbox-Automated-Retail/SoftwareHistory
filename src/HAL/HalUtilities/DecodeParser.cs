using System;
using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Core;

namespace HALUtilities;

internal sealed class DecodeParser
{
	private readonly List<DecodeStat> Stats = new List<DecodeStat>();

	private readonly char[] Separator = new char[1] { ',' };

	internal void ParseStats(string file, IConsole console)
	{
		if (!File.Exists(file))
		{
			console.WriteLine("The file '{0}' doesn't exist.");
			return;
		}
		using StreamReader streamReader = new StreamReader(file);
		while (true)
		{
			string text = streamReader.ReadLine();
			if (text != null)
			{
				string[] elements = text.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
				Stats.Add(new DecodeStat(elements));
				continue;
			}
			break;
		}
	}

	internal void DumpScanTimes(IConsole console)
	{
		double totalTime = 0.0;
		Stats.ForEach(delegate(DecodeStat stat)
		{
			totalTime += stat.ScanTime.TotalSeconds;
		});
		console.WriteLine("For sample size {0} elements:", Stats.Count);
		double num = totalTime / (double)Stats.Count;
		console.WriteLine(" Avg decode time: {0}s", num.ToString());
	}
}
