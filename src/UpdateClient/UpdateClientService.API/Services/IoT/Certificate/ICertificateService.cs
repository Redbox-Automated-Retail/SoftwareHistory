using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace UpdateClientService.API.Services.IoT.Certificate;

public interface ICertificateService
{
	Task<(X509Certificate rootCa, X509Certificate2 deviceCertPfx)> GetCertificatesAsync();

	Task<(X509Certificate rootCa, X509Certificate2 deviceCertPfx)> GenerateNewCertificatesAsync();

	Task DeleteLocalCertsIfNotValidAsync();
}
