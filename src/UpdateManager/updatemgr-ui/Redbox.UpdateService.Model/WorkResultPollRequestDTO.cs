using System.Collections.Generic;

namespace Redbox.UpdateService.Model;

internal class WorkResultPollRequestDTO
{
	public string StoreNumber;

	public long ScriptID;

	public Dictionary<object, object> Results;
}
