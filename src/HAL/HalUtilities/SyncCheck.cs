using System;
using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Core;

namespace HALUtilities;

internal class SyncCheck
{
	private class SyncLocation
	{
		internal int Deck { get; private set; }

		internal int Slot { get; private set; }

		internal string GetMatrix { get; set; }

		internal string PutMatrix { get; set; }

		internal SyncLocation(int deck, int slot)
		{
			Deck = deck;
			Slot = slot;
		}
	}

	private List<SyncLocation> Locations { get; set; }

	private string JobID { get; set; }

	internal SyncCheck(string jobId)
	{
		Locations = new List<SyncLocation>();
		JobID = jobId;
	}

	internal void Summarize()
	{
		string path = "c:\\Program Files\\Redbox\\KioskLogs\\Sync";
		string text = Path.Combine(path, $"sync-{JobID}.log");
		if (File.Exists(text))
		{
			ParseFile(text);
			return;
		}
		text = Path.Combine(path, $"sync-locations-{JobID}.log");
		if (File.Exists(text))
		{
			ParseFile(text);
		}
	}

	private void ParseFile(string file)
	{
		string text = "Deck=";
		string text2 = "Slot=";
		string text3 = "ID=";
		string path = Path.Combine(RuntimeService.Instance.AssemblyDirectory, $"Sync-{JobID}-analysis.log");
		if (File.Exists(path))
		{
			try
			{
				FileHelper.CreateBackup(path, BackupAction.Move);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Unable to backup analysis file: {0}", ex.Message);
			}
		}
		int num = 0;
		using StreamWriter streamWriter = new StreamWriter(path);
		streamWriter.WriteLine("Analyzing file '{0}'", file);
		using (TextReader textReader = new StreamReader(file))
		{
			while (true)
			{
				string text4 = textReader.ReadLine();
				if (text4 == null)
				{
					break;
				}
				if (text4.IndexOf("SLOTEMPTY") != -1)
				{
					num++;
					continue;
				}
				bool flag = text4.IndexOf("PUT") != -1;
				bool flag2 = text4.IndexOf("GET") != -1;
				if (!flag && !flag2)
				{
					Console.WriteLine("ignoring line {0}", text4);
					continue;
				}
				if (-1 == text4.IndexOf(text3))
				{
					Console.WriteLine("ignoring line '{0}'", text4);
					continue;
				}
				int num2 = text4.IndexOf(text);
				if (-1 == num2)
				{
					continue;
				}
				int slot = -1;
				string s = text4.Substring(num2 + text.Length, 1);
				int deck = int.Parse(s);
				num2 = text4.IndexOf(text2);
				string text5 = text4.Substring(num2 + text2.Length);
				char[] array = text5.ToCharArray();
				for (int i = 0; i < array.Length; i++)
				{
					if (char.IsWhiteSpace(array[i]))
					{
						string text6 = text5.Substring(0, i + 1);
						if (!int.TryParse(text6, out slot))
						{
							streamWriter.WriteLine("Unable to parse {0}", text6);
							Environment.Exit(1);
						}
						break;
					}
				}
				string text7 = null;
				num2 = text4.IndexOf(text3);
				text7 = text4.Substring(num2 + text3.Length);
				SyncLocation syncLocation = Locations.Find((SyncLocation each) => each.Deck == deck && each.Slot == slot);
				if (syncLocation == null)
				{
					syncLocation = new SyncLocation(deck, slot);
					Locations.Add(syncLocation);
				}
				if (flag2)
				{
					syncLocation.GetMatrix = text7;
				}
				else
				{
					syncLocation.PutMatrix = text7;
				}
			}
		}
		streamWriter.WriteLine("There are {0} empty slots.", num);
		streamWriter.WriteLine("There are {0} locations in the sync.", Locations.Count);
		foreach (SyncLocation location in Locations)
		{
			if (location.PutMatrix != location.GetMatrix)
			{
				streamWriter.WriteLine("The location {0}, {1} went from {2} -> {3}", location.Deck, location.Slot, location.GetMatrix, location.PutMatrix);
			}
		}
	}
}
