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

	private class ReadData
	{
		private int Reads;

		private int SecureReads;

		internal int Deck { get; private set; }

		internal int Slot { get; private set; }

		internal bool Read => Reads > 0;

		internal bool SecureFound => SecureReads > 0;

		internal bool IsEmpty { get; set; }

		internal void ReadOk()
		{
			Reads++;
		}

		internal void SecureReadOk()
		{
			SecureReads++;
		}

		internal ReadData(int deck, int slot)
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

	internal void AnalyzeCognex(string file)
	{
		string path = Path.Combine(RuntimeService.Instance.AssemblyDirectory, $"Cognex-{JobID}-analysis.log");
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
		ReadData readData = null;
		List<ReadData> list = new List<ReadData>();
		try
		{
			using StreamWriter streamWriter = new StreamWriter(path);
			streamWriter.WriteLine("Analyzing file '{0}'", file);
			using (TextReader textReader = new StreamReader(file))
			{
				while (true)
				{
					string text = textReader.ReadLine();
					if (text == null)
					{
						break;
					}
					if (text.IndexOf("Sync location") != -1)
					{
						string text2 = "Deck = ";
						string text3 = "Slot = ";
						int num = text.IndexOf(text2);
						if (-1 != num)
						{
							int result = -1;
							string s = text.Substring(num + text2.Length, 1);
							int deck = int.Parse(s);
							num = text.IndexOf(text3);
							string text4 = text.Substring(num + text3.Length).TrimEnd();
							if (!int.TryParse(text4, out result))
							{
								streamWriter.WriteLine("Unable to parse {0}", text4);
								Environment.Exit(1);
							}
							readData = new ReadData(deck, result);
							list.Add(readData);
						}
					}
					else if (text.Contains("***CortexService:"))
					{
						string text5 = "Matrix = ";
						string text6 = "secure code count = ";
						int num2 = text.IndexOf(text5);
						int num3 = text.IndexOf(text6);
						if (-1 == num2 || num3 == -1)
						{
							Console.WriteLine("Invalid line '{0}'", text);
							Environment.Exit(1);
						}
						string text7 = ParseItem(text, num2, text5.Length);
						string text8 = text.Substring(num3 + text6.Length, 1);
						if (!text7.StartsWith("UNKNOWN"))
						{
							readData.ReadOk();
						}
						if ("0" != text8)
						{
							readData.SecureReadOk();
						}
					}
				}
			}
			list.RemoveAll((ReadData each) => each.IsEmpty);
			Console.WriteLine("There are {0} readable disks.", list.Count);
			List<ReadData> list2 = list.FindAll((ReadData each) => each.SecureFound && !each.Read);
			Console.WriteLine(" {0} read only the secure code.", list2.Count);
			foreach (ReadData item in list2)
			{
				Console.WriteLine("   YesOnly: deck = {0} slot = {1}", item.Deck, item.Slot);
			}
			List<ReadData> list3 = list.FindAll((ReadData each) => each.Read && !each.SecureFound);
			Console.WriteLine(" {0} read only the std code.", list3.Count);
			foreach (ReadData item2 in list3)
			{
				Console.WriteLine("   MatrixOnly: deck = {0} slot = {1}", item2.Deck, item2.Slot);
			}
			int count = list.FindAll((ReadData each) => each.Read && each.SecureFound).Count;
			Console.WriteLine(" {0} read both.", count);
			int count2 = list.FindAll((ReadData each) => !each.Read && !each.SecureFound).Count;
			Console.WriteLine(" {0} were no read.", count2);
			foreach (ReadData item3 in list.FindAll((ReadData each) => !each.Read && !each.SecureFound))
			{
				Console.WriteLine("   Noread: deck = {0} slot = {1}", item3.Deck, item3.Slot);
			}
		}
		finally
		{
			list.Clear();
		}
	}

	internal void AnalyzeCortex(string file)
	{
		string path = Path.Combine(RuntimeService.Instance.AssemblyDirectory, $"Cortex-{JobID}-analysis.log");
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
		ReadData readData = null;
		List<ReadData> list = new List<ReadData>();
		try
		{
			using StreamWriter streamWriter = new StreamWriter(path);
			streamWriter.WriteLine("Analyzing file '{0}'", file);
			using (TextReader textReader = new StreamReader(file))
			{
				while (true)
				{
					string text = textReader.ReadLine();
					if (text == null)
					{
						break;
					}
					if (text.IndexOf("Sync location") != -1)
					{
						string text2 = "Deck = ";
						string text3 = "Slot = ";
						int num = text.IndexOf(text2);
						if (-1 != num)
						{
							int result = -1;
							string s = text.Substring(num + text2.Length, 1);
							int deck = int.Parse(s);
							num = text.IndexOf(text3);
							string text4 = text.Substring(num + text3.Length).TrimEnd();
							if (!int.TryParse(text4, out result))
							{
								streamWriter.WriteLine("Unable to parse {0}", text4);
								Environment.Exit(1);
							}
							readData = new ReadData(deck, result);
							list.Add(readData);
						}
					}
					else
					{
						if (text.Contains("ReadDisc"))
						{
							continue;
						}
						if (text.Contains("GET: no disk in picker after pull."))
						{
							readData.IsEmpty = true;
						}
						else
						{
							if (text.Contains("PickerSensors: 1"))
							{
								continue;
							}
							string value = "data (barcode = ";
							int num2 = text.IndexOf(">> Reponse packet");
							if (-1 == num2)
							{
								continue;
							}
							int num3 = text.IndexOf(value);
							if (num3 != -1)
							{
								if (text.Contains("YES") || text.Contains("yes"))
								{
									readData.SecureReadOk();
								}
								else
								{
									readData.ReadOk();
								}
							}
						}
					}
				}
			}
			list.RemoveAll((ReadData each) => each.IsEmpty);
			Console.WriteLine("There are {0} readable disks.", list.Count);
			List<ReadData> list2 = list.FindAll((ReadData each) => each.SecureFound && !each.Read);
			Console.WriteLine(" {0} read only the secure code.", list2.Count);
			foreach (ReadData item in list2)
			{
				Console.WriteLine("   YesOnly: deck = {0} slot = {1}", item.Deck, item.Slot);
			}
			List<ReadData> list3 = list.FindAll((ReadData each) => each.Read && !each.SecureFound);
			Console.WriteLine(" {0} read only the std code.", list3.Count);
			foreach (ReadData item2 in list3)
			{
				Console.WriteLine("   MatrixOnly: deck = {0} slot = {1}", item2.Deck, item2.Slot);
			}
			int count = list.FindAll((ReadData each) => each.Read && each.SecureFound).Count;
			Console.WriteLine(" {0} read both.", count);
			int count2 = list.FindAll((ReadData each) => !each.Read && !each.SecureFound).Count;
			Console.WriteLine(" {0} were no read.", count2);
			foreach (ReadData item3 in list.FindAll((ReadData each) => !each.Read && !each.SecureFound))
			{
				Console.WriteLine("   Noread: deck = {0} slot = {1}", item3.Deck, item3.Slot);
			}
		}
		finally
		{
			list.Clear();
		}
	}

	private string ParseItem(string statement, int startIdx, int tokLength)
	{
		string text = statement.Substring(startIdx + tokLength);
		char[] array = text.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (char.IsWhiteSpace(array[i]))
			{
				return text.Substring(0, i + 1);
			}
		}
		return string.Empty;
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
