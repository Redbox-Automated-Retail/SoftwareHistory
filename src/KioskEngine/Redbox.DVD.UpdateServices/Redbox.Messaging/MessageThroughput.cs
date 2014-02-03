using System;

namespace Redbox.Messaging;

internal class MessageThroughput
{
	public double PerMinute => (double)Messages / Duration.TotalMinutes;

	public double PerSecond => (double)Messages / Duration.TotalSeconds;

	public double PerMilliSecond => (double)Messages / Duration.TotalMilliseconds;

	public long Messages { get; set; }

	public TimeSpan Duration { get; set; }

	public MessageThroughput()
	{
	}

	public MessageThroughput(long messages, TimeSpan duration)
	{
		Messages = messages;
		Duration = duration;
	}

	public static Redbox.Messaging.MessageThroughput operator +(Redbox.Messaging.MessageThroughput t1, Redbox.Messaging.MessageThroughput t2)
	{
		return new Redbox.Messaging.MessageThroughput(t1.Messages + t2.Messages, t1.Duration + t2.Duration);
	}

	public static bool operator >(Redbox.Messaging.MessageThroughput t1, Redbox.Messaging.MessageThroughput t2)
	{
		return t1.PerSecond > t2.PerSecond;
	}

	public static bool operator <(Redbox.Messaging.MessageThroughput t1, Redbox.Messaging.MessageThroughput t2)
	{
		return t1.PerSecond < t2.PerSecond;
	}
}
