using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public sealed class DecksManager
{
	private readonly List<Deck> Decks = new List<Deck>();

	public static DecksManager Instance => Singleton<DecksManager>.Instance;

	public int DeckCount => Decks.Count;

	internal int Count => Decks.Count;

	public Deck First()
	{
		return Decks[0];
	}

	public Deck Last()
	{
		return Decks[Decks.Count - 1];
	}

	public Deck GetDeckNumber(int number)
	{
		return Decks.Find((Deck each) => each.Number == number);
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
		Deck deck = GetDeck(loc.Deck);
		if (!(deck == null))
		{
			return deck.IsSlotValid(loc.Slot);
		}
		return false;
	}

	public void Add(Deck deck)
	{
		if (deck.IsQlmDeck && QLM.Deck != null)
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
		Decks.Sort((Deck x, Deck y) => x.Number.CompareTo(y.Number));
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
		using (new DisposeableList<Deck>(list))
		{
			if (xmlNodeList.Count != Decks.Count)
			{
				LogHelper.Instance.Log("The deck configuration has changed in count; this is unsupported.");
				return;
			}
			foreach (XmlNode item in xmlNodeList)
			{
				Deck deck = Deck.FromXmlNode(item);
				Deck deckNumber = GetDeckNumber(deck.Number);
				if (deck != deckNumber)
				{
					list.Add(deck);
				}
			}
			if (list.Count == 0)
			{
				LogHelper.Instance.Log("Update decks: there were no changes.");
				return;
			}
			try
			{
				LogHelper.Instance.Log("Update decks info:");
				foreach (Deck item2 in list)
				{
					Deck deckNumber2 = GetDeckNumber(item2.Number);
					LogHelper.Instance.Log("  Deck Number {0} changes", deckNumber2.Number);
					LogHelper.Instance.Log("    Y-offset: new = {0} old = {1}", item2.YOffset, deckNumber2.YOffset);
					if (item2.Quadrants.Length == deckNumber2.Quadrants.Length)
					{
						for (int i = 0; i < item2.Quadrants.Length; i++)
						{
							LogHelper.Instance.Log("     Quadrant {0} offsets: new = {1} old = {2}", i + 1, item2.Quadrants[i].Offset, deckNumber2.Quadrants[i].Offset);
						}
					}
					else
					{
						LogHelper.Instance.Log("    Quadrant counts do not match: print <NONE> for non-existent quadrants in config.");
						int num = Math.Max(item2.Quadrants.Length, deckNumber2.Quadrants.Length);
						for (int j = 0; j < num; j++)
						{
							string text = ((j < item2.Quadrants.Length) ? item2.Quadrants[j].Offset.ToString() : "<NONE>");
							string text2 = ((j < deckNumber2.Quadrants.Length) ? deckNumber2.Quadrants[j].Offset.ToString() : "<NONE>");
							LogHelper.Instance.Log("     Quadrant {0} offsets: new = {1} old = {2}", j + 1, text, text2);
						}
					}
					deckNumber2.Copy(item2);
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("There was an exception during deck update.", e);
			}
		}
	}

	private DecksManager()
	{
	}
}
