using System.IO;

namespace Belikov.GenuineChannels.Security.SSPI;

internal class SspiServerSecurityContext : SspiSecurityContext
{
	public KeyProvider_SspiServer KeyProvider_SspiServer;

	public bool ContinueProcessing = true;

	private bool firstCall = true;

	public SspiServerSecurityContext(KeyProvider_SspiServer keyProvider_SspiServer)
	{
		KeyProvider_SspiServer = keyProvider_SspiServer;
		SspiApi.AcquireCredentialsHandle(null, KeyProvider_SspiServer.PackageName, 1, _credHandle, ref _ptsExpiry);
	}

	public bool BuildUpSecurityContext(Stream receivedData, Stream outputStream)
	{
		bool flag = SspiApi.AcceptSecurityContext(_credHandle, _phContext, KeyProvider_SspiServer.RequiredFeatures, ref _ptsExpiry, receivedData, outputStream, firstCall);
		firstCall = false;
		ContinueProcessing = !flag;
		return flag;
	}
}
