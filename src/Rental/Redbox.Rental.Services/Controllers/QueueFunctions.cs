using System.Collections;
using System.Collections.Generic;
using System.Text;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.Services.Controllers;

public class QueueFunctions
{
	internal static QueuedPendingCounts GetPendingCounts()
	{
		QueuedPendingCounts result = new QueuedPendingCounts();
		IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
		if (service == null)
		{
			return result;
		}
		ITrackDataService service2 = ServiceLocator.Instance.GetService<ITrackDataService>();
		if (service2 == null || !service2.HasValidData())
		{
			return result;
		}
		string currentCardHashId = service2.GenerateCardHashId();
		service.ForEach(delegate(IMessage m)
		{
			Dictionary<string, object> dictionary = Encoding.ASCII.GetString(m.Data).ToDictionary();
			if (dictionary.ContainsKey("CreditCard"))
			{
				IDictionary<string, object> dictionary2 = (IDictionary<string, object>)dictionary["CreditCard"];
				if ((!dictionary2.ContainsKey("CardId") || string.Compare((string)dictionary2["CardId"], currentCardHashId) == 0) && dictionary.ContainsKey("Contents"))
				{
					IDictionary<string, object> dictionary3 = (IDictionary<string, object>)dictionary["Contents"];
					if (dictionary3.ContainsKey("rental"))
					{
						foreach (IDictionary<string, object> item in (ArrayList)((IDictionary<string, object>)dictionary3["rental"])["Items"])
						{
							if (!item.ContainsKey("VendStatus") || string.Compare((string)item["VendStatus"], "Vended", ignoreCase: true) == 0)
							{
								if (item.ContainsKey("MultiDisc") && string.Compare(item["MultiDisc"].ToString(), "true", ignoreCase: true) == 0 && item.ContainsKey("Prices"))
								{
									IDictionary<string, object> dictionary5 = (IDictionary<string, object>)item["Prices"];
									if (dictionary5.ContainsKey("initialnight") && (string.Compare(dictionary5["initialnight"].ToString(), "0") == 0 || string.Compare(dictionary5["initialnight"].ToString(), "0.0") == 0))
									{
										continue;
									}
								}
								if (item.ContainsKey("ProductFamily") && string.Compare((string)item["ProductFamily"], "Movies", ignoreCase: true) == 0)
								{
									result.MovieRentalCount++;
								}
								if (item.ContainsKey("ProductFamily") && string.Compare((string)item["ProductFamily"], "Games", ignoreCase: true) == 0)
								{
									result.GameRentalCount++;
								}
							}
						}
					}
					if (dictionary3.ContainsKey("purchase"))
					{
						foreach (IDictionary<string, object> item2 in (ArrayList)((IDictionary<string, object>)dictionary3["purchase"])["Items"])
						{
							if (!item2.ContainsKey("VendStatus") || string.Compare((string)item2["VendStatus"], "Vended", ignoreCase: true) == 0)
							{
								if (item2.ContainsKey("ProductFamily") && string.Compare((string)item2["ProductFamily"], "Movies", ignoreCase: true) == 0)
								{
									result.MoviePurchaseCount++;
								}
								if (item2.ContainsKey("ProductFamily") && string.Compare((string)item2["ProductFamily"], "Games", ignoreCase: true) == 0)
								{
									result.GamePurchaseCount++;
								}
							}
						}
					}
				}
			}
		}, "Reconcile");
		return result;
	}
}
