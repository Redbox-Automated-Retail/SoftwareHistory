using System.Threading.Tasks;

namespace UpdateClientService.API.Services.IoT.Commands;

public interface IIoTCommandService
{
	Task SubscribeToCommandTopics();
}
