using System.Collections.Generic;
using System.Threading;
using Redbox.BrokerServices.Proxy.ComponentModel;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Services.KioskBrokerServices.KioskShared.DomainObjects;
using Redbox.Services.KioskBrokerServices.KioskShared.Enums;

namespace Redbox.BrokerServices.Proxy;

public class ReservationCallbackEntry3 : ICallbackEntry
{
	internal readonly AutoResetEvent ResetEvent = new AutoResetEvent(initialState: false);

	public ReservationResult BrokerResult { get; private set; }

	public ReservationCart3 Cart { get; internal set; }

	public ReservationCallbackEntry3()
	{
		BrokerResult = new ReservationResult();
	}

	public void Invoke()
	{
		Dictionary<string, object> dictionary = Cart.ToJson().ToObject<Dictionary<string, object>>();
		if (dictionary.ContainsKey("Email") && dictionary["Email"] != null)
		{
			dictionary["Email"] = ObfuscateMessageData(dictionary["Email"].ToString(), 'x', 1, 0);
		}
		if (dictionary.ContainsKey("CardID") && dictionary["CardID"] != null)
		{
			dictionary["CardID"] = ObfuscateMessageData(dictionary["CardID"].ToString(), 'x', 3, 3);
		}
		LogHelper.Instance.Log("BrokerServicesProxy received a remote reservation request: {0}", dictionary.ToJson());
		if (ReservationServicesProxy.Instance.RequestCallback3 == null)
		{
			BrokerResult.ErrorMessage = "No reservation request callback registered.";
			BrokerResult.ReservationSucceeded = false;
			ResetEvent.Set();
			LogHelper.Instance.Log("ReservationCallbackEntry3:Invoke() No reservation request callback registered.");
			return;
		}
		Dictionary<string, List<int>> dictionary2 = new Dictionary<string, List<int>>();
		foreach (ReservationTitle3 title in Cart.Titles)
		{
			string key = ((title.ReservationType == ReservationType.Purchase) ? "purchase" : "rental");
			List<int> list = (dictionary2.ContainsKey(key) ? dictionary2[key] : (dictionary2[key] = new List<int>()));
			list.Add(title.TitleID);
		}
		IReservationResult reservationResult = ReservationServicesProxy.Instance.RequestCallback3(Cart.CardID, Cart.Email, Cart.ReferenceNumber, Cart.ReserveDate, dictionary2, Cart.AppliedCredit, Cart.AppliedHiveOnlinePromotion, Cart.AppliedTitleMarketing, Cart.CustomerNumber, Cart.DiscountedSubTotal, Cart.GrandTotal, Cart.HistoryTitleIDs, Cart.HiveOnlinePromotionDiscount, Cart.IsMnp, Cart.NumberOfCreditsRemaining, Cart.PromoCode, Cart.SubTotal, Cart.Tax, Cart.Titles, Cart.IsGc, Cart.GcIds, Cart.CcIds, Cart.TaxRate, Cart.Zip, Cart.IsMdv);
		BrokerResult.ReservationSucceeded = reservationResult.Success;
		BrokerResult.ErrorMessage = reservationResult.ErrorMessage;
		List<TitleResult> list3 = new List<TitleResult>();
		foreach (IReservedItem item in reservationResult.Items)
		{
			foreach (ReservationTitle3 title2 in Cart.Titles)
			{
				if (title2.TitleID == item.ProductId)
				{
					list3.Add(new TitleResult
					{
						ItemID = title2.ItemID,
						Reservable = ((item.Barcode != "empty") ? true : false),
						ReservedBarcode = item.Barcode
					});
					break;
				}
			}
		}
		BrokerResult.TitleResults = list3;
		LogHelper.Instance.Log("BrokerServicesProxy reservation response: {0}", reservationResult.ToJson());
		ResetEvent.Set();
	}

	private string ObfuscateMessageData(string msgText, char newChar, int leading, int trailing)
	{
		if (leading + trailing > msgText.Length)
		{
			leading = 0;
			trailing = 0;
		}
		string obj = ((leading > 0) ? msgText.Substring(0, leading) : string.Empty);
		string text;
		if (trailing <= 0)
		{
			text = string.Empty;
		}
		else
		{
			text = msgText.Substring(msgText.Length - trailing, trailing);
		}
		string text2 = text;
		string text3 = new string(newChar, msgText.Length - (leading + trailing));
		return obj + text3 + text2;
	}
}
