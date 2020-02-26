using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API;

public static class LoggerExtensions
{
	public static void LogInfoWithSource<T>(this ILogger<T> logger, string message, [CallerMemberName] string memberName = "")
	{
		string name = typeof(T).Name;
		logger.LogInformation(name + "." + memberName + " -> " + message);
	}

	public static void LogErrorWithSource<T>(this ILogger<T> logger, Exception ex, string message, [CallerMemberName] string memberName = "")
	{
		string name = typeof(T).Name;
		logger.LogError(ex, name + "." + memberName + " -> " + message);
	}
}
