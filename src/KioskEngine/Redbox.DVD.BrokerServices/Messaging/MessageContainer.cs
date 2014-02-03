using System;
using System.Collections;
using System.Threading;
using Belikov.Common.ThreadProcessing;
using Belikov.GenuineChannels.Logbook;
using Belikov.GenuineChannels.Parameters;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.Messaging;

internal class MessageContainer
{
	internal Queue _queue = new Queue(20);

	private ITransportContext _iTransportContext;

	private int _currentTotalSize = 0;

	private int _currentTotalMessages = 0;

	private int _maxContentSize;

	private int _maxQueuedItems;

	private int _maxTotalSize;

	private bool _noSizeChecking;

	private bool _disposed = false;

	private ReaderWriterLock _disposeLock = new ReaderWriterLock();

	private Exception _disposeException;

	public bool IsMessageAvailable
	{
		get
		{
			lock (_queue)
			{
				return _queue.Count > 0;
			}
		}
	}

	public MessageContainer(ITransportContext iTransportContext)
	{
		_iTransportContext = iTransportContext;
		_maxQueuedItems = (int)iTransportContext.IParameterProvider[GenuineParameter.MaxQueuedItems];
		_maxTotalSize = (int)iTransportContext.IParameterProvider[GenuineParameter.MaxTotalSize];
		_maxContentSize = (int)iTransportContext.IParameterProvider[GenuineParameter.MaxContentSize];
		_noSizeChecking = (bool)iTransportContext.IParameterProvider[GenuineParameter.NoSizeChecking];
	}

	public void UnregisterSyncMessage(Message message)
	{
		lock (_queue)
		{
			_currentTotalMessages--;
			_currentTotalSize -= message.EffectiveMessageSize;
			if (_currentTotalMessages < 0 || _currentTotalSize < 0)
			{
				GenuineLoggingServices.BinaryLogWriter?.WriteImplementationWarningEvent("MessageContainer.UnregisterSyncMessage", LogMessageType.CriticalError, GenuineExceptions.Get_Debugging_GeneralWarning("Implementation bug."), GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, "Incorrect queue state. CurrentTotalMessages = {0}. CurrentTotalSize = {1}.", _currentTotalMessages, _currentTotalSize);
			}
		}
	}

