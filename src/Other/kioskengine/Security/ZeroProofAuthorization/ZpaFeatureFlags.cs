using System;
using System.Runtime.InteropServices;

namespace Belikov.GenuineChannels.Security.ZeroProofAuthorization;

[Flags]
[ComVisible(true)]
public enum ZpaFeatureFlags
{
	None = 0,
	ElectronicCodebookEncryption = 1,
	CipherBlockChainingEncryption = 2,
	Mac3DesCbcSigning = 4,
	HmacSha1Signing = 8
}
