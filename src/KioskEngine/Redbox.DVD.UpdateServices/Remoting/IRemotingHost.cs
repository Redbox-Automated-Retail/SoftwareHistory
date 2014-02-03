namespace Redbox.IPC.Framework.Remoting;

internal interface IRemotingHost
{
	string ExecuteCommand(string commandString);
}
