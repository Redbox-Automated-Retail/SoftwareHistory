using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

internal class ESPHelper
{
	private ReadOnlyCollection<ESPNode> m_pattern;

	private readonly object Lock = new object();

	internal ReadOnlyCollection<ESPNode> EmptyPattern => m_pattern;

	internal void Dump()
	{
		DumpInner();
	}

	internal void Initialize()
	{
		LogHelper.Instance.Log("ESPHelper.Initialize()", LogEntryType.Debug);
		lock (Lock)
		{
			ComputeList();
		}
	}

	internal ESPHelper()
	{
	}

	private void AddLocation(Deck deck, int slot, List<ESPNode> tempList, ref int idx)
	{
		lock (Lock)
		{
			if (deck.IsExcluded(slot))
			{
				LogHelper.Instance.Log(LogEntryType.Debug, "Location Deck = {0} Slot = {1} is already excluded.", deck.Number, slot);
			}
			else if (deck.IsSparse && deck.Number == 7 && slot % 6 == 0)
			{
				LogHelper.Instance.Log(" Apply exclude policy to Deck = {0} Slot = {1}", deck.Number, slot);
				deck.ExcludeSlot(slot, excluded: true);
			}
			else if (SpecialLocationLocatorService.Instance.ExcludeSlot(deck, slot))
			{
				LogHelper.Instance.Log(" Apply exclude policy to Deck = {0} Slot = {1}", deck.Number, slot);
			}
			else
			{
				tempList.Add(new ESPNode(deck.Number, slot, idx));
			}
		}
	}

	private void ComputeList()
	{
		List<ESPNode> list = new List<ESPNode>();
		using (new DisposeableList<ESPNode>(list))
		{
			int[] array = new int[5] { 3, 2, 4, 1, 5 };
			int idx = 0;
			Deck deck = null;
			if (!ControllerConfiguration.Instance.IsVMZMachine)
			{
				for (int i = 0; i < 6; i++)
				{
					int[] array2 = array;
					foreach (int deck2 in array2)
					{
						deck = DecksManager.Instance.GetDeck(deck2);
						int num = (deck.IsSparse ? 12 : 15);
						int num2 = i * num + 1;
						for (int k = num2; k <= num2 + num - 1; k++)
						{
							AddLocation(deck, k, list, ref idx);
						}
					}
				}
				deck = DecksManager.Instance.GetDeck(6);
				for (int l = 1; l <= deck.NumberOfSlots; l++)
				{
					AddLocation(deck, l, list, ref idx);
				}
				deck = DecksManager.Instance.GetDeck(7);
				for (int m = 1; m <= deck.NumberOfSlots; m++)
				{
					AddLocation(deck, m, list, ref idx);
				}
				deck = DecksManager.Instance.GetDeck(8);
				if (!deck.IsQlmDeck)
				{
					for (int n = 1; n <= deck.NumberOfSlots; n++)
					{
						AddLocation(deck, n, list, ref idx);
					}
				}
			}
			else
			{
				for (int num3 = 1; num3 < 6; num3++)
				{
					int[] array3 = array;
					foreach (int deck3 in array3)
					{
						deck = DecksManager.Instance.GetDeck(deck3);
						int num5 = num3 * deck.SlotsPerQuadrant + 1;
						for (int num6 = num5; num6 <= num5 + deck.SlotsPerQuadrant - 1; num6++)
						{
							AddLocation(deck, num6, list, ref idx);
						}
					}
				}
				deck = DecksManager.Instance.GetDeck(6);
				for (int num7 = 16; num7 <= deck.NumberOfSlots; num7++)
				{
					AddLocation(deck, num7, list, ref idx);
				}
				deck = DecksManager.Instance.GetDeck(7);
				for (int num8 = 16; num8 <= deck.NumberOfSlots; num8++)
				{
					AddLocation(deck, num8, list, ref idx);
				}
				deck = DecksManager.Instance.GetDeck(8);
				for (int num9 = 16; num9 <= deck.NumberOfSlots; num9++)
				{
					AddLocation(deck, num9, list, ref idx);
				}
				for (int num10 = 1; num10 <= 8; num10++)
				{
					deck = DecksManager.Instance.GetDeck(num10);
					for (int num11 = 15; num11 >= 1; num11--)
					{
						AddLocation(deck, num11, list, ref idx);
					}
				}
			}
			int count = 0;
			DecksManager.Instance.ForAllDecksDo(delegate(Deck _deck)
			{
				count += _deck.GetExcludedCount();
				return true;
			});
			m_pattern = Array.AsReadOnly(list.ToArray());
			LogHelper.Instance.Log(" ESP statistics:");
			LogHelper.Instance.Log("  Total entries: {0}", m_pattern.Count);
			LogHelper.Instance.Log("  There are {0} total excluded slots.", count);
		}
	}

	private void DumpInner()
	{
		DumpNodeList("*** Dump of ESP list ***");
	}

	private void DumpNodeList(string header)
	{
		string path = Path.Combine(ControllerConfiguration.Instance.KioskLogsPath, "Service");
		string path2 = Path.Combine(path, "ESP.log");
		using StreamWriter streamWriter = new StreamWriter(File.Open(path2, FileMode.Append, FileAccess.Write, FileShare.Read));
		string value = $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}: {header}";
		streamWriter.WriteLine(value);
		foreach (ESPNode item in m_pattern)
		{
			streamWriter.WriteLine("Deck = {0} Slot = {1} Excluded = {2}", item.Deck, item.Slot, DecksManager.Instance.GetDeck(item.Deck).IsExcluded(item.Slot).ToString());
		}
		streamWriter.WriteLine();
	}
}