	public void AddMessage(Message message, bool onlyCheckIn)
	{
		BinaryLogWriter binaryLogWriter = GenuineLoggingServices.BinaryLogWriter;
		ReaderAutoLocker readerAutoLocker = new ReaderAutoLocker(_disposeLock);
		try
		{
			if (_disposed)
			{
				throw OperationException.WrapException(_disposeException);
			}
			int num = 0;
			if (message.SecuritySessionParameters != null)
			{
				num = message.SecuritySessionParameters.MaxContentSize;
			}
			if (num == 0)
			{
				num = _maxContentSize;
			}
			if (num > 0 && !_noSizeChecking)
			{
				try
				{
					if (message.SerializedContent != null && message.SerializedContent.CanSeek)
					{
						message.EffectiveMessageSize = (int)message.SerializedContent.Length;
					}
				}
				catch (Exception exception)
				{
					if (binaryLogWriter != null && binaryLogWriter[LogCategory.ImplementationWarning] > 0)
					{
						binaryLogWriter.WriteEvent(LogCategory.ImplementationWarning, "MessageContainer.AddMessage", LogMessageType.Warning, exception, message, message.Recipient, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "Message stream doesn't support the Length property. Set the NoSizeChecking parameter to true to increase performance");
					}
				}
				try
				{
					if (message.SerializedContent != null && message.EffectiveMessageSize == 0 && message.SerializedContent.CanSeek)
					{
						message.EffectiveMessageSize = Math.Min(message.EffectiveMessageSize, (int)message.SerializedContent.Length);
					}
				}
				catch (Exception exception2)
				{
					if (binaryLogWriter != null && binaryLogWriter[LogCategory.ImplementationWarning] > 0)
					{
						binaryLogWriter.WriteEvent(LogCategory.ImplementationWarning, "MessageContainer.AddMessage", LogMessageType.Warning, exception2, message, message.Recipient, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "Message stream doesn't support the Length property. Set the NoSizeChecking parameter to true to increase performance");
					}
				}
				if (message.EffectiveMessageSize > num)
				{
					throw GenuineExceptions.Get_Send_TooLargePacketSize(message.EffectiveMessageSize, num);
				}
			}
			bool flag = false;
			lock (_queue)
			{
				if (_currentTotalMessages + 1 > _maxQueuedItems || message.EffectiveMessageSize + _currentTotalSize > _maxTotalSize)
				{
					flag = true;
				}
				else
				{
					_currentTotalMessages++;
					_currentTotalSize += message.EffectiveMessageSize;
					if (!onlyCheckIn)
					{
						_queue.Enqueue(message);
						if (message.Recipient != null)
						{
							message.Recipient.QueueLength = _queue.Count;
						}
						if (binaryLogWriter != null && binaryLogWriter[LogCategory.MessageProcessing] > 0)
						{
							binaryLogWriter.WriteEvent(LogCategory.MessageProcessing, "MessageContainer.AddMessage", LogMessageType.MessageIsEnqueued, null, message, message.Recipient, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "The message has been enqueued because no connections are available. Messages in the queue: {0}. Queue size: {1}.", _currentTotalMessages, _currentTotalSize);
						}
					}
				}
			}
			if (flag)
			{
				Exception ex = GenuineExceptions.Get_Send_QueueIsOverloaded(_maxQueuedItems, _currentTotalMessages + 1, _maxTotalSize, message.EffectiveMessageSize + _currentTotalSize);
				if (binaryLogWriter != null && binaryLogWriter[LogCategory.MessageProcessing] > 0)
				{
					binaryLogWriter.WriteEvent(LogCategory.MessageProcessing, "MessageContainer.AddMessage", LogMessageType.MessageIsEnqueued, ex, message, message.Recipient, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "Queue is overrun.");
				}
				Dispose(ex);
				throw ex;
			}
		}
		finally
		{
			readerAutoLocker.Dispose();
		}
	}

	public Message GetMessage()
	{
		Message message = null;
		ReaderAutoLocker readerAutoLocker = new ReaderAutoLocker(_disposeLock);
		try
		{
			if (_disposed)
			{
				return null;
			}
			lock (_queue)
			{
				if (_queue.Count > 0)
				{
					message = (Message)_queue.Dequeue();
				}
				if (message != null)
				{
					_currentTotalMessages--;
					_currentTotalSize -= message.EffectiveMessageSize;
					if (message.Recipient != null)
					{
						message.Recipient.QueueLength = _queue.Count;
					}
					BinaryLogWriter binaryLogWriter = _iTransportContext.BinaryLogWriter;
					if (binaryLogWriter != null && binaryLogWriter[LogCategory.MessageProcessing] > 0)
					{
						binaryLogWriter.WriteEvent(LogCategory.MessageProcessing, "MessageContainer.GetMessage", LogMessageType.MessageHasBeenDequeued, null, message, message.Recipient, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "The message has been dequeued. Messages in the queue: {0}. Queue size: {1}.", _currentTotalMessages, _currentTotalSize);
					}
				}
			}
		}
		finally
		{
			readerAutoLocker.Dispose();
		}
		return message;
	}

	public void Dispose(Exception reason)
	{
		if (reason == null)
		{
			reason = GenuineExceptions.Get_Processing_TransportConnectionFailed();
		}
		WriterAutoLocker writerAutoLocker = new WriterAutoLocker(_disposeLock);
		try
		{
			if (_disposed)
			{
				return;
			}
			_disposed = true;
			_disposeException = reason;
		}
		finally
		{
			writerAutoLocker.Dispose();
		}
		while (_queue.Count > 0)
		{
			Message message = (Message)_queue.Dequeue();
			message.SyncWaitException = reason;
			if (message.ReplyToId < 0)
			{
				message.ITransportContext.IIncomingStreamHandler.DispatchException(message, reason);
			}
			if (message.ConnectionAvailable != null)
			{
				message.ConnectionAvailable.Set();
			}
			message.Dispose();
		}
	}
}
