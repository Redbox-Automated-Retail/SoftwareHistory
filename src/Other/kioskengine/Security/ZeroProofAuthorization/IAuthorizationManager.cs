using System.Runtime.InteropServices;

namespace Belikov.GenuineChannels.Security.ZeroProofAuthorization;

[ComVisible(true)]
public interface IAuthorizationManager
{
	string GetPassword(object providedLogin);
}
