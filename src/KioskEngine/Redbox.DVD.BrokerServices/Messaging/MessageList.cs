namespace Belikov.GenuineChannels.Messaging;

internal class MessageList
{
	private Message[] _messages;

	private int _head = 0;

	public MessageList(int size)
	{
		_messages = new Message[size];
	}

	public void PutMessage(Message message)
	{
		lock (this)
		{
			_messages[_head]?.Dispose();
			_messages[_head++] = message;
			if (_head >= _messages.Length)
			{
				_head = 0;
			}
		}
	}

	public void MoveMessages(MessageContainer messageContainer)
	{
		lock (this)
		{
			for (int i = 0; i < _messages.Length; i++)
			{
				Message message = _messages[i];
				if (message != null && !message.HasBeenAsnwered)
				{
					message.SerializedContent.Position = 0L;
					messageContainer.AddMessage(message, onlyCheckIn: false);
					_messages[i] = null;
				}
			}
		}
	}

	public void ReleaseAllMessages()
	{
		lock (this)
		{
			for (int i = 0; i < _messages.Length; i++)
			{
				Message message = _messages[i];
				if (message != null)
				{
					message.Dispose();
					_messages[i] = null;
				}
			}
		}
	}
}
