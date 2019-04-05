using System.Collections.Generic;

namespace DeviceService.ComponentModel.Responses;

public class EMVCardReadModel : EncryptedCardReadModel
{
	public override bool HasChip => true;

	public IDictionary<string, string> Tags { get; set; }

	public string AID { get; set; }

	public EMVCardReadModel()
	{
		Tags = new Dictionary<string, string>();
	}

	public EMVCardReadModel(IEnumerable<Error> errors)
		: base(errors)
	{
		Tags = new Dictionary<string, string>();
	}

	public override void ObfuscateSensitiveData()
	{
		if (Tags != null)
		{
			foreach (string item in new List<string>(Tags.Keys))
			{
				Tags[item] = ObfuscationHelper.ObfuscateValue(Tags[item]);
			}
		}
		base.ObfuscateSensitiveData();
	}
}
