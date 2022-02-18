using System.Threading.Tasks;

namespace UpdateClientService.API.Services.IoT.Certificate.Security;

public interface IEncryptionService
{
	Task<string> Encrypt(string plainText);

	Task<string> Decrypt(string ciphertext);
}
