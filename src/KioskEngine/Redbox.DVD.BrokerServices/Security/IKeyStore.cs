namespace Belikov.GenuineChannels.Security;

internal interface IKeyStore
{
	IKeyProvider GetKey(string name);

	void SetKey(string name, IKeyProvider iKeyProvider);
}
