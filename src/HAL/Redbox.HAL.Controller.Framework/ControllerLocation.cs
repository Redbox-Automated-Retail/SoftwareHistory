namespace Redbox.HAL.Controller.Framework;

public sealed class ControllerLocation
{
	private int? m_deck = null;

	private int? m_slot = null;

	public int Deck
	{
		get
		{
			return m_deck.Value;
		}
		set
		{
			m_deck = value;
		}
	}

	public int Slot
	{
		get
		{
			return m_slot.Value;
		}
		internal set
		{
			m_slot = value;
		}
	}

	public bool IsValid
	{
		get
		{
			if (m_deck.HasValue)
			{
				return m_slot.HasValue;
			}
			return false;
		}
	}

	internal bool IsQlmDeck
	{
		get
		{
			if (!IsValid)
			{
				return false;
			}
			Deck deck = DecksManager.Instance.GetDeck(m_deck.Value);
			return deck.IsQlmDeck;
		}
	}

	internal bool IsWideSlot
	{
		get
		{
			if (!IsValid)
			{
				return false;
			}
			return DecksManager.Instance.GetDeck(m_deck.Value).IsSlotSellThru(m_slot.Value);
		}
	}

	public void Invalidate()
	{
		m_slot = (m_deck = null);
	}
}
