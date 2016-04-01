using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal interface IQlm
{
	QlmStatus GetStatus();

	ErrorCodes Engage(bool home, IFormattedLog log);

	ErrorCodes Disengage(bool home, IFormattedLog log);

	void Drop();

	void Lift();

	bool HaltOperation();
}
