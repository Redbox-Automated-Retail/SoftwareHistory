using System.Collections.Generic;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.DirectShow.Frame.Plugin;

internal sealed class Service : ICameraPlugin
{
	private ICameraPlugin m_plugin;

	public bool IsRunning => m_plugin.IsRunning;

	public bool SupportsReset => m_plugin.SupportsReset;

	public bool Start()
	{
		return m_plugin.Start();
	}

	public void Snap(string file)
	{
		m_plugin.Snap(file);
	}

	public bool Stop()
	{
		return m_plugin.Stop();
	}

	public void InitWithProperties(IDictionary<string, object> properties)
	{
		if (m_plugin == null)
		{
			Implementors implementors = Implementors.Legacy;
			if (properties.ContainsKey("Implementor"))
			{
				implementors = Enum<Implementors>.ParseIgnoringCase((string)properties["Implementor"], Implementors.Legacy);
			}
			if (implementors == Implementors.Slim)
			{
				m_plugin = new SlimFrameService();
			}
			else
			{
				m_plugin = new LegacyService();
			}
			LogHelper.Instance.Log("[DirectShowPlugin {0}] Implementing service is {1}", typeof(Service).Assembly.GetName().Version, implementors.ToString());
			m_plugin.InitWithProperties(properties);
		}
	}
}
