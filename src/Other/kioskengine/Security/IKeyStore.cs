using System.Runtime.InteropServices;

namespace Belikov.GenuineChannels.Security;

[ComVisible(true)]
public interface IKeyStore
{
	IKeyProvider GetKey(string name);

	void SetKey(string name, IKeyProvider iKeyProvider);
}
