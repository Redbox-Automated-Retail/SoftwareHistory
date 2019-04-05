using System;
using System.IO;
using System.Linq;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Analytics;

namespace Redbox.Rental.Services.Analytics;

public class AnalyticsService : IAnalyticsService
{
	private string _analyticsSessionsPath = "AnalyticsSessions";

	public static AnalyticsService Instance => Singleton<AnalyticsService>.Instance;

	public void Initialize()
	{
		try
		{
			IEngineApplication service = ServiceLocator.Instance.GetService<IEngineApplication>();
			if (service == null)
			{
				return;
			}
			string path = Path.Combine(service.DataPath, _analyticsSessionsPath);
			if (!Directory.Exists(path))
			{
				return;
			}
			foreach (string item in (from fp in Directory.GetFiles(path, "*.xml", SearchOption.TopDirectoryOnly)
				where File.GetCreationTime(fp) < DateTime.Now.AddDays(-7.0)
				select fp).ToList())
			{
				if (File.Exists(item))
				{
					LogHelper.Instance.Log("AnalyticsService.Initialize: deleting file {0}.", item);
					File.Delete(item);
				}
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in AnalyticsService.Initialize.", e);
		}
	}

	public void WriteSessionToFile(string sessionId, string xml)
	{
		try
		{
			IEngineApplication service = ServiceLocator.Instance.GetService<IEngineApplication>();
			if (service != null)
			{
				string text = Path.Combine(service.DataPath, _analyticsSessionsPath);
				if (!Directory.Exists(text))
				{
					LogHelper.Instance.Log("AnalyticsService.WriteSessionToFile: {0} not found, creating directory.", text);
					Directory.CreateDirectory(text);
				}
				string text2 = Path.Combine(text, $"{sessionId}.xml");
				LogHelper.Instance.Log("AnalyticsService.WriteSessionToFile: writing file {0}.", text2);
				File.WriteAllText(text2, xml);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in AnalyticsService.WriteSessionToFile.", e);
		}
	}

	private AnalyticsService()
	{
	}
}
