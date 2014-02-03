using System.Runtime.Remoting;
using Belikov.GenuineChannels.DirectExchange;
using Belikov.GenuineChannels.Security;

namespace Belikov.GenuineChannels.Logbook;

internal class GenuineLoggingServices
{
	public const string DefaultLoggingOptions = "C1E1M2I3S1B2T1X1H1A1D2V1L0N1";

	public const string NameOfLoggingSES = "/GenuineChannels/__LoggingSES";

	public const string RemoteLogServiceObjectUri = "__GenuineChannels__RemoteLogServiceObjectUri.rem";

	public const string LoggingSESSPName = "/__GC/Logging/SES";

	private static BinaryLogWriter _logger = null;

	private static object _accessToLocalMembers = new object();

	private static RemoteLogService _remoteLogService;

	private static MemoryWritingStream _memoryWritingStream = null;

	private static BinaryLogWriter _memoryWritingWriter = null;

	public static BinaryLogWriter BinaryLogWriter
	{
		get
		{
			lock (_accessToLocalMembers)
			{
				return _logger;
			}
		}
		set
		{
			lock (_accessToLocalMembers)
			{
				_logger = value;
			}
		}
	}

	public static object SyncRoot => _accessToLocalMembers;

	public static RemoteLogService RemoteLogService
	{
		get
		{
			lock (_accessToLocalMembers)
			{
				return _remoteLogService;
			}
		}
	}

	private GenuineLoggingServices()
	{
	}

	public static void StopLogging()
	{
		lock (_accessToLocalMembers)
		{
			BinaryLogWriter = null;
			if (_memoryWritingWriter != null)
			{
				RemotingServices.Disconnect(_remoteLogService);
				_remoteLogService.StopLogging();
				_remoteLogService = null;
				_memoryWritingStream = null;
				_memoryWritingWriter = null;
			}
		}
	}

	public static void SetUpLoggingToMemory(int maximumMemorySize, string logOptions)
	{
		lock (_accessToLocalMembers)
		{
			if (_memoryWritingWriter == null)
			{
				_memoryWritingStream = new MemoryWritingStream(maximumMemorySize);
				_memoryWritingWriter = new BinaryLogWriter(_memoryWritingStream);
				SetUpLoggingOptionsFromString(_memoryWritingWriter, "C1E1M2I3S1B2T1X1H1A1D2V1L0N1");
				SetUpLoggingOptionsFromString(_memoryWritingWriter, logOptions);
				_remoteLogService = new RemoteLogService(_memoryWritingStream);
				RemotingServices.Marshal(_remoteLogService, "__GenuineChannels__RemoteLogServiceObjectUri.rem");
				DirectExchangeManager.RegisterGlobalServerService("__GenuineChannels__RemoteLogServiceObjectUri.rem", _remoteLogService);
				SecuritySessionServices.SetGlobalKey("/__GC/Logging/SES", new KeyProvider_SelfEstablishingSymmetric());
			}
			BinaryLogWriter = _memoryWritingWriter;
		}
	}

	public static void SetUpLoggingToFile(string baseFileName, string logOptions)
	{
		SetUpLoggingToFile(baseFileName, logOptions, addSuffixToBaseFileName: true);
	}

	public static void SetUpLoggingToFile(string baseFileName, string logOptions, bool addSuffixToBaseFileName)
	{
		lock (_accessToLocalMembers)
		{
			BinaryLogWriter binaryLogWriter = new BinaryLogWriter(new FileWritingStream(baseFileName, addSuffixToBaseFileName));
			SetUpLoggingOptionsFromString(binaryLogWriter, "C1E1M2I3S1B2T1X1H1A1D2V1L0N1");
			SetUpLoggingOptionsFromString(binaryLogWriter, logOptions);
			BinaryLogWriter = binaryLogWriter;
		}
	}

	public static void SetUpLoggingOptionsFromString(BinaryLogWriter binaryLogWriter, string logOptions)
	{
		if (logOptions == null || logOptions.Length <= 0)
		{
			return;
		}
		for (int i = 0; i < logOptions.Length; i += 2)
		{
			if (logOptions.Length - i >= 2 && char.IsDigit(logOptions[i + 1]))
			{
				int value = (short)logOptions[i + 1] - 48;
				switch (char.ToUpper(logOptions[i]))
				{
				case 'C':
					binaryLogWriter[LogCategory.Connection] = value;
					break;
				case 'E':
					binaryLogWriter[LogCategory.ChannelEvent] = value;
					break;
				case 'M':
					binaryLogWriter[LogCategory.MessageProcessing] = value;
					break;
				case 'I':
					binaryLogWriter[LogCategory.ImplementationWarning] = value;
					break;
				case 'S':
					binaryLogWriter[LogCategory.Security] = value;
					break;
				case 'B':
					binaryLogWriter[LogCategory.BroadcastEngine] = value;
					break;
				case 'T':
					binaryLogWriter[LogCategory.Transport] = value;
					break;
				case 'X':
					binaryLogWriter[LogCategory.DXM] = value;
					break;
				case 'H':
					binaryLogWriter[LogCategory.HostInformation] = value;
					break;
				case 'A':
					binaryLogWriter[LogCategory.AcceptingConnection] = value;
					break;
				case 'D':
					binaryLogWriter[LogCategory.Debugging] = value;
					break;
				case 'V':
					binaryLogWriter[LogCategory.Version] = value;
					break;
				case 'L':
					binaryLogWriter[LogCategory.LowLevelTransport] = value;
					break;
				case 'N':
					binaryLogWriter[LogCategory.StatisticCounters] = value;
					break;
				}
			}
		}
	}
}
