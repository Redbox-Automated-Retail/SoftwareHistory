using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal class VendTarget
{
	internal string ID { get; private set; }

	internal Location Location { get; private set; }

	internal bool IsStuck { get; set; }

	internal bool Vended { get; set; }

	internal GetResult GetResult { get; set; }

	internal VendTarget(string id, Location location)
	{
		ID = id;
		Location = location;
	}
}
