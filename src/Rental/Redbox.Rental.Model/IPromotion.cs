namespace Redbox.Rental.Model;

public interface IPromotion
{
	Promotion GetPromotion();

	void SetPromotion(Promotion promotion);

	void ClearPromotion();
}
