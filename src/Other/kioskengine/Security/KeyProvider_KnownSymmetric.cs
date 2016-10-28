using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.Security;

[ComVisible(true)]
public class KeyProvider_KnownSymmetric : IKeyProvider
{
	public SymmetricAlgorithm SymmetricAlgorithm;

	public KeyProvider_KnownSymmetric(SymmetricAlgorithm symmetricAlgorithm)
	{
		SymmetricAlgorithm = symmetricAlgorithm;
	}

	public SecuritySession CreateSecuritySession(string name, HostInformation remote)
	{
		return new SecuritySession_KnownSymmetric(SymmetricAlgorithm, name);
	}

	public override string ToString()
	{
		return $"KeyProvider_KnownSymmetric. Symmetric algorithm: {SymmetricAlgorithm.GetType().Name}. Key size: {SymmetricAlgorithm.KeySize}";
	}
}
