using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace UpdateClientService.API;

public static class LoggerExtensions
{
	public static void LogErrorWithSource<T>(this ILogger<T> logger, Exception ex, [CallerMemberName] string memberName = "")
	{
		string name = typeof(T).Name;
		logger.LogError(name + "." + memberName + " -> ");
		logger.LogError(ex, name + "." + memberName + " -> ");
	}
}
