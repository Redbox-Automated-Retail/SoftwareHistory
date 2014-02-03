namespace Belikov.GenuineChannels;

internal interface IGenuineExceptionLocalizer
{
	string Localize(string errorId, params object[] messageParameters);
}
