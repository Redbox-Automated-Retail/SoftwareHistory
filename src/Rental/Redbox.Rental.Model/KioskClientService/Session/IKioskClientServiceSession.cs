using System.Threading.Tasks;

namespace Redbox.Rental.Model.KioskClientService.Session;

public interface IKioskClientServiceSession
{
	Task<bool> Start(SessionType sessionType);

	Task<bool> End();
}
