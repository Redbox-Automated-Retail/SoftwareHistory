using System.Collections.Generic;
using System.Linq;

namespace Redbox.UpdateService.Model;

internal class RepositoryDiffWrapper
{
	public string O { get; set; }

	public string I { get; set; }

	public List<Dictionary<string, string>> U { get; set; }

	public List<ClientRepositoryDataDTO> A { get; set; }

	public List<string> D { get; set; }

	public RepositoryDiffWrapper Copy()
	{
		RepositoryDiffWrapper repositoryDiffWrapper = new RepositoryDiffWrapper();
		repositoryDiffWrapper.O = string.Copy(O);
		repositoryDiffWrapper.I = string.Copy(I);
		repositoryDiffWrapper.A = A.Copy();
		repositoryDiffWrapper.D = D.ToList();
		repositoryDiffWrapper.U = U.Select((Dictionary<string, string> each) => new Dictionary<string, string>(each)).ToList();
		return repositoryDiffWrapper;
	}
}
