using Redbox.Rental.Model;

namespace Redbox.Rental.Services;

public class PromotionService : IPromotion
{
	private static IPromotion _instance;

	private Promotion _promotion;

	public static IPromotion Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new PromotionService();
			}
			return _instance;
		}
	}

	public Promotion GetPromotion()
	{
		return _promotion;
	}

	public void SetPromotion(Promotion promotion)
	{
		_promotion = promotion;
	}

	public void ClearPromotion()
	{
		_promotion = null;
	}
}
