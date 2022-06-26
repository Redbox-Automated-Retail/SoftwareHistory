using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.WelcomeScreen;

public class WelcomeScreenAsset
{
	public string Name { get; set; }

	public DateTime StartDate { get; set; }

	public DateTime EndDate { get; set; }

	public int? Order { get; set; }

	public int? ProductGroupId { get; set; }

	public string FileType { get; set; }

	public string FileName { get; set; }

	public string FilePath { get; set; }

	public static WelcomeScreenAsset LoadFromFile<T>(string filePath, ILogger<T> logger)
	{
		try
		{
			string fileName = Path.GetFileName(filePath);
			if (string.IsNullOrWhiteSpace(fileName))
			{
				logger.LogErrorWithSource("Invalid filename for WelcomeScreenAsset: " + filePath, "LoadFromFile");
				return null;
			}
			string extension = Path.GetExtension(fileName);
			if (string.IsNullOrWhiteSpace(extension))
			{
				logger.LogErrorWithSource("Invalid filename extension for WelcomeScreenAsset: " + fileName, "LoadFromFile");
				return null;
			}
			extension = extension.Replace(".", "").ToLower();
			string start = string.Empty;
			string end = string.Empty;
			int order = 0;
			int productGroupId = 0;
			List<string> list = Path.GetFileNameWithoutExtension(fileName).ToLower().Split('_')
				.ToList();
			string name = list.FirstOrDefault();
			list.ForEach(delegate(string value)
			{
				if (value.Contains('-'))
				{
					string[] array = value.Split('-');
					if (array.Length == 2)
					{
						if (array[0] == "start")
						{
							start = array[1];
						}
						else if (array[0] == "end")
						{
							end = array[1];
						}
						else if (array[0] == "productgroupid")
						{
							int.TryParse(array[1], out productGroupId);
						}
						else if (array[0] == "order")
						{
							int.TryParse(array[1], out order);
						}
					}
				}
			});
			if (string.IsNullOrEmpty(start) || string.IsNullOrEmpty(end))
			{
				logger.LogErrorWithSource("start or end date is missing for WelcomeScreenAsset: " + fileName, "LoadFromFile");
				return null;
			}
			DateTime.TryParseExact(start, "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.None, out var result);
			DateTime.TryParseExact(end, "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.None, out var result2);
			return new WelcomeScreenAsset
			{
				FilePath = filePath,
				FileName = fileName,
				Name = name,
				StartDate = result,
				EndDate = result2,
				FileType = extension,
				Order = order,
				ProductGroupId = productGroupId
			};
		}
		catch (Exception ex)
		{
			logger.LogErrorWithSource(ex, "Exception for WelcomeScreenAsset: " + filePath, "LoadFromFile");
			return null;
		}
	}
}
