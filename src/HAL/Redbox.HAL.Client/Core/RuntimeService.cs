using System.IO;

namespace Redbox.HAL.Core;

internal class RuntimeService
{
	private string m_assemblyDirectory;

	public static RuntimeService Instance => Singleton<RuntimeService>.Instance;

	public string AssemblyDirectory
	{
		get
		{
			if (string.IsNullOrEmpty(m_assemblyDirectory))
			{
				m_assemblyDirectory = Path.GetDirectoryName(typeof(RuntimeService).Assembly.Location);
			}
			return m_assemblyDirectory;
		}
	}

	public string RuntimePath(string fileName)
	{
		return Path.Combine(AssemblyDirectory, fileName);
	}

	private RuntimeService()
	{
	}
}
