using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace UpdateClientService.API.Services.IoT.Commands;

public class IotCommandDispatch : IIotCommandDispatch
{
	private readonly IServiceScopeFactory _serviceScopeFactory;

	private readonly ILogger<IoTCommandService> _logger;

	public IotCommandDispatch(IServiceScopeFactory serviceScopeFactory, ILogger<IoTCommandService> logger)
	{
		_serviceScopeFactory = serviceScopeFactory;
		_logger = logger;
	}

	public async Task Execute(byte[] message, string topic)
	{
		_logger.LogInfoWithSource("Execute(" + topic, "Execute");
		using IServiceScope scope = _serviceScopeFactory.CreateScope();
		IIoTCommandService service = scope.ServiceProvider.GetService<IIoTCommandService>();
		await service.Execute(message, topic);
	}
}
