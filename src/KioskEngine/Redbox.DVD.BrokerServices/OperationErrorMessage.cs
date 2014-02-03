using System;

namespace Belikov.GenuineChannels;

[Serializable]
internal class OperationErrorMessage
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
