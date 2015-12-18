using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public class SpecialLocation
{
	public bool IsSecureLocation { get; protected set; }

	public string Name { get; private set; }

	public int? Deck { get; private set; }

	public Range Slots { get; private set; }

	public int? Offset { get; protected set; }

	public virtual string EncryptedPassword => string.Empty;

	public SpecialLocation(string name, int deck, Range slotRange)
	{
		Name = name;
		Deck = deck;
		Slots = slotRange;
	}

	public SpecialLocation(string name, int offset)
	{
		Name = name;
		Offset = offset;
	}

	public virtual Location MoveableLocation()
	{
		if (IsSecureLocation || !Deck.HasValue)
		{
			return null;
		}
		return new Location(Deck.Value, Slots.Start);
	}

	internal virtual bool ExcludeFromESP(Deck deck, int slot)
	{
		return false;
	}
}
