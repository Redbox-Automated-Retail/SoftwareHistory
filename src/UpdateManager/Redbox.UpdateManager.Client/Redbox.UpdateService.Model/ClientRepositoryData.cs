namespace Redbox.UpdateService.Model;

internal class ClientRepositoryData
{
	public class Revlog
	{
		public string Hash;

		public long OID;
	}

	public const string EmptyHash = "0000000000000000000000000000000000000000";

	public string RepositoryName;

	public Revlog Assigned;

	public Revlog InTransit;

	public Revlog Head;

	public Revlog Staged;

	public Revlog Active;

	public ClientRepositoryData()
	{
	}

	public ClientRepositoryData(ClientRepositoryDataDTO dto)
	{
		RepositoryName = dto.R;
		InTransit = new Revlog
		{
			Hash = dto.I
		};
		Staged = new Revlog
		{
			Hash = dto.S
		};
		Active = new Revlog
		{
			Hash = dto.A
		};
		Head = new Revlog
		{
			Hash = dto.H
		};
		Assigned = new Revlog
		{
			Hash = "0000000000000000000000000000000000000000"
		};
	}
}
