using System.Text;
using Redbox.Core;

namespace Redbox.UpdateService.Client;

internal class Work
{
	public long ID { get; set; }

	public string Command { get; set; }

	public string Name { get; set; }

	public string GetScriptText()
	{
		return Encoding.Unicode.GetString(StringExtensions.Base64ToBytes(Command));
	}
}
