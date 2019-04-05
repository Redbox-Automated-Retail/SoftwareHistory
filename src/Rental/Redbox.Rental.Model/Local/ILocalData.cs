namespace Redbox.Rental.Model.Local;

public interface ILocalData
{
	ILocalDataInstance LoadData();

	void UpdateBadCard(IBadCard badCard);

	void RemoveBadCard(IBadCard badCard);
}
