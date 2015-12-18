namespace Redbox.UpdateManager.ComponentModel;

internal interface IPollService
{
	ErrorList ServerPoll();

	ErrorList ServerPoll(bool suppressHealthUpdate);

	ErrorList ServerPoll(IPoll poll);
}
