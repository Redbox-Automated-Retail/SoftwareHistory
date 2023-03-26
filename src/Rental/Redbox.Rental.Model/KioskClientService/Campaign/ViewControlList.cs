using System.Collections.Generic;
using Newtonsoft.Json;

namespace Redbox.Rental.Model.KioskClientService.Campaign;

[JsonConverter(typeof(ViewControlListConverter))]
public class ViewControlList : List<IViewControl>
{
}
