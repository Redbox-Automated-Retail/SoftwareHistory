namespace Redbox.HAL.Controller.Framework;

public class Location
{
	private int m_deck;

	private int m_slot;

	private string m_stringRep;

	public int Deck => m_deck;

	public int Slot => m_slot;

	public string Matrix => _Deck.GetIdForSlot(m_slot);

	public bool IsEmpty => _Deck.GetIdForSlot(m_slot) == "EMPTY";

	public bool IsUnknown => _Deck.GetIdForSlot(m_slot) == "UNKNOWN";

	public MerchFlags Flags => _Deck.GetMerchFlags(m_slot);

	public bool IsStuck => _Deck.GetEmptyStuckCount(m_slot) > 0;

	public bool IsInVMZ
	{
		get
		{
			if (MerchandizingMode.VMZ_Gen2 != ControllerConfiguration.Instance.MerchandizingMode)
			{
				return false;
			}
			return VMZ.Instance.LocIsInVMZ(this);
		}
	}

	public bool IsWide => _Deck.IsSlotSellThru(m_slot);

	public bool IsExcluded => _Deck.IsExcluded(m_slot);

	private Deck _Deck => DecksManager.Instance.GetDeck(m_deck);

	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}
		Location location = (Location)obj;
		if (location.Deck == Deck)
		{
			return location.Slot == Slot;
		}
		return false;
	}

	public override string ToString()
	{
		if (!string.IsNullOrEmpty(m_stringRep))
		{
			return m_stringRep;
		}
		return m_stringRep = $"Deck = {Deck} Slot = {Slot}";
	}

	public override int GetHashCode()
	{
		return 101 * m_deck + m_slot;
	}

	public void ChangeMerchFlags(MerchFlags flags)
	{
		_Deck.UpdateMerchFlag(flags, m_slot);
	}

	public bool IncrementEmptyStuck()
	{
		return _Deck.UpdateEmptyStuck(m_slot);
	}

	public bool ResetEmptyStuck()
	{
		return _Deck.ResetEmptyStuckCount(m_slot);
	}

	public Location(int deck, int slot)
	{
		m_deck = deck;
		m_slot = slot;
	}
}
