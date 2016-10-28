using System;
using System.Runtime.InteropServices;

namespace Belikov.GenuineChannels;

[Serializable]
[ComVisible(true)]
public class OperationErrorMessage
{
	public string ErrorIdentifier;

	public string UserFriendlyMessage;

	public OperationErrorMessage()
	{
	}

	public OperationErrorMessage(string errorIdentifier, string userFriendlyMessage)
	{
		ErrorIdentifier = errorIdentifier;
		UserFriendlyMessage = userFriendlyMessage;
	}
}
