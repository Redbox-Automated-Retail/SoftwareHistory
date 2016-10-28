using System.IO;
using System.Runtime.InteropServices;

namespace Belikov.GenuineChannels.Security.SSPI;

[ComVisible(true)]
public class SspiClientSecurityContext : SspiSecurityContext
{
	public KeyProvider_SspiClient KeyProvider_SspiClient;

	public bool ContinueProcessing = true;

	public SspiClientSecurityContext(KeyProvider_SspiClient keyProvider_SspiClient)
	{
		KeyProvider_SspiClient = keyProvider_SspiClient;
		if (KeyProvider_SspiClient.DelegatedContext == null)
		{
			SspiApi.AcquireCredentialsHandle(keyProvider_SspiClient.AuthIdentity, keyProvider_SspiClient.PackageName, 3, _credHandle, ref _ptsExpiry);
		}
		else
		{
			SspiApi.AcquireCredentialsHandle(keyProvider_SspiClient.AuthIdentity, keyProvider_SspiClient.PackageName, 3, _credHandle, ref _ptsExpiry);
		}
	}

	public bool BuildUpSecurityContext(Stream receivedData, Stream outputStream)
	{
		bool flag = SspiApi.InitializeSecurityContext(_credHandle, (receivedData == null) ? null : _phContext, _phContext, KeyProvider_SspiClient.ServerName, KeyProvider_SspiClient.RequiredFeatures, ref _ptsExpiry, receivedData, outputStream);
		ContinueProcessing = !flag;
		return flag;
	}
}
