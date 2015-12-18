using System.Collections.Generic;

namespace Redbox.Rental.Model.Profile;

public interface IProfileLuaView
{
	Dictionary<string, IProfileLuaTable> Tables { get; }
}
