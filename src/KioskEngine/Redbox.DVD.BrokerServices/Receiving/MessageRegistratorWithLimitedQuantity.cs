namespace Belikov.GenuineChannels.Receiving;

internal class MessageRegistratorWithLimitedQuantity : IMessageRegistrator
{
	private int[] _registeredMessages;

	private int _head = 0;

	public MessageRegistratorWithLimitedQuantity(int capacity)
	{
		_registeredMessages = new int[capacity];
	}

	public bool WasRegistered(string uri, int messageId, int replyId)
	{
		lock (_registeredMessages)
		{
			for (int i = 0; i < _registeredMessages.Length; i++)
			{
				if (_registeredMessages[i] == messageId)
				{
					return true;
				}
			}
			_registeredMessages[_head++] = messageId;
			_head %= _registeredMessages.Length;
		}
		return false;
	}
}
