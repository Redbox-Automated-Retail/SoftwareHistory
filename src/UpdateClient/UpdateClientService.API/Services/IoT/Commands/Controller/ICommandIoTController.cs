using System.Threading.Tasks;

namespace UpdateClientService.API.Services.IoT.Commands.Controller;

public interface ICommandIoTController
{
	CommandEnum CommandEnum { get; }

	Task Execute(IoTCommandModel ioTCommand);
}
