namespace Belikov.GenuineChannels.Security.ZeroProofAuthorization;

internal interface IAuthorizationManager
{
	string GetPassword(object providedLogin);
}
