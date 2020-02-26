using System.Threading.Tasks;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.App;

public interface ISessionService
{
	Task<ApplicationResponse> Start(SessionType sessionType);

	Task<ApplicationResponse> End();
}
