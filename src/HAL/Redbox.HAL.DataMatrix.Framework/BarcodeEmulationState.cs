using Redbox.HAL.Core;

namespace Redbox.HAL.DataMatrix.Framework;

public sealed class BarcodeEmulationState : EmulationStateConfig
{
	private CircularList<string> m_expectedScanResults;

	public static BarcodeEmulationState Instance => Singleton<BarcodeEmulationState>.Instance;

	public CircularList<string> ExpectedScanResults
	{
		get
		{
			if (m_expectedScanResults == null)
			{
				m_expectedScanResults = new CircularList<string>();
			}
			return m_expectedScanResults;
		}
	}

	public void Reset()
	{
		ExpectedScanResults.Clear();
	}

	protected override object GetEmulated(string name)
	{
		return null;
	}

	protected override bool ProcessOption(string name, object[] value)
	{
		if (string.Compare("ExpectedScanResults.Set", name, ignoreCase: true) == 0)
		{
			ExpectedScanResults.Clear();
			for (int i = 0; i < value.Length; i++)
			{
				string item = (string)value[i];
				ExpectedScanResults.Add(item);
			}
			return true;
		}
		return false;
	}

	private BarcodeEmulationState()
	{
		Reset();
	}
}
