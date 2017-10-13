using System;
using System.Collections.Generic;
using Redbox.Services.KioskBrokerServices.KioskShared.DomainObjects;

namespace Redbox.BrokerServices.Proxy.ComponentModel;

public delegate IReservationResult ReservationRequestCallback2(string cardId, string email, long referenceNumber, DateTime reserveDate, IDictionary<string, List<int>> productsToReserve, bool appliedCredit, bool appliedHiveOnlinePromotion, bool appliedTitleMarketing, string customerNumber, decimal discountedSubTotal, decimal grandTotal, List<int> historyTitleIDs, decimal hiveOnlinePromotionDiscount, int numberOfCreditsRemaining, string promoCode, decimal subTotal, decimal tax, List<ReservationTitle2> reservationTitles);
