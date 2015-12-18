namespace Redbox.Rental.Model.KioskData;

public interface IKioskData
{
	bool HasStaged();

	IKioskDataInstance GetData();
}
