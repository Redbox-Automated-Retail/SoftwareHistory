namespace Redbox.UpdateService.Model;

internal class InterServerMessage
{
	public InterServerTask Task { get; set; }

	public Server Server { get; set; }

	public InterServerMessage(InterServerTask task, Server server)
	{
		Task = task;
		Server = server;
	}

	public InterServerMessage()
	{
	}
}
