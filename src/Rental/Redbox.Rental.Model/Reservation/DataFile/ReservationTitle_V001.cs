using System.Runtime.CompilerServices;
using Redbox.Services.KioskBrokerServices.KioskShared.Enums;

namespace Redbox.Rental.Model.Reservation.DataFile;

public class ReservationTitle_V001
{
	[CompilerGenerated]
	private DiscountType _003CDiscountType_003Ek__BackingField;

	public string Barcode { get; set; }

	public long TitleId { get; set; }

	public decimal DiscountedPrice { get; set; }

	public int ItemId { get; set; }

	public decimal Discount { get; set; }

	public ReservationType_V001 ReservationType { get; set; }

	public decimal Price { get; set; }

	public decimal LoyaltyPoints { get; set; }

	public DiscountType DiscountType
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003CDiscountType_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003CDiscountType_003Ek__BackingField = value;
		}
	}
}
