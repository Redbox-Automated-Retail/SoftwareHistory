using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Serilog;

namespace UpdateClientService.API.Services.FileSets;

public class FileSetPollRequestList
{
	public List<FileSetPollRequest> FileSetPollRequests { get; set; }

	public FileSetPollRequestList()
	{
		FileSetPollRequests = new List<FileSetPollRequest>();
	}

	public string ToShortFormat()
	{
		if (FileSetPollRequests.Count == 0)
		{
			return string.Empty;
		}
		int pos = 0;
		int count = FileSetPollRequests.Count();
		StringBuilder sb = new StringBuilder();
		FileSetPollRequests.ForEach(delegate(FileSetPollRequest item)
		{
			sb.Append($"{item.FileSetId},{item.FileSetRevisionId},{(int)item.FileSetState}");
			pos++;
			if (pos < count)
			{
				sb.Append("|");
			}
		});
		return sb.ToString();
	}

	public void FromShortFormat(string data)
	{
		try
		{
			FileSetPollRequests.Clear();
			string[] array = data.Split(new string[1] { "|" }, StringSplitOptions.RemoveEmptyEntries);
			string[] array2 = array;
			foreach (string text in array2)
			{
				string[] array3 = text.Split(new string[1] { "," }, StringSplitOptions.RemoveEmptyEntries);
				FileSetPollRequest item = new FileSetPollRequest
				{
					FileSetId = Convert.ToInt64(array3[0]),
					FileSetRevisionId = Convert.ToInt64(array3[1]),
					FileSetState = (FileSetState)Convert.ToInt32(array3[2])
				};
				FileSetPollRequests.Add(item);
			}
		}
		catch (Exception propertyValue)
		{
			Log.Error("FileSetPollRequestList - Unhandled exception.", propertyValue);
		}
	}
}
