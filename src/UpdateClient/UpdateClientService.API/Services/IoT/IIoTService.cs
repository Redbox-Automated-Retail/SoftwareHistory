using System.Threading.Tasks;
using UpdateClientService.API.Services.IoT.UpdateServiceRequests;

namespace UpdateClientService.API.Services.IoT;

public interface IIoTService
{
	Task<string> UpdateServiceRequest(UpdateServiceRequest request);
}
