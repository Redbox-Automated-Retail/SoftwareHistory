using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public class DecksManager
{
	public static DecksManager Instance => Singleton<DecksManager>.Instance;

	public int DeckCount => Decks.Count;

	internal List<Deck> Decks { get; private set; }

	public Deck First()
	{
		return Decks[0];
	}

	public Deck Last()
	{
		return Decks[Decks.Count - 1];
	}

	public Deck GetDeck(int deck)
	{
		if (deck < 1 || deck > Decks.Count)
		{
			return null;
		}
		return Decks[deck - 1];
	}

	public bool IsValidLocation(Location loc)
	{
		return GetDeck(loc.Deck)?.IsSlotValid(loc.Slot) ?? false;
	}

	public void Add(Deck deck)
	{
		if (deck.IsQlmDeck && QLM.Instance.Deck != null)
		{
			throw new ArgumentException("Only one QLM deck is allowed per configuration.");
		}
		Decks.Add(deck);
	}

	public void ForAllDecksDo(DecksPredicate predicate)
	{
		foreach (Deck deck in Decks)
		{
			if (!predicate(deck))
			{
				break;
			}
		}
	}

	public void ForAllReverseDecksDo(DecksPredicate predicate)
	{
		for (int num = Decks.Count; num >= 1; num--)
		{
			Deck deck = Decks[num - 1];
			if (!predicate(deck))
			{
				break;
			}
		}
	}

	public void Clear()
	{
		Decks.Clear();
	}

	internal void LoadInventory()
	{
		InventoryManager.Instance.Initialize();
		foreach (Deck deck in Decks)
		{
			deck.LoadData();
		}
		LogHelper.Instance.Log("DecksManager: Inventory is initialized.");
	}

	internal void Initialize(XmlNode decksNode)
	{
		Decks.Clear();
		foreach (XmlNode childNode in decksNode.ChildNodes)
		{
			Deck item = Deck.FromXmlNode(childNode);
			Decks.Add(item);
		}
		LogHelper.Instance.Log("DecksManager: kiosk configured for {0} decks.", Decks.Count);
	}

	internal void SaveConfiguration(XmlDocument document)
	{
		using (StringWriter stringWriter = new StringWriter())
		{
			XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter);
			xmlTextWriter.Formatting = Formatting.Indented;
			XmlTextWriter xmlTextWriter2 = xmlTextWriter;
			foreach (Deck deck in Decks)
			{
				deck.ToXmlWriter(xmlTextWriter2);
			}
			xmlTextWriter2.Flush();
			XmlNodeExtensions.SelectSingleNodeAndSetInnerXml(document.DocumentElement, "Controller/Decks", stringWriter);
		}
		LogHelper.Instance.Log("Completed decks save.", LogEntryType.Info);
	}

	internal void ToPropertyXml(XmlWriter writer)
	{
		foreach (Deck deck in Decks)
		{
			deck.ToXmlWriter(writer);
		}
	}

	internal void UpdateFromPropertyXml(XmlNode propertyNode)
	{
		XmlNodeList xmlNodeList = propertyNode.SelectNodes("Deck");
		if (xmlNodeList == null)
		{
			return;
		}
		List<Deck> list = new List<Deck>();
		list.AddRange(Decks);
		Decks.Clear();
		foreach (XmlNode item in xmlNodeList)
		{
			Decks.Add(Deck.FromXmlNode(item));
		}
		Decks.Sort((Deck x, Deck y) => x.Number.CompareTo(y.Number));
		try
		{
			if (Decks.Count != list.Count)
			{
				LogHelper.Instance.Log("The deck configuration has changed in count; this is unsupported.");
				Decks.Clear();
				Decks.AddRange(list);
				return;
			}
			list.Sort((Deck x, Deck y) => x.Number.CompareTo(y.Number));
			for (int num = 0; num < list.Count; num++)
			{
				Decks[num].CopyData(list[num]);
				if (Decks[num].NumberOfSlots == list[num].NumberOfSlots)
				{
					Decks[num].ApplyQuadrantExcludes();
					continue;
				}
				if (list[num].NumberOfSlots < Decks[num].NumberOfSlots)
				{
					Range range = new Range(list[num].NumberOfSlots + 1, Decks[num].NumberOfSlots);
					if (!Decks[num].AddEntries(range))
					{
						LogHelper.Instance.Log($"Unable to add entries {range.Start} -> {range.End} on deck {Decks[num].Number}", LogEntryType.Error);
					}
					else if (list[num].IsQlmDeck && !Decks[num].IsQlmDeck)
					{
						Decks[num].ResetRange(new Range(1, Decks[num].NumberOfSlots));
					}
				}
				else
				{
					Range slots = new Range(Decks[num].NumberOfSlots + 1, list[num].NumberOfSlots);
					Decks[num].RemoveEntries(slots);
				}
				Decks[num].ApplyQuadrantExcludes();
			}
			LogHelper.Instance.Log("Update decks info:");
			for (int num2 = 0; num2 < list.Count; num2++)
			{
				LogHelper.Instance.Log("  Deck Number {0} changes", Decks[num2].Number);
				LogHelper.Instance.Log("    Y-offset: new = {0} old = {1}", Decks[num2].YOffset, list[num2].YOffset);
				if (Decks[num2].Quadrants.Length == list[num2].Quadrants.Length)
				{
					for (int num3 = 0; num3 < Decks[num2].Quadrants.Length; num3++)
					{
						LogHelper.Instance.Log("     Quadrant {0} offsets: new = {1} old = {2}", num3 + 1, Decks[num2].Quadrants[num3].Offset, list[num2].Quadrants[num3].Offset);
					}
					continue;
				}
				LogHelper.Instance.Log("    Quadrant counts do not match: print <NONE> for non-existent quadrants in config.");
				int num4 = Math.Max(Decks[num2].Quadrants.Length, list[num2].Quadrants.Length);
				for (int num5 = 0; num5 < num4; num5++)
				{
					string text = ((num5 < Decks[num2].Quadrants.Length) ? Decks[num2].Quadrants[num5].Offset.ToString() : "<NONE>");
					string text2 = ((num5 < list[num2].Quadrants.Length) ? list[num2].Quadrants[num5].Offset.ToString() : "<NONE>");
					LogHelper.Instance.Log("     Quadrant {0} offsets: new = {1} old = {2}", num5 + 1, text, text2);
				}
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("There was an exception during deck update.", e);
		}
		finally
		{
			list.Clear();
		}
	}

	private DecksManager()
	{
		Decks = new List<Deck>();
	}

	private void ConfigureEmulatedDecks()
	{
		Quadrant quadrant = new Quadrant(3446);
		quadrant.IsExcluded = true;
		Decks.Add(new Deck(1, -18760, isQlmDeck: false, null, 90, 166.6667m, null, null, new Quadrant[6]
		{
			new Quadrant(780),
			quadrant,
			new Quadrant(6113),
			new Quadrant(8779),
			new Quadrant(11446),
			new Quadrant(14112)
		}));
		Decks.Add(new Deck(2, -63120, isQlmDeck: false, null, 90, 166.6667m, null, null, new Quadrant[6]
		{
			new Quadrant(784),
			new Quadrant(3450),
			new Quadrant(6117),
			new Quadrant(8783),
			new Quadrant(11450),
			new Quadrant(14116)
		}));
		Decks.Add(new Deck(3, -107600, isQlmDeck: false, null, 90, 166.6667m, null, null, new Quadrant[6]
		{
			new Quadrant(793),
			new Quadrant(3459),
			new Quadrant(6126),
			new Quadrant(8792),
			new Quadrant(11459),
			new Quadrant(14125)
		}));
		Decks.Add(new Deck(4, -151800, isQlmDeck: false, null, 90, 166.6667m, null, null, new Quadrant[6]
		{
			new Quadrant(805),
			new Quadrant(3471),
			new Quadrant(6138),
			new Quadrant(8804),
			new Quadrant(11471),
			new Quadrant(14137)
		}));
		Decks.Add(new Deck(5, -195940, isQlmDeck: false, null, 90, 166.6667m, null, null, new Quadrant[6]
		{
			new Quadrant(805),
			new Quadrant(3471),
			new Quadrant(6150),
			new Quadrant(8816),
			new Quadrant(11483),
			new Quadrant(14149)
		}));
		Decks.Add(new Deck(6, -240400, isQlmDeck: false, null, 90, 166.6667m, null, null, new Quadrant[6]
		{
			new Quadrant(805),
			new Quadrant(3471),
			new Quadrant(6153),
			new Quadrant(8819),
			new Quadrant(11486),
			new Quadrant(14152)
		}));
		Decks.Add(new Deck(7, -284880, isQlmDeck: false, null, 90, 166.6667m, null, null, new Quadrant[6]
		{
			new Quadrant(805),
			new Quadrant(3471),
			new Quadrant(6154),
			new Quadrant(8820),
			new Quadrant(11487),
			new Quadrant(14153)
		}));
		Decks.Add(new Deck(8, -377140, isQlmDeck: true, 500, 80, 177.7m, null, null, new Quadrant[4]
		{
			new Quadrant(554, new Range(1, 19)),
			new Quadrant(4199, new Range(20, 41)),
			new Quadrant(8199, new Range(42, 63)),
			new Quadrant(12199, new Range(64, 80))
		}));
	}
}
