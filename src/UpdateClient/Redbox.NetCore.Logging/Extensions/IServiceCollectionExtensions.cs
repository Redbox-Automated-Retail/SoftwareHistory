using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Abstractions;

namespace Redbox.NetCore.Logging.Extensions;

public static class IServiceCollectionExtensions
{
	private class TestableLogger<T> : ITestableLogger<T>, ILogger<T>, ILogger, ITestableLogger
	{
		private readonly ILogger<T> _logger;

		public TestableLogger(ILogger<T> logger)
		{
			_logger = logger;
		}

		public void LogCritical(string message, params object[] args)
		{
			_logger.LogCritical(message, args);
		}

		public void LogCritical(Exception exception, string message, params object[] args)
		{
			_logger.LogCritical(exception, message, args);
		}

		public void LogCritical(EventId eventId, string message, params object[] args)
		{
			_logger.LogCritical(eventId, message, args);
		}

		public void LogCritical(EventId eventId, Exception exception, string message, params object[] args)
		{
			_logger.LogCritical(eventId, exception, message, args);
		}

		public void LogDebug(EventId eventId, Exception exception, string message, params object[] args)
		{
			_logger.LogDebug(eventId, exception, message, args);
		}

		public void LogDebug(EventId eventId, string message, params object[] args)
		{
			_logger.LogDebug(eventId, message, args);
		}

		public void LogDebug(Exception exception, string message, params object[] args)
		{
			_logger.LogDebug(exception, message, args);
		}

		public void LogDebug(string message, params object[] args)
		{
			_logger.LogDebug(message, args);
		}

		public void LogError(string message, params object[] args)
		{
			_logger.LogError(message, args);
		}

		public void LogError(Exception exception, string message, params object[] args)
		{
			_logger.LogError(exception, message, args);
		}

		public void LogError(EventId eventId, Exception exception, string message, params object[] args)
		{
			_logger.LogError(eventId, exception, message, args);
		}

		public void LogError(EventId eventId, string message, params object[] args)
		{
			_logger.LogError(eventId, message, args);
		}

		public void LogInformation(EventId eventId, string message, params object[] args)
		{
			_logger.LogInformation(eventId, message, args);
		}

		public void LogInformation(Exception exception, string message, params object[] args)
		{
			_logger.LogInformation(exception, message, args);
		}

		public void LogInformation(EventId eventId, Exception exception, string message, params object[] args)
		{
			_logger.LogInformation(eventId, exception, message, args);
		}

		public void LogInformation(string message, params object[] args)
		{
			_logger.LogInformation(message, args);
		}

		public void LogTrace(string message, params object[] args)
		{
			_logger.LogTrace(message, args);
		}

		public void LogTrace(Exception exception, string message, params object[] args)
		{
			_logger.LogTrace(exception, message, args);
		}

		public void LogTrace(EventId eventId, string message, params object[] args)
		{
			_logger.LogTrace(eventId, message, args);
		}

		public void LogTrace(EventId eventId, Exception exception, string message, params object[] args)
		{
			_logger.LogTrace(eventId, exception, message, args);
		}

		public void LogWarning(EventId eventId, string message, params object[] args)
		{
			_logger.LogWarning(eventId, message, args);
		}

		public void LogWarning(EventId eventId, Exception exception, string message, params object[] args)
		{
			_logger.LogWarning(eventId, exception, message, args);
		}

		public void LogWarning(string message, params object[] args)
		{
			_logger.LogWarning(message, args);
		}

		public void LogWarning(Exception exception, string message, params object[] args)
		{
			_logger.LogWarning(exception, message, args);
		}

		public void LogDebugWithSource(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string callerLocation = "")
		{
			_logger.LogDebugWithSource(message, memberName, callerLocation);
		}

		public void LogErrorWithSource(Exception ex, string message, [CallerMemberName] string memberName = "", [CallerFilePath] string callerLocation = "")
		{
			_logger.LogErrorWithSource(ex, message, memberName, callerLocation);
		}

		public void LogInfoWithSource(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string callerLocation = "")
		{
			_logger.LogInfoWithSource(message, memberName, callerLocation);
		}

		public void LogErrorWithSource(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string callerLocation = "")
		{
			_logger.LogErrorWithSource(message, memberName, callerLocation);
		}

		public void LogCriticalWithSource(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string callerLocation = "")
		{
			_logger.LogCriticalWithSource(message, memberName, callerLocation);
		}

		public void LogCriticalWithSource(Exception ex, string message, [CallerMemberName] string memberName = "", [CallerFilePath] string callerLocation = "")
		{
			_logger.LogCriticalWithSource(ex, message, memberName, callerLocation);
		}

		public void LogWarningWithSource(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string callerLocation = "")
		{
			_logger.LogWarningWithSource(message, memberName, callerLocation);
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			_logger.Log(logLevel, eventId, state, exception, formatter);
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return _logger.IsEnabled(logLevel);
		}

		public IDisposable BeginScope<TState>(TState state)
		{
			return _logger.BeginScope(state);
		}
	}

	public static IServiceCollection AddTestableLogging(this IServiceCollection serviceCollection)
	{
		return serviceCollection.AddSingleton(typeof(ITestableLogger<>), typeof(TestableLogger<>));
	}
}
