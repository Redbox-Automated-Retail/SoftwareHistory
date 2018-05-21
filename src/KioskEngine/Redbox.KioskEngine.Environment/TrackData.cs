using System;
using System.Security;
using System.Text.RegularExpressions;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class TrackData
{
	public SecureString Track1 { get; private set; }

	public SecureString Track2 { get; private set; }

	public string LastFour { get; private set; }

	public string LastName { get; private set; }

	public CardType? CardType { get; private set; }

	public string FirstName { get; private set; }

	public string ExpiryYear { get; private set; }

	public string ExpiryMonth { get; private set; }

	public SecureString AccountNumber { get; private set; }

	public static ErrorList Parse(SecureString value, out TrackData trackData)
	{
		ErrorList errorList = new ErrorList();
		trackData = new TrackData();
		try
		{
			string[] array = CardHelper.ExtractFromSecure(value).Split(';');
			if (array.Length == 0)
			{
				errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("T008", "No valid tracks in data stream.", "Valid track data is required."));
				return errorList;
			}
			if (array.Length > 1)
			{
				trackData.Track1 = CardHelper.CopyToSecure(array[0]);
				trackData.Track2 = CardHelper.CopyToSecure(array[1]);
				MatchCollection matchCollection = new Regex("[a-z][^0-9][a-z]*", RegexOptions.IgnoreCase).Matches(CardHelper.ExtractFromSecure(trackData.Track1));
				if (matchCollection.Count > 1)
				{
					trackData.FirstName = matchCollection[0].Groups[0].Value;
					trackData.LastName = matchCollection[1].Groups[0].Value;
				}
				else if (matchCollection.Count > 0)
				{
					trackData.FirstName = matchCollection[0].Groups[0].Value;
				}
			}
			else if (array.Length != 0)
			{
				trackData.Track1.Clear();
				trackData.Track2 = CardHelper.CopyToSecure(array[0]);
			}
			string[] array2 = CardHelper.ExtractFromSecure(trackData.Track2).Split('=');
			trackData.AccountNumber = CardHelper.CopyToSecure(array2[0]);
			if (trackData.AccountNumber == null || trackData.AccountNumber.Length < 13 || trackData.AccountNumber.Length > 16)
			{
				trackData.AccountNumber.Clear();
				errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("T001", "Invalid PAN provided in track data stream.", "A valid PAN is required."));
				return errorList;
			}
			if (array2[0].Substring(0, 6) == "601056")
			{
				if (trackData.FirstName.ToUpper() != "REDBOX" && trackData.LastName.ToUpper() != "REDBOX")
				{
					errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("T007", "Invalid Redbox Card name provided in track data.", "Valid Redbox Card name is required."));
					return errorList;
				}
				if (array2[1].Length < 20)
				{
					errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("T006", "Track data will fail First Data account number parsing.", "Valid track data is required."));
					return errorList;
				}
				LogHelper.Instance.Log("Using First Data account number parsing.");
				trackData.AccountNumber = CardHelper.CopyToSecure(array2[1].Substring(12, 1) + array2[1].Substring(14, 2) + array2[0].Substring(6, 9) + array2[1].Substring(16, 4));
				trackData.CardType = Redbox.Core.CardType.RedboxGiftCard;
			}
			if (trackData.CardType != Redbox.Core.CardType.RedboxGiftCard)
			{
				trackData.CardType = CardTypeHelper.GetCardType(CardHelper.ExtractFromSecure(trackData.AccountNumber), checkGCRanges: false);
			}
			if (!trackData.CardType.HasValue)
			{
				errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("T003", "Invalid PAN provided in track data stream; unknown bin range.", "A valid PAN is required."));
				return errorList;
			}
			if (trackData.CardType != Redbox.Core.CardType.RedboxGiftCard && !LuhnHelper.IsCreditCardValid(CardHelper.ExtractFromSecure(trackData.AccountNumber)))
			{
				errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("T002", "PAN failed Luhn validation.", "A valid PAN is required."));
				return errorList;
			}
			trackData.LastFour = CardHelper.ExtractFromSecure(trackData.AccountNumber).Substring(trackData.AccountNumber.Length - 4);
			trackData.ExpiryYear = array2[1].Substring(0, 2);
			trackData.ExpiryMonth = array2[1].Substring(2, 2);
			if (trackData.CardType != Redbox.Core.CardType.RedboxGiftCard)
			{
				if (trackData.ExpiryYear == null || !IsNumeric(trackData.ExpiryYear))
				{
					errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("T004", "Invalid expiry year in track data stream.", "A valid exiry year is required."));
					return errorList;
				}
				if (trackData.ExpiryMonth == null || !IsNumeric(trackData.ExpiryMonth))
				{
					errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("T005", "Invalid expiry month provided in track data stream.", "A valid expiry month is required."));
					return errorList;
				}
				int result = 0;
				int.TryParse(trackData.ExpiryMonth, out result);
				if (result < 1 || result > 12)
				{
					errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("T005", "Invalid expiry month provided in track data stream.", "A valid expiry month is required."));
					return errorList;
				}
			}
			if (trackData.Track2.Length > 1)
			{
				trackData.Track2 = CardHelper.CopyToSecure(CardHelper.ExtractFromSecure(trackData.Track2).Substring(0, CardHelper.ExtractFromSecure(trackData.Track2).IndexOf("?")));
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in TrackData.Parse.", e);
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("T999", "An unhandled exception was raised in TrackData.Parse.", e));
		}
		return errorList;
	}

	private TrackData()
	{
	}

	private static bool IsNumeric(string stringToCheck)
	{
		return new Regex("^[0-9]+$").IsMatch(stringToCheck);
	}
}
