using System;

namespace Belikov.GenuineChannels.Security.ZeroProofAuthorization;

[Flags]
internal enum ZpaFeatureFlags
{
	None = 0,
	ElectronicCodebookEncryption = 1,
	CipherBlockChainingEncryption = 2,
	Mac3DesCbcSigning = 4,
	HmacSha1Signing = 8
}
