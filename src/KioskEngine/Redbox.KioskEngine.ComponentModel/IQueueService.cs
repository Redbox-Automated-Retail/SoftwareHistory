using System;

namespace Redbox.KioskEngine.ComponentModel;

public interface IQueueService : IDisposable
{
	int QueueReadPeriodMin { get; set; }

	int QueueReadPeriodMax { get; set; }

	event ProcessMessageDelegate ProcessMessage;

	ErrorList Clear();

	ErrorList Peek(out object instance);

	ErrorList GetDepth(out int count);

	ErrorList Enqueue(byte priority, string type, object message);

	ErrorList EnqueueOnlyOne(byte priority, string type, object message);

	ErrorList Update(int id, object message);

	ErrorList Update(int id, string message);

	ErrorList ExportToXml(string fileName);

	ErrorList Initialize(string path);

	ErrorList ForEach(Action<IMessage> action);

	ErrorList ForEach(Action<IMessage> action, string type);

	bool IsWorkerStarted();

	void StopQueueWorker();

	void StartQueueWorker();

	ErrorList DeleteMessage(int id);

	ErrorList DeleteMessageByType(string type);

	ErrorList Dequeue(Predicate<IMessage> predicate);
}
