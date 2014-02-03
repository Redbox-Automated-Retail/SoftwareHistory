namespace Redbox.UpdateManager.ComponentModel;

internal interface IPollService
{
	ErrorList ServerPoll();

	ErrorList ServerPoll(IPoll poll);
}
