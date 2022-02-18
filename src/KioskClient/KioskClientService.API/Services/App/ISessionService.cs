using KioskClientService.API.App;

namespace KioskClientService.API.Services.App;

public interface ISessionService
{
	ApplicationResponse Start(SessionType sessionType);

	ApplicationResponse End();
}
