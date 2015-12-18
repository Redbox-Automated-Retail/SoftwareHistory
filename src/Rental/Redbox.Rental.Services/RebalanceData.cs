using System.Collections.Generic;

namespace Redbox.Rental.Services;

public class RebalanceData
{
	public RebalanceVersion Version;

	public string LastVersionHash;

	public string VersionHash;

	public bool IsVersionNew;

	public List<RebalanceIn> In;

	public string LastInHash;

	public string InHash;

	public bool IsInNew;

	public List<RebalanceOut> Out;

	public string LastOutHash;

	public string OutHash;

	public bool IsOutNew;

	public List<RebalanceUnthin> UnThin;

	public string LastUnThinHash;

	public string UnThinHash;

	public bool IsUnThinNew;
}
