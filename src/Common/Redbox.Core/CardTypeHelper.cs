using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Redbox.Core;

public static class CardTypeHelper
{
	private static bool m_isInitialized;

	private static RedboxGiftCard m_rbGiftCard = new RedboxGiftCard();

	public static void Initialize(List<BinRange> rbGiftCardBinRanges)
	{
		if (!m_isInitialized)
		{
			m_rbGiftCard.AddBinRanges(rbGiftCardBinRanges);
			m_isInitialized = true;
		}
	}

	public static CardType? GetCardType(string cardNumber)
	{
		return GetCardType(cardNumber, checkGCRanges: true);
	}

	public static CardType? GetCardType(string cardNumber, bool checkGCRanges)
	{
		if (checkGCRanges && m_rbGiftCard.IsRedboxGiftCard(cardNumber))
		{
			return CardType.RedboxGiftCard;
		}
		if (NumberMatches(cardNumber, "^(4)", 16))
		{
			return CardType.Visa;
		}
		if (NumberMatches(cardNumber, "^(34|37)", 15))
		{
			return CardType.AmericanExpress;
		}
		if (NumberMatches(cardNumber, "^(51|52|53|54|55)", 16))
		{
			return CardType.MasterCard;
		}
		if (NumberMatches(cardNumber, "^(30|38|39|6011|62|64|65)", 16) || NumberMatches(cardNumber, "^(36)", 14))
		{
			return CardType.Discover;
		}
		return null;
	}

	private static bool NumberMatches(string cardNumber, string regex, int length)
	{
		if (Regex.IsMatch(cardNumber, regex))
		{
			return cardNumber.Length == length;
		}
		return false;
	}
}
