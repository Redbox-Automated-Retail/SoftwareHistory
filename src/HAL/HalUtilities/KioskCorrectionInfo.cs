using System;
using System.Collections.Generic;
using System.IO;

namespace HALUtilities;

internal sealed class KioskCorrectionInfo : IDisposable
{
	internal DateTime SurveyDate { get; private set; }

	internal string Kiosk { get; private set; }

	internal List<ArcusCorrectionStatistic> Stats { get; private set; }

	internal int FailedCorrections { get; private set; }

	internal int SuccessfulCorrections { get; private set; }

	public void Dispose()
	{
		Stats.Clear();
	}

	internal KioskCorrectionInfo(string file)
	{
		Stats = new List<ArcusCorrectionStatistic>();
		using (StreamReader streamReader = new StreamReader(file))
		{
			int num = 0;
			while (true)
			{
				string text = streamReader.ReadLine();
				if (text == null)
				{
					break;
				}
				if (num++ == 0)
				{
					int num2 = text.IndexOf("Kiosk");
					if (-1 == num2)
					{
						Console.WriteLine("[KioskCorrectionInfo] Unable to parse line '{0}'", text);
						throw new InvalidDataException($"Invalid file data: line - '{text}'");
					}
					string s = text.Substring(0, num2 - 1);
					SurveyDate = DateTime.Parse(s);
					string text2 = text.Substring(num2, text.Length - num2);
					Kiosk = text2.Split(':')[1];
				}
				else
				{
					Stats.Add(new ArcusCorrectionStatistic(text));
				}
			}
		}
		if (Stats.Count > 0)
		{
			FailedCorrections = Stats.FindAll((ArcusCorrectionStatistic each) => !each.CorrectionOk).Count;
			SuccessfulCorrections = Stats.FindAll((ArcusCorrectionStatistic each) => each.CorrectionOk).Count;
		}
	}
}
