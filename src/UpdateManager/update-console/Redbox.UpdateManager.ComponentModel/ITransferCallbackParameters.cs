using System.Collections.Generic;

namespace Redbox.UpdateManager.ComponentModel;

internal interface ITransferCallbackParameters
{
	string Path { get; set; }

	string Executable { get; set; }

	List<string> Arguments { get; set; }
}
