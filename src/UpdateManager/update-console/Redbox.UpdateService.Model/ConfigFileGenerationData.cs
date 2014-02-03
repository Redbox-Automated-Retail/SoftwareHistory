using System;
using System.Collections.Generic;

namespace Redbox.UpdateService.Model;

internal class ConfigFileGenerationData
{
	public long OID { get; set; }

	public long ConfigFileOID { get; set; }

	public DateTime? CreatedOn { get; set; }

	public List<ConfigFileDataData> ConfigFileDataList { get; set; }
}
