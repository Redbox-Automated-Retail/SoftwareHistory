using System.Collections.Generic;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.Campaigns;

[JsonConverter(typeof(ViewControlListConverter))]
public class ViewControlList : List<IViewControl>
{
}
