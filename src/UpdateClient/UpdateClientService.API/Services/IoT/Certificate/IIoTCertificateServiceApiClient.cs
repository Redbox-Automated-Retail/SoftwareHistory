using System.Threading.Tasks;

namespace UpdateClientService.API.Services.IoT.Certificate;

public interface IIoTCertificateServiceApiClient
{
	Task<bool> IsCertificateValid(string certificateId);

	Task<IoTCertificateServiceResponse> GetCertificates();

	Task<IoTCertificateServiceResponse> GenerateNewCertificates();
}
