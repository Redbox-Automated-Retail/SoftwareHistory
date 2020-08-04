using System.Threading.Tasks;
using UpdateClientService.API.Services.IoT.Certificate;

namespace UpdateClientService.API.Services.IoT.Commands.Controller;

public class RenewIoTCertificates : ICommandIoTController
{
	private readonly ICertificateService _certificateInstaller;

	public CommandEnum CommandEnum => CommandEnum.RenewIoTCertificates;

	public RenewIoTCertificates(ICertificateService certificateInstaller)
	{
		_certificateInstaller = certificateInstaller;
	}

	public async Task Execute(IoTCommandModel ioTCommand)
	{
		await _certificateInstaller.GenerateNewCertificatesAsync();
	}
}
